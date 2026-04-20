using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("statistics")]
public class StatisticEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string AgentId { get; set; } = string.Empty;

    public DateTime StatDate { get; set; }

    public int TotalRequests { get; set; }

    public int SuccessRequests { get; set; }

    public int FailedRequests { get; set; }

    public decimal? AvgDuration { get; set; }

    public long DataVolume { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
