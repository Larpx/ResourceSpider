using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("expressions")]
public class ExpressionEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string ExpressionId { get; set; } = string.Empty;

    [SugarColumn(Length = 128)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(Length = 512, IsNullable = true)]
    public string? Description { get; set; }

    [SugarColumn(Length = 32)]
    public string SelectorType { get; set; } = "XPath";

    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? ContainerExpression { get; set; }

    public int Status { get; set; } = 1;

    public int SuccessCount { get; set; }

    public int FailureCount { get; set; }

    public int ConsecutiveFailures { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? LastValidatedAt { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? LastUsedAt { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? ExpiredAt { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
