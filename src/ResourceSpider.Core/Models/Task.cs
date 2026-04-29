using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

public class SpiderTask
{
    public string TaskId { get; set; } = Guid.NewGuid().ToString("N");

    public string TaskName { get; set; } = string.Empty;

    public TaskType TaskType { get; set; } = TaskType.SinglePage;

    public int Priority { get; set; } = 5;

    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;

    public StepRequestConfig? RequestConfig { get; set; }

    public TaskScheduleConfig? ScheduleConfig { get; set; }

    public StepRetryPolicy? RetryPolicy { get; set; }

    public AntiCrawlConfig? AntiCrawlConfig { get; set; }

    public TaskGlobalConfig? GlobalConfig { get; set; }

    public int ConfigVersion { get; set; } = 1;

    public List<string>? Tags { get; set; }

    public string? AgentGroupId { get; set; }

    public string? AssignedAgentId { get; set; }

    public string? ExpressionId { get; set; }

    public ExpressionConfig? ExpressionConfig { get; set; }

    public decimal Progress { get; set; }

    public int TotalRequests { get; set; }

    public int CompletedRequests { get; set; }

    public int FailedRequests { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<TaskStep>? Steps { get; set; }
}
