using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

public class TaskExecution
{
    public string ExecutionId { get; set; } = Guid.NewGuid().ToString("N");

    public string TaskId { get; set; } = string.Empty;

    public string AgentId { get; set; } = string.Empty;

    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;

    public Dictionary<string, object?>? ConfigSnapshot { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int TotalPages { get; set; }

    public int SuccessCount { get; set; }

    public int FailCount { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
