namespace ResourceSpider.Server.DTOs;

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
    DateTime CreatedAt
);

public record TaskExecutionListResponse(
    List<TaskExecutionDto> Executions,
    int Total,
    int PageIndex,
    int PageSize
);
