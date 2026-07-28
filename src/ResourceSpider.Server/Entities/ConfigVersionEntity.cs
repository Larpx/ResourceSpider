using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Entities;

/// <summary>
/// 配置版本实体，映射数据库 config_versions 表
/// 记录任务配置的历史版本，支持配置回滚和变更追踪
/// </summary>
[SugarTable("config_versions")]
public class ConfigVersionEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 配置版本唯一标识符，业务主键
    /// </summary>
    [SugarColumn(Length = 64)]
    public string VersionId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的任务 ID，标识该配置版本所属的任务
    /// </summary>
    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 版本号，从 1 开始递增
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// 配置内容 JSON，包含该版本的完整任务配置快照
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string ConfigContent { get; set; } = "{}";

    /// <summary>
    /// 变更描述，说明本次配置修改的内容和原因
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? ChangeDescription { get; set; }

    /// <summary>
    /// 执行配置变更的用户 ID
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 配置版本创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
