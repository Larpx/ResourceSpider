namespace ResourceSpider.Server.DTOs;

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

public record SystemStatisticsDto(
    int OnlineAgents,
    int TotalAgents,
    int RunningTasks,
    int PendingTasks,
    int CompletedTasks,
    long TotalDataVolume,
    decimal AvgSuccessRate
);

public record TrendDataPoint(
    DateTime Date,
    int TotalRequests,
    int SuccessRequests,
    int FailedRequests,
    long DataVolume
);
