using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("agent_groups")]
public class AgentGroupEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string GroupId { get; set; } = string.Empty;

    [SugarColumn(Length = 128)]
    public string GroupName { get; set; } = string.Empty;

    [SugarColumn(Length = 512, IsNullable = true)]
    public string? Description { get; set; }

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? AgentIds { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
