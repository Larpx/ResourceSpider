namespace Larpx.PersonalTools.ResourceSpider.Core.Models;

/// <summary>
/// 代理组模型，用于将多个代理节点组织在一起进行统一管理
/// </summary>
public class AgentGroup
{
    /// <summary>
    /// 代理组唯一标识
    /// </summary>
    public string GroupId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 代理组名称
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// 代理组描述信息
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 组内代理节点标识列表
    /// </summary>
    public List<string> AgentIds { get; set; } = new();

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
