using SqlSugar;

namespace ResourceSpider.Server.Entities;

/// <summary>
/// 系统日志实体，映射数据库 system_logs 表
/// 记录系统运行过程中的关键事件和异常信息，用于问题排查和审计追踪
/// </summary>
[SugarTable("system_logs")]
public class SystemLogEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 日志级别：Debug、Info、Warning、Error、Critical
    /// </summary>
    [SugarColumn(Length = 20)]
    public string Level { get; set; } = "Info";

    /// <summary>
    /// 日志分类，标识日志所属的功能模块，如 Auth、Task、Agent 等
    /// </summary>
    [SugarColumn(Length = 100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 日志消息摘要
    /// </summary>
    [SugarColumn(Length = 500)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 日志详细信息 JSON，包含异常堆栈、请求参数等补充信息
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? Detail { get; set; }

    /// <summary>
    /// 触发日志的用户 ID，系统级日志可为空
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? UserId { get; set; }

    /// <summary>
    /// 日志记录时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
