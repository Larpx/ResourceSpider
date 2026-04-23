using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface ICollectionResultService
{
    Task<CollectionResultDto?> GetByIdAsync(string resultId);
    Task<CollectionResultListResponse> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize, string? keyword = null);
    Task<CollectionResultListResponse> GetByExpressionIdAsync(string expressionId, int pageIndex, int pageSize);
    Task StoreResultsAsync(string taskId, string? expressionId, string agentId, List<CollectionResultItemDto> results);
    Task<int> GetResultCountByTaskIdAsync(string taskId);
}

public class CollectionResultService : ICollectionResultService
{
    private readonly ICollectionResultRepository _mysqlResultRepository;
    private readonly IPostgreCollectionResultRepository _postgreResultRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IStepResourceRepository _stepResourceRepository;
    private readonly IStorageStrategyService _storageStrategyService;
    private readonly IPostgreSqlResultStorageFeatureService _postgreFeatureService;
    private readonly ILogger<CollectionResultService> _logger;

    public CollectionResultService(
        ICollectionResultRepository mysqlResultRepository,
        IPostgreCollectionResultRepository postgreResultRepository,
        ITaskRepository taskRepository,
        IStepResourceRepository stepResourceRepository,
        IStorageStrategyService storageStrategyService,
        IPostgreSqlResultStorageFeatureService postgreFeatureService,
        ILogger<CollectionResultService> logger)
    {
        _mysqlResultRepository = mysqlResultRepository;
        _postgreResultRepository = postgreResultRepository;
        _taskRepository = taskRepository;
        _stepResourceRepository = stepResourceRepository;
        _storageStrategyService = storageStrategyService;
        _postgreFeatureService = postgreFeatureService;
        _logger = logger;
    }

    public async Task<CollectionResultDto?> GetByIdAsync(string resultId)
    {
        var mysqlEntity = await _mysqlResultRepository.GetByIdAsync(resultId);
        if (mysqlEntity != null)
        {
            return MapToDto(mysqlEntity);
        }

        if (_postgreFeatureService.IsConfigured && _postgreFeatureService.IsConnected)
        {
            var postgreEntity = await _postgreResultRepository.GetByIdAsync(resultId);
            if (postgreEntity != null)
            {
                return MapToDto(postgreEntity);
            }
        }

        return null;
    }

    public async Task<CollectionResultListResponse> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize, string? keyword = null)
    {
        var repository = await ResolveReadRepositoryByTaskAsync(taskId);
        var normalizedKeyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();

        if (string.IsNullOrWhiteSpace(normalizedKeyword))
        {
            var results = await repository.GetByTaskIdAsync(taskId, pageIndex, pageSize);
            var total = await repository.CountByTaskIdAsync(taskId);

            return new CollectionResultListResponse(
                results.Select(MapToDto).ToList(),
                (int)total,
                pageIndex,
                pageSize);
        }

        var allResults = await repository.GetAllByTaskIdAsync(taskId);
        var filtered = allResults
            .Where(result => MatchesKeyword(result, normalizedKeyword))
            .ToList();

        var paged = filtered
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        return new CollectionResultListResponse(paged, filtered.Count, pageIndex, pageSize);
    }

    public async Task<CollectionResultListResponse> GetByExpressionIdAsync(string expressionId, int pageIndex, int pageSize)
    {
        var results = await _mysqlResultRepository.GetByExpressionIdAsync(expressionId, pageIndex, pageSize);
        var total = await _mysqlResultRepository.CountByExpressionIdAsync(expressionId);
        return new CollectionResultListResponse(
            results.Select(MapToDto).ToList(),
            (int)total,
            pageIndex,
            pageSize);
    }

    public async Task StoreResultsAsync(string taskId, string? expressionId, string agentId, List<CollectionResultItemDto> results)
    {
        var storageEngine = _storageStrategyService.GetCurrentEngineName();

        var entities = results.Select(r => new CollectionResultEntity
        {
            ResultId = r.ResultId ?? Guid.NewGuid().ToString("N"),
            TaskId = taskId,
            StepId = r.StepId,
            ExpressionId = expressionId,
            AgentId = agentId,
            SourceUrl = r.SourceUrl,
            Fields = JsonSerializer.Serialize(NormalizeFields(r.Fields)),
            FieldExpressionMap = r.FieldExpressionMap != null
                ? JsonSerializer.Serialize(r.FieldExpressionMap)
                : null,
            StorageEngine = storageEngine,
            CollectedAt = r.CollectedAt ?? DateTime.UtcNow
        }).ToList();

        await _storageStrategyService.StoreResultsAsync(entities);

        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task != null && !string.Equals(task.ResultStorageEngine, storageEngine, StringComparison.OrdinalIgnoreCase))
        {
            task.ResultStorageEngine = storageEngine;
            await _taskRepository.UpdateAsync(task);
        }

        var stepResources = entities
            .Where(e => !string.IsNullOrWhiteSpace(e.StepId))
            .Select(e => new StepResourceEntity
            {
                ResourceId = Guid.NewGuid().ToString("N"),
                TaskId = e.TaskId,
                StepId = e.StepId!,
                SourceStepId = e.StepId,
                ResourceType = "CollectionResult",
                Payload = e.Fields,
                ContentHash = ComputeHash(e.Fields),
                SourceUrl = e.SourceUrl,
                Status = 0
            })
            .ToList();

        await _stepResourceRepository.AddRangeAsync(stepResources);

        _logger.LogInformation(
            "Stored {Count} results for task {TaskId}, expression {ExpressionId}, storage {StorageEngine}",
            entities.Count, taskId, expressionId, storageEngine);
    }

    public async Task<int> GetResultCountByTaskIdAsync(string taskId)
    {
        var repository = await ResolveReadRepositoryByTaskAsync(taskId);
        return (int)await repository.CountByTaskIdAsync(taskId);
    }

    private async Task<ICollectionResultReadRepository> ResolveReadRepositoryByTaskAsync(string taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        var usePostgre = string.Equals(task?.ResultStorageEngine, "PostgreSQL", StringComparison.OrdinalIgnoreCase)
                         && _postgreFeatureService.IsConfigured
                         && _postgreFeatureService.IsConnected;

        return usePostgre
            ? new PostgreCollectionResultReadAdapter(_postgreResultRepository)
            : new MySqlCollectionResultReadAdapter(_mysqlResultRepository);
    }

    private static CollectionResultDto MapToDto(CollectionResultEntity entity)
    {
        var fields = JsonSerializer.Deserialize<Dictionary<string, object?>>(entity.Fields)
            ?? new Dictionary<string, object?>();
        var fieldExpressionMap = entity.FieldExpressionMap != null
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(entity.FieldExpressionMap)
            : new Dictionary<string, string>();

        return new CollectionResultDto(
            entity.ResultId,
            entity.TaskId,
            entity.ExpressionId ?? string.Empty,
            entity.AgentId ?? string.Empty,
            entity.SourceUrl ?? string.Empty,
            fields,
            fieldExpressionMap ?? new Dictionary<string, string>(),
            entity.StepId,
            entity.CollectedAt,
            entity.CreatedAt,
            entity.StorageEngine
        );
    }

    private static Dictionary<string, object?> NormalizeFields(Dictionary<string, object?> fields)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in fields)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            result[key.Trim()] = value switch
            {
                null => null,
                JsonElement el => NormalizeJsonElement(el),
                _ => value
            };
        }

        return result;
    }

    private static object? NormalizeJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.TryGetDecimal(out var d) ? d : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Object => element.ToString(),
            JsonValueKind.Array => element.ToString(),
            _ => element.ToString()
        };
    }

    private static string ComputeHash(string payload)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static bool MatchesKeyword(CollectionResultEntity entity, string keyword)
    {
        if (!string.IsNullOrWhiteSpace(entity.ResultId) && entity.ResultId.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(entity.SourceUrl) && entity.SourceUrl.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(entity.Fields))
        {
            return false;
        }

        try
        {
            var fields = JsonSerializer.Deserialize<Dictionary<string, object?>>(entity.Fields);
            if (fields == null)
            {
                return entity.Fields.Contains(keyword, StringComparison.OrdinalIgnoreCase);
            }

            foreach (var pair in fields)
            {
                if (pair.Key.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (pair.Value?.ToString()?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return true;
                }
            }
        }
        catch
        {
            return entity.Fields.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private interface ICollectionResultReadRepository
    {
        Task<List<CollectionResultEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize);
        Task<List<CollectionResultEntity>> GetAllByTaskIdAsync(string taskId);
        Task<long> CountByTaskIdAsync(string taskId);
    }

    private sealed class MySqlCollectionResultReadAdapter : ICollectionResultReadRepository
    {
        private readonly ICollectionResultRepository _repository;

        public MySqlCollectionResultReadAdapter(ICollectionResultRepository repository)
        {
            _repository = repository;
        }

        public Task<List<CollectionResultEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize)
            => _repository.GetByTaskIdAsync(taskId, pageIndex, pageSize);

        public Task<List<CollectionResultEntity>> GetAllByTaskIdAsync(string taskId)
            => _repository.GetAllByTaskIdAsync(taskId);

        public Task<long> CountByTaskIdAsync(string taskId)
            => _repository.CountByTaskIdAsync(taskId);
    }

    private sealed class PostgreCollectionResultReadAdapter : ICollectionResultReadRepository
    {
        private readonly IPostgreCollectionResultRepository _repository;

        public PostgreCollectionResultReadAdapter(IPostgreCollectionResultRepository repository)
        {
            _repository = repository;
        }

        public Task<List<CollectionResultEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize)
            => _repository.GetByTaskIdAsync(taskId, pageIndex, pageSize);

        public Task<List<CollectionResultEntity>> GetAllByTaskIdAsync(string taskId)
            => _repository.GetAllByTaskIdAsync(taskId);

        public Task<long> CountByTaskIdAsync(string taskId)
            => _repository.CountByTaskIdAsync(taskId);
    }
}
