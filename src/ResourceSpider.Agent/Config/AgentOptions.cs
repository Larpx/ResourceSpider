namespace ResourceSpider.Agent.Config;

/// <summary>
/// Agent 全局配置选项，包含运行模式及对应模式的子配置
/// </summary>
public class AgentOptions
{
    /// <summary>
    /// Agent 运行模式，支持 "Local"（本地模式）和 "Online"（在线模式），默认为 Local
    /// </summary>
    public string Mode { get; set; } = "Local";

    /// <summary>
    /// 本地模式配置，仅在 Mode 为 "Local" 时生效
    /// </summary>
    public LocalModeOptions LocalConfig { get; set; } = new();

    /// <summary>
    /// 在线模式配置，仅在 Mode 为 "Online" 时生效
    /// </summary>
    public OnlineModeOptions ServerConfig { get; set; } = new();
}
