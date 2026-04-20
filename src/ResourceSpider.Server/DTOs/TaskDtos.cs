using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

public record CreateTaskRequest(
    [Required, StringLength(256)] string TaskName,
    [Required, StringLength(64)] string TaskType,
    [Range(1, 10)] int Priority = 5,
    string? RequestConfig = null,
    string? ScheduleConfig = null,
    string? RetryPolicy = null
);

public record UpdateTaskRequest(
    string? TaskName,
    int? Priority,
    string? RequestConfig,
    string? ScheduleConfig,
    string? RetryPolicy
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
    string? AssignedAgentId,
    decimal Progress,
    int TotalRequests,
    int CompletedRequests,
    int FailedRequests,
    DateTime? StartTime,
    DateTime? EndTime,
    string? CreatedBy,
    DateTime CreatedAt
);

public record TaskListResponse(
    List<TaskDto> Tasks,
    int Total,
    int PageIndex,
    int PageSize
);
