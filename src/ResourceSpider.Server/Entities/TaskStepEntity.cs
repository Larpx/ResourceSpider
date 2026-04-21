using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("task_steps")]
public class TaskStepEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string StepId { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    public int StepOrder { get; set; }

    [SugarColumn(Length = 100)]
    public string StepName { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string CollectionMode { get; set; } = "HttpClient";

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? AgentGroupId { get; set; }

    [SugarColumn(ColumnDataType = "json")]
    public string RequestConfig { get; set; } = "{}";

    [SugarColumn(ColumnDataType = "json")]
    public string ExtractionRules { get; set; } = "[]";

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? VariableMappings { get; set; }

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? PaginationConfig { get; set; }

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? OutputConfig { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
