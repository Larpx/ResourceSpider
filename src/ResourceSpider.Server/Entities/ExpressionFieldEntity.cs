using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("expression_fields")]
public class ExpressionFieldEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string FieldId { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string ExpressionId { get; set; } = string.Empty;

    [SugarColumn(Length = 128)]
    public string FieldName { get; set; } = string.Empty;

    [SugarColumn(Length = 32)]
    public string SelectorType { get; set; } = "XPath";

    [SugarColumn(Length = 1024)]
    public string Expression { get; set; } = string.Empty;

    [SugarColumn(Length = 128, IsNullable = true)]
    public string? AttributeName { get; set; }

    public bool IsRequired { get; set; }

    [SugarColumn(Length = 256, IsNullable = true)]
    public string? DefaultValue { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? Formatter { get; set; }

    [SugarColumn(Length = 512, IsNullable = true)]
    public string? FormatterArgs { get; set; }

    public int Order { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
