using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Agent.Config;

/// <summary>
/// Agent 配置选项模型，定义 Agent 的运行模式、并发数、心跳间隔等核心配置
/// </summary>
public class AgentOptions
{
    /// <summary>
    /// 运行模式，支持 "Local"（本地模式）和 "Online"（在线模式），默认 Local
    /// </summary>
    public string Mode { get; set; } = "Local";

    /// <summary>
    /// 本地模式配置，当 Mode 为 Local 时生效
    /// </summary>
    public LocalModeOptions LocalConfig { get; set; } = new();

    /// <summary>
    /// 在线模式配置，当 Mode 为 Online 时生效
    /// </summary>
    public OnlineModeOptions ServerConfig { get; set; } = new();

    /// <summary>
    /// 最大并发任务数，默认 5
    /// </summary>
    public int MaxConcurrentTasks { get; set; } = 5;

    /// <summary>
    /// 心跳间隔时间（秒），默认 30 秒
    /// </summary>
    public int HeartbeatIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Agent 标签列表，用于标识 Agent 的能力和分组
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// 本地模式配置的快捷访问属性
    /// </summary>
    public LocalModeOptions? LocalMode => LocalConfig;

    /// <summary>
    /// 在线模式配置的快捷访问属性
    /// </summary>
    public OnlineModeOptions? OnlineMode => ServerConfig;
}
