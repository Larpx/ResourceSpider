using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("expression_availability")]
public class ExpressionAvailabilityEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string ExpressionId { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string AgentId { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? FailureReason { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? LastCheckedAt { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? LastSuccessAt { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? LastFailureAt { get; set; }

    public int ConsecutiveFailures { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
