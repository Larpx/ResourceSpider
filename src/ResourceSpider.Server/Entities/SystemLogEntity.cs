using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("system_logs")]
public class SystemLogEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 20)]
    public string Level { get; set; } = "Info";

    [SugarColumn(Length = 100)]
    public string Category { get; set; } = string.Empty;

    [SugarColumn(Length = 500)]
    public string Message { get; set; } = string.Empty;

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? Detail { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
