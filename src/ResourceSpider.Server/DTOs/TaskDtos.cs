using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

public record CreateTaskRequest(
    [Required, StringLength(256)] string TaskName,
    [Required, StringLength(64)] string TaskType,
    [Range(1, 10)] int Priority = 5,
    string? RequestConfig = null,
    string? ScheduleConfig = null,
    string? RetryPolicy = null,
    string? AntiCrawlConfig = null,
    string? GlobalConfig = null,
    string? Tags = null,
    string? AgentGroupId = null,
    string? ExpressionId = null,
    List<CreateTaskStepRequest>? Steps = null
);

public record UpdateTaskRequest(
    string? TaskName,
    int? Priority,
    string? RequestConfig,
    string? ScheduleConfig,
    string? RetryPolicy,
    string? AntiCrawlConfig,
    string? GlobalConfig,
    string? Tags,
    string? AgentGroupId
);

public record TaskDto(
    string TaskId,
    string TaskName,
    string TaskType,
    int Priority,
    int Status,
    string RequestConfig,
    string? ScheduleConfig,
    string? RetryPolicy,
    string? AntiCrawlConfig,
    string? GlobalConfig,
    int ConfigVersion,
    string? Tags,
    string? AgentGroupId,
    string? AssignedAgentId,
    decimal Progress,
    int TotalRequests,
    int CompletedRequests,
    int FailedRequests,
    DateTime? StartTime,
    DateTime? EndTime,
    string? CreatedBy,
    DateTime CreatedAt,
    string? ExpressionId = null,
    ExpressionConfigDto? ExpressionConfig = null,
    List<TaskStepDto>? Steps = null
);

public record TaskListResponse(
    List<TaskDto> Tasks,
    int Total,
    int PageIndex,
    int PageSize
);
