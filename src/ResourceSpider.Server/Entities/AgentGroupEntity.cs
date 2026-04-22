using SqlSugar;

namespace ResourceSpider.Server.Entities;

/// <summary>
/// 代理分组实体，映射数据库 agent_groups 表
/// 用于将代理节点按功能、地域等维度进行分组管理，便于任务调度时选择合适的代理组
/// </summary>
[SugarTable("agent_groups")]
public class AgentGroupEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 分组唯一标识符，业务主键
    /// </summary>
    [SugarColumn(Length = 64)]
    public string GroupId { get; set; } = string.Empty;

    /// <summary>
    /// 分组名称，用于展示和识别分组
    /// </summary>
    [SugarColumn(Length = 128)]
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// 分组描述，说明分组的用途和适用场景
    /// </summary>
    [SugarColumn(Length = 512, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 分组内代理节点 ID 列表 JSON 数组
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? AgentIds { get; set; }

    /// <summary>
    /// 分组创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 分组信息最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
