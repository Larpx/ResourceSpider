using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("collection_results")]
public class CollectionResultEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string ResultId { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? ExpressionId { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? AgentId { get; set; }

    [SugarColumn(Length = 2048, IsNullable = true)]
    public string? SourceUrl { get; set; }

    [SugarColumn(ColumnDataType = "json")]
    public string Fields { get; set; } = "{}";

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? FieldExpressionMap { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? CollectedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
