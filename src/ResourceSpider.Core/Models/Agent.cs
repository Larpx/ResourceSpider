using Larpx.PersonalTools.ResourceSpider.Core.Enums;

namespace Larpx.PersonalTools.ResourceSpider.Core.Models;

/// <summary>
/// 代理节点模型，表示一个执行爬取任务的工作节点
/// </summary>
public class Agent
{
    /// <summary>
    /// 代理节点唯一标识
    /// </summary>
    public string AgentId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 代理节点名称
    /// </summary>
    public string AgentName { get; set; } = string.Empty;

    /// <summary>
    /// 代理节点认证令牌
    /// </summary>
    public string? AgentToken { get; set; }

    /// <summary>
    /// 代理节点 IP 地址
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 代理节点端口号
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 代理节点能力描述，如支持的下载器类型等
    /// </summary>
    public Dictionary<string, object?>? Capabilities { get; set; }

    /// <summary>
    /// 代理节点当前在线状态
    /// </summary>
    public AgentStatus Status { get; set; } = AgentStatus.Offline;

    /// <summary>
    /// CPU 使用率百分比
    /// </summary>
    public decimal? CpuUsage { get; set; }

    /// <summary>
    /// 内存使用率百分比
    /// </summary>
    public decimal? MemoryUsage { get; set; }

    /// <summary>
    /// 当前正在执行的任务数量
    /// </summary>
    public int TaskCount { get; set; }

    /// <summary>
    /// 最后一次心跳时间
    /// </summary>
    public DateTime? LastHeartbeat { get; set; }

    /// <summary>
    /// 代理节点标签列表，用于分类和筛选
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// 所属代理组标识
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// 操作系统信息
    /// </summary>
    public string? OS { get; set; }

    /// <summary>
    /// 代理节点版本号
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 代理节点配置信息
    /// </summary>
    public Dictionary<string, object?>? Config { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
