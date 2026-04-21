using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("crawl_results")]
public class CrawlResultEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string ResultId { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string ExecutionId { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? StepId { get; set; }

    [SugarColumn(ColumnDataType = "json")]
    public string ExtractedData { get; set; } = "{}";

    [SugarColumn(Length = 2000, IsNullable = true)]
    public string? SourceUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
