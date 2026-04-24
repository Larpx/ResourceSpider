using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

/// <summary>
/// 采集结果服务接口，提供采集结果的存储、查询和统计功能
/// </summary>
public interface ICollectionResultService
{
    /// <summary>
    /// 根据结果标识获取单条采集结果
    /// </summary>
    /// <param name="resultId">结果唯一标识</param>
    /// <returns>采集结果 DTO，若不存在返回 null</returns>
    Task<CollectionResultDto?> GetByIdAsync(string resultId);

    /// <summary>
    /// 根据任务标识分页查询采集结果
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>采集结果列表响应</returns>
    Task<CollectionResultListResponse> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize);

    /// <summary>
    /// 根据表达式标识分页查询采集结果
    /// </summary>
    /// <param name="expressionId">表达式唯一标识</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>采集结果列表响应</returns>
    Task<CollectionResultListResponse> GetByExpressionIdAsync(string expressionId, int pageIndex, int pageSize);

    /// <summary>
    /// 批量存储 Agent 上报的采集结果数据
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <param name="expressionId">关联的表达式标识</param>
    /// <param name="agentId">上报结果的 Agent 标识</param>
    /// <param name="results">采集结果项列表</param>
    Task StoreResultsAsync(string taskId, string? expressionId, string agentId, List<CollectionResultItemDto> results);

    /// <summary>
    /// 获取指定任务的采集结果总数
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <returns>结果总数</returns>
    Task<int> GetResultCountByTaskIdAsync(string taskId);
}

/// <summary>
/// 采集结果服务实现，管理采集结果的持久化存储、查询和统计
/// </summary>
public class CollectionResultService : ICollectionResultService
{
    private readonly ICollectionResultRepository _resultRepository;
    private readonly IExpressionRepository _expressionRepository;
    private readonly IStepResourceRepository _stepResourceRepository;
    private readonly IStorageStrategyService _storageStrategyService;
    private readonly ILogger<CollectionResultService> _logger;

    public CollectionResultService(
        ICollectionResultRepository resultRepository,
        IExpressionRepository expressionRepository,
        IStepResourceRepository stepResourceRepository,
        IStorageStrategyService storageStrategyService,
        ILogger<CollectionResultService> logger)
    {
        _resultRepository = resultRepository;
        _expressionRepository = expressionRepository;
        _stepResourceRepository = stepResourceRepository;
        _storageStrategyService = storageStrategyService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CollectionResultDto?> GetByIdAsync(string resultId)
    {
        var entity = await _resultRepository.GetByIdAsync(resultId);
        return entity != null ? MapToDto(entity) : null;
    }

    /// <inheritdoc />
    public async Task<CollectionResultListResponse> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize)
    {
        var results = await _resultRepository.GetByTaskIdAsync(taskId, pageIndex, pageSize);
        var total = await _resultRepository.CountByTaskIdAsync(taskId);
        return new CollectionResultListResponse(
            results.Select(MapToDto).ToList(),
            (int)total, pageIndex, pageSize);
    }

    /// <inheritdoc />
    public async Task<CollectionResultListResponse> GetByExpressionIdAsync(string expressionId, int pageIndex, int pageSize)
    {
        var results = await _resultRepository.GetByExpressionIdAsync(expressionId, pageIndex, pageSize);
        var total = await _resultRepository.CountByExpressionIdAsync(expressionId);
        return new CollectionResultListResponse(
            results.Select(MapToDto).ToList(),
            (int)total, pageIndex, pageSize);
    }

    /// <inheritdoc />
    public async Task StoreResultsAsync(string taskId, string? expressionId, string agentId, List<CollectionResultItemDto> results)
    {
        var storageEngine = _storageStrategyService.GetCurrentEngine().ToString();

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
            "Stored {Count} results for task {TaskId}, expression {ExpressionId}",
            entities.Count, taskId, expressionId);
    }

    /// <inheritdoc />
    public async Task<int> GetResultCountByTaskIdAsync(string taskId)
    {
        return (int)await _resultRepository.CountByTaskIdAsync(taskId);
    }

    /// <summary>
    /// 将采集结果实体映射为采集结果 DTO
    /// </summary>
    /// <param name="entity">采集结果实体</param>
    /// <returns>采集结果 DTO</returns>
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
            entity.CreatedAt
        );
    }

    /// <summary>
    /// 标准化字段，将输入的领域模型转换为一致的存储格式
    /// </summary>
    /// <param name="fields">输入的字段字典</param>
    /// <returns>标准化后的字段字典</returns>
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

    /// <summary>
    /// 标准化 JSON 元素的值，将其转换为可存储的基本类型
    /// </summary>
    /// <param name="element">输入的 JSON 元素</param>
    /// <returns>标准化后的元素值</returns>
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

    /// <summary>
    /// 计算给定负载的 SHA256 哈希值
    /// </summary>
    /// <param name="payload">输入的字符串负载</param>
    /// <returns>计算得到的哈希值（十六进制字符串）</returns>
    private static string ComputeHash(string payload)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
