using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("agents")]
public class AgentEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string AgentId { get; set; } = string.Empty;

    [SugarColumn(Length = 128)]
    public string AgentName { get; set; } = string.Empty;

    [SugarColumn(Length = 256)]
    public string AgentToken { get; set; } = string.Empty;

    [SugarColumn(Length = 45)]
    public string IpAddress { get; set; } = string.Empty;

    public int Port { get; set; }

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? Capabilities { get; set; }

    public int Status { get; set; }

    [SugarColumn(IsNullable = true)]
    public decimal? CpuUsage { get; set; }

    [SugarColumn(IsNullable = true)]
    public decimal? MemoryUsage { get; set; }

    public int TaskCount { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? LastHeartbeat { get; set; }

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? Tags { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? GroupId { get; set; }

    [SugarColumn(Length = 100, IsNullable = true)]
    public string? OS { get; set; }

    [SugarColumn(Length = 50, IsNullable = true)]
    public string? Version { get; set; }

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? Config { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
