using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("step_resources")]
public class StepResourceEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string ResourceId { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string StepId { get; set; } = string.Empty;

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? SourceStepId { get; set; }

    [SugarColumn(Length = 64)]
    public string ResourceType { get; set; } = "Record";

    [SugarColumn(ColumnDataType = "json")]
    public string Payload { get; set; } = "{}";

    [SugarColumn(Length = 128)]
    public string ContentHash { get; set; } = string.Empty;

    [SugarColumn(Length = 2048, IsNullable = true)]
    public string? SourceUrl { get; set; }

    /// <summary>
    /// 0-可用,1-已消费
    /// </summary>
    public int Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
