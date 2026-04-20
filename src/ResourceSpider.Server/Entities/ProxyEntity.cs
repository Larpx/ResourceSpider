using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("proxies")]
public class ProxyEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string ProxyId { get; set; } = string.Empty;

    [SugarColumn(Length = 255)]
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    [SugarColumn(Length = 16)]
    public string Protocol { get; set; } = "HTTP";

    [SugarColumn(Length = 128, IsNullable = true)]
    public string? Username { get; set; }

    [SugarColumn(Length = 256, IsNullable = true)]
    public string? Password { get; set; }

    public int Status { get; set; }

    public int SuccessCount { get; set; }

    public int FailureCount { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? LastCheckedAt { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? NextCheckAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
