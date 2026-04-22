namespace ResourceSpider.Core.Models;

/// <summary>
/// 系统日志模型，记录系统运行过程中的关键事件
/// </summary>
public class SystemLog
{
    /// <summary>
    /// 日志唯一标识
    /// </summary>
    public string LogId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 日志级别，如 Info、Warning、Error
    /// </summary>
    public string Level { get; set; } = "Info";

    /// <summary>
    /// 日志分类，如 Agent、Task、Scheduler 等
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 日志消息内容
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 日志详细信息，以键值对形式存储
    /// </summary>
    public Dictionary<string, object?>? Detail { get; set; }

    /// <summary>
    /// 关联的用户标识
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// 日志创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
