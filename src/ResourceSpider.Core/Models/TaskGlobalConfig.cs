namespace ResourceSpider.Core.Models;

/// <summary>
/// 任务全局配置模型，定义所有步骤共享的全局变量、请求头、代理和速率限制
/// </summary>
public class TaskGlobalConfig
{
    /// <summary>
    /// 全局变量字典，可在步骤中通过 {{变量名}} 引用
    /// </summary>
    public Dictionary<string, object?>? GlobalVariables { get; set; }

    /// <summary>
    /// 全局 HTTP 请求头，所有步骤的请求都会携带
    /// </summary>
    public Dictionary<string, string>? GlobalHeaders { get; set; }

    /// <summary>
    /// 全局代理配置，所有步骤共享的代理设置
    /// </summary>
    public StepProxyConfig? GlobalProxy { get; set; }

    /// <summary>
    /// 速率限制配置，控制请求间隔
    /// </summary>
    public RateLimitConfig? RateLimit { get; set; }
}

/// <summary>
/// 速率限制配置模型，控制请求之间的最小和最大间隔
/// </summary>
public class RateLimitConfig
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
}
