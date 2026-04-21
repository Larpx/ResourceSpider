using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface ICollectionResultService
{
    Task<CollectionResultDto?> GetByIdAsync(string resultId);
    Task<CollectionResultListResponse> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize);
    Task<CollectionResultListResponse> GetByExpressionIdAsync(string expressionId, int pageIndex, int pageSize);
    Task StoreResultsAsync(string taskId, string? expressionId, string agentId, List<CollectionResultItemDto> results);
    Task<int> GetResultCountByTaskIdAsync(string taskId);
}

public class CollectionResultService : ICollectionResultService
{
    private readonly ICollectionResultRepository _resultRepository;
    private readonly IExpressionRepository _expressionRepository;
    private readonly ILogger<CollectionResultService> _logger;

    public CollectionResultService(
        ICollectionResultRepository resultRepository,
        IExpressionRepository expressionRepository,
        ILogger<CollectionResultService> logger)
    {
        _resultRepository = resultRepository;
        _expressionRepository = expressionRepository;
        _logger = logger;
    }

    public async Task<CollectionResultDto?> GetByIdAsync(string resultId)
    {
        var entity = await _resultRepository.GetByIdAsync(resultId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<CollectionResultListResponse> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize)
    {
        var results = await _resultRepository.GetByTaskIdAsync(taskId, pageIndex, pageSize);
        var total = await _resultRepository.CountByTaskIdAsync(taskId);
        return new CollectionResultListResponse(
            results.Select(MapToDto).ToList(),
            (int)total, pageIndex, pageSize);
    }

    public async Task<CollectionResultListResponse> GetByExpressionIdAsync(string expressionId, int pageIndex, int pageSize)
    {
        var results = await _resultRepository.GetByExpressionIdAsync(expressionId, pageIndex, pageSize);
        var total = await _resultRepository.CountByExpressionIdAsync(expressionId);
        return new CollectionResultListResponse(
            results.Select(MapToDto).ToList(),
            (int)total, pageIndex, pageSize);
    }

    public async Task StoreResultsAsync(string taskId, string? expressionId, string agentId, List<CollectionResultItemDto> results)
    {
        var entities = results.Select(r => new CollectionResultEntity
        {
            ResultId = r.ResultId ?? Guid.NewGuid().ToString("N"),
            TaskId = taskId,
            ExpressionId = expressionId,
            AgentId = agentId,
            SourceUrl = r.SourceUrl,
            Fields = System.Text.Json.JsonSerializer.Serialize(r.Fields),
            FieldExpressionMap = r.FieldExpressionMap != null
                ? System.Text.Json.JsonSerializer.Serialize(r.FieldExpressionMap)
                : null,
            CollectedAt = r.CollectedAt ?? DateTime.UtcNow
        }).ToList();

        await _resultRepository.AddRangeAsync(entities);
        _logger.LogInformation(
            "Stored {Count} results for task {TaskId}, expression {ExpressionId}",
            entities.Count, taskId, expressionId);
    }

    public async Task<int> GetResultCountByTaskIdAsync(string taskId)
    {
        return (int)await _resultRepository.CountByTaskIdAsync(taskId);
    }

    private static CollectionResultDto MapToDto(CollectionResultEntity entity)
    {
        var fields = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(entity.Fields)
            ?? new Dictionary<string, object?>();
        var fieldExpressionMap = entity.FieldExpressionMap != null
            ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(entity.FieldExpressionMap)
            : new Dictionary<string, string>();

        return new CollectionResultDto(
            entity.ResultId,
            entity.TaskId,
            entity.ExpressionId ?? string.Empty,
            entity.AgentId ?? string.Empty,
            entity.SourceUrl ?? string.Empty,
            fields,
            fieldExpressionMap ?? new Dictionary<string, string>(),
            entity.CollectedAt,
            entity.CreatedAt
        );
    }
}
