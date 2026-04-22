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
    /// <summary>
    /// 采集结果数据仓库，用于结果实体的持久化操作
    /// </summary>
    private readonly ICollectionResultRepository _resultRepository;

    /// <summary>
    /// 表达式数据仓库，用于表达式关联查询
    /// </summary>
    private readonly IExpressionRepository _expressionRepository;

    /// <summary>
    /// 日志记录器，用于记录采集结果操作相关事件
    /// </summary>
    private readonly ILogger<CollectionResultService> _logger;

    /// <summary>
    /// 初始化采集结果服务实例
    /// </summary>
    /// <param name="resultRepository">采集结果数据仓库</param>
    /// <param name="expressionRepository">表达式数据仓库</param>
    /// <param name="logger">日志记录器</param>
    public CollectionResultService(
        ICollectionResultRepository resultRepository,
        IExpressionRepository expressionRepository,
        ILogger<CollectionResultService> logger)
    {
        _resultRepository = resultRepository;
        _expressionRepository = expressionRepository;
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
