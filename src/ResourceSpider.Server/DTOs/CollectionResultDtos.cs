namespace ResourceSpider.Server.DTOs;

/// <summary>
/// 采集结果数据传输对象
/// </summary>
/// <param name="ResultId">结果 ID</param>
/// <param name="TaskId">关联任务 ID</param>
/// <param name="ExpressionId">关联表达式 ID</param>
/// <param name="AgentId">采集代理 ID</param>
/// <param name="SourceUrl">来源 URL</param>
/// <param name="Fields">提取的字段数据</param>
/// <param name="FieldExpressionMap">字段与表达式的映射关系</param>
/// <param name="CollectedAt">采集时间</param>
/// <param name="CreatedAt">记录创建时间</param>
public record CollectionResultDto(
    string ResultId,
    string TaskId,
    string ExpressionId,
    string AgentId,
    string SourceUrl,
    Dictionary<string, object?> Fields,
    Dictionary<string, string> FieldExpressionMap,
    DateTime? CollectedAt,
    DateTime CreatedAt
);

/// <summary>
/// 采集结果列表响应，包含分页信息
/// </summary>
/// <param name="Results">结果列表</param>
/// <param name="Total">总数</param>
/// <param name="PageIndex">当前页码</param>
/// <param name="PageSize">每页数量</param>
public record CollectionResultListResponse(
    List<CollectionResultDto> Results,
    int Total,
    int PageIndex,
    int PageSize
);

/// <summary>
/// 采集结果项数据传输对象，用于代理提交结果
/// </summary>
/// <param name="ResultId">结果 ID，可选</param>
/// <param name="SourceUrl">来源 URL，可选</param>
/// <param name="Fields">提取的字段数据</param>
/// <param name="FieldExpressionMap">字段与表达式的映射关系，可选</param>
/// <param name="CollectedAt">采集时间，可选</param>
public record CollectionResultItemDto(
    string? ResultId,
    string? SourceUrl,
    Dictionary<string, object?> Fields,
    Dictionary<string, string>? FieldExpressionMap,
    DateTime? CollectedAt
);

/// <summary>
/// 代理提交采集结果请求
/// </summary>
/// <param name="AgentId">代理 ID</param>
/// <param name="AgentToken">代理认证令牌</param>
/// <param name="TaskId">关联任务 ID</param>
/// <param name="ExpressionId">关联表达式 ID，可选</param>
/// <param name="Results">采集结果列表</param>
public record StoreCollectionResultsRequest(
    string AgentId,
    string AgentToken,
    string TaskId,
    string? ExpressionId,
    List<CollectionResultItemDto> Results
);

/// <summary>
/// 代理拉取表达式请求
/// </summary>
/// <param name="AgentId">代理 ID</param>
/// <param name="AgentToken">代理认证令牌</param>
/// <param name="ExpressionId">表达式 ID，可选</param>
public record PullExpressionRequest(
    [System.ComponentModel.DataAnnotations.Required] string AgentId,
    [System.ComponentModel.DataAnnotations.Required] string AgentToken,
    string? ExpressionId
);
