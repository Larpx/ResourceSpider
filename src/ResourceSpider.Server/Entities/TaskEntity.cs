using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("tasks")]
public class TaskEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    [SugarColumn(Length = 256)]
    public string TaskName { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string TaskType { get; set; } = "SinglePage";

    public int Priority { get; set; } = 5;

    public int Status { get; set; }

    [SugarColumn(ColumnDataType = "json")]
    public string RequestConfig { get; set; } = "{}";

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? ScheduleConfig { get; set; }

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? RetryPolicy { get; set; }

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? AntiCrawlConfig { get; set; }

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? GlobalConfig { get; set; }

    public int ConfigVersion { get; set; } = 1;

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? Tags { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? AgentGroupId { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? AssignedAgentId { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? ExpressionId { get; set; }

    public decimal Progress { get; set; }

    public int TotalRequests { get; set; }

    public int CompletedRequests { get; set; }

    public int FailedRequests { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? StartTime { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? EndTime { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
