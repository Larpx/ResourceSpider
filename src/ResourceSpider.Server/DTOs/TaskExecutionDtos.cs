namespace Larpx.PersonalTools.ResourceSpider.Server.DTOs;

/// <summary>
/// 任务执行记录数据传输对象
/// </summary>
/// <param name="ExecutionId">执行记录 ID</param>
/// <param name="TaskId">关联任务 ID</param>
/// <param name="AgentId">执行代理 ID</param>
/// <param name="Status">执行状态</param>
/// <param name="ConfigSnapshot">执行时的配置快照 JSON</param>
/// <param name="StartedAt">开始时间</param>
/// <param name="CompletedAt">完成时间</param>
/// <param name="TotalPages">总页面数</param>
/// <param name="SuccessCount">成功数</param>
/// <param name="FailCount">失败数</param>
/// <param name="ErrorMessage">错误信息</param>
/// <param name="CreatedAt">记录创建时间</param>
public record TaskExecutionDto(
    string ExecutionId,
    string TaskId,
    string AgentId,
    int Status,
    string? ConfigSnapshot,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    int TotalPages,
    int SuccessCount,
    int FailCount,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? StartTime = null,
    DateTime? EndTime = null,
    long? Duration = null
);

/// <summary>
/// 任务执行记录列表响应，包含分页信息
/// </summary>
/// <param name="Executions">执行记录列表</param>
/// <param name="Total">总数</param>
/// <param name="PageIndex">当前页码</param>
/// <param name="PageSize">每页数量</param>
public record TaskExecutionListResponse(
    List<TaskExecutionDto> Executions,
    int Total,
    int PageIndex,
    int PageSize
);
