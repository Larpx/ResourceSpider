using ResourceSpider.Core.Enums;
using TaskStatusEnum = ResourceSpider.Core.Enums.TaskStatus;

namespace ResourceSpider.Core.Models;

public class SpiderTask
{
    public string TaskId { get; set; } = Guid.NewGuid().ToString("N");
    
    public string TaskName { get; set; } = string.Empty;
    
    public string TaskType { get; set; } = string.Empty;
    
    public int Priority { get; set; } = 5;
    
    public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Pending;
    
    public Dictionary<string, object?> RequestConfig { get; set; } = new();
    
    public Dictionary<string, object?>? ScheduleConfig { get; set; }
    
    public Dictionary<string, object?>? RetryPolicy { get; set; }
    
    public string? AssignedAgentId { get; set; }
    
    public decimal Progress { get; set; }
    
    public int TotalRequests { get; set; }
    
    public int CompletedRequests { get; set; }
    
    public int FailedRequests { get; set; }
    
    public DateTime? StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string? ExpressionId { get; set; }

    public ExpressionConfig? ExpressionConfig { get; set; }
}
