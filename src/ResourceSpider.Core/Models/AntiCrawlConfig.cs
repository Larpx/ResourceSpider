namespace ResourceSpider.Core.Models;

/// <summary>
/// 反爬策略配置模型，定义请求间隔、User-Agent 轮换、代理轮换等反检测措施
/// </summary>
public class AntiCrawlConfig
{
    /// <summary>
    /// 请求最小间隔时间（毫秒），默认 1000ms
    /// </summary>
    public int MinIntervalMs { get; set; } = 1000;

    /// <summary>
    /// 请求最大间隔时间（毫秒），默认 3000ms
    /// </summary>
    public int MaxIntervalMs { get; set; } = 3000;

    /// <summary>
    /// 是否在最小和最大间隔之间随机延迟，默认启用
    /// </summary>
    public bool RandomDelay { get; set; } = true;

    /// <summary>
    /// User-Agent 列表，用于轮换请求头中的 User-Agent
    /// </summary>
    public List<string>? UserAgentList { get; set; }

    /// <summary>
    /// User-Agent 轮换策略，如 "RoundRobin"（轮询）或 "Random"（随机），默认 "RoundRobin"
    /// </summary>
    public string UserAgentRotationStrategy { get; set; } = "RoundRobin";

    /// <summary>
    /// 是否启用代理轮换，默认不启用
    /// </summary>
    public bool UseProxyRotation { get; set; }

    /// <summary>
    /// 是否随机化浏览器指纹，默认不启用
    /// </summary>
    public bool RandomizeFingerprint { get; set; }

    /// <summary>
    /// 是否模拟人类行为（如鼠标移动、滚动延迟等），默认不启用
    /// </summary>
    public bool SimulateHumanBehavior { get; set; }
}
