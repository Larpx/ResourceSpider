using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("task_executions")]
public class TaskExecutionEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string ExecutionId { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string AgentId { get; set; } = string.Empty;

    public int Status { get; set; }

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? ConfigSnapshot { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? StartedAt { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? CompletedAt { get; set; }

    public int TotalPages { get; set; }

    public int SuccessCount { get; set; }

    public int FailCount { get; set; }

    [SugarColumn(IsNullable = true)]
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
