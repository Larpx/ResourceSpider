using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("config_versions")]
public class ConfigVersionEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string VersionId { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    public int Version { get; set; }

    [SugarColumn(ColumnDataType = "json")]
    public string ConfigContent { get; set; } = "{}";

    [SugarColumn(IsNullable = true)]
    public string? ChangeDescription { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
