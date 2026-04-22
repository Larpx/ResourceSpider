namespace ResourceSpider.Server.DTOs;

/// <summary>
/// 代理统计数据传输对象
/// </summary>
/// <param name="AgentId">代理 ID</param>
/// <param name="AgentName">代理名称</param>
/// <param name="Status">代理状态</param>
/// <param name="TotalTasks">总任务数</param>
/// <param name="CompletedTasks">已完成任务数</param>
/// <param name="FailedTasks">失败任务数</param>
/// <param name="AvgDuration">平均执行时长（毫秒）</param>
/// <param name="LastHeartbeat">最后心跳时间</param>
public record AgentStatisticsDto(
    string AgentId,
    string AgentName,
    int Status,
    int TotalTasks,
    int CompletedTasks,
    int FailedTasks,
    decimal? AvgDuration,
    DateTime? LastHeartbeat
);

/// <summary>
/// 任务统计数据传输对象
/// </summary>
/// <param name="TaskId">任务 ID</param>
/// <param name="TaskName">任务名称</param>
/// <param name="TotalRequests">总请求数</param>
/// <param name="SuccessRequests">成功请求数</param>
/// <param name="FailedRequests">失败请求数</param>
/// <param name="Progress">执行进度百分比</param>
/// <param name="StartTime">开始时间</param>
/// <param name="EndTime">结束时间</param>
public record TaskStatisticsDto(
    string TaskId,
    string TaskName,
    int TotalRequests,
    int SuccessRequests,
    int FailedRequests,
    decimal Progress,
    DateTime? StartTime,
    DateTime? EndTime
);

/// <summary>
/// 系统统计数据传输对象
/// </summary>
/// <param name="OnlineAgents">在线代理数</param>
/// <param name="TotalAgents">代理总数</param>
/// <param name="RunningTasks">运行中任务数</param>
/// <param name="PendingTasks">待执行任务数</param>
/// <param name="CompletedTasks">已完成任务数</param>
/// <param name="TotalDataVolume">数据总量</param>
/// <param name="AvgSuccessRate">平均成功率</param>
public record SystemStatisticsDto(
    int OnlineAgents,
    int TotalAgents,
    int RunningTasks,
    int PendingTasks,
    int CompletedTasks,
    long TotalDataVolume,
    decimal AvgSuccessRate
);

/// <summary>
/// 趋势数据点，用于绘制统计图表
/// </summary>
/// <param name="Date">日期</param>
/// <param name="TotalRequests">总请求数</param>
/// <param name="SuccessRequests">成功请求数</param>
/// <param name="FailedRequests">失败请求数</param>
/// <param name="DataVolume">数据量</param>
public record TrendDataPoint(
    DateTime Date,
    int TotalRequests,
    int SuccessRequests,
    int FailedRequests,
    long DataVolume
);
