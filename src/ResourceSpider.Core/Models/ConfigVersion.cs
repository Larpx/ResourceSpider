namespace Larpx.PersonalTools.ResourceSpider.Core.Models;

/// <summary>
/// 配置版本模型，记录任务配置的变更历史
/// </summary>
public class ConfigVersion
{
    /// <summary>
    /// 版本唯一标识
    /// </summary>
    public string VersionId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 关联的任务标识
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 版本号，递增表示更新的配置
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// 配置内容，以键值对形式存储
    /// </summary>
    public Dictionary<string, object?> ConfigContent { get; set; } = new();

    /// <summary>
    /// 变更描述，说明本次配置修改的内容
    /// </summary>
    public string? ChangeDescription { get; set; }

    /// <summary>
    /// 创建者标识
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
