namespace ResourceSpider.Core.Models;

using ResourceSpider.Core.Enums;

/// <summary>
/// 任务全局配置模型，定义所有步骤共享的全局变量、请求头、代理、速率限制和去重策略
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

    /// <summary>
    /// 结果数据去重配置，定义去重策略和参与去重的字段
    /// </summary>
    public DeduplicationConfig? Deduplication { get; set; }
}

/// <summary>
/// 去重配置模型，定义去重策略和参与去重计算的字段
/// </summary>
public class DeduplicationConfig
{
    /// <summary>
    /// 去重策略，默认基于全字段指纹去重
    /// </summary>
    public DeduplicationStrategy Strategy { get; set; } = DeduplicationStrategy.FullFingerprint;

    /// <summary>
    /// 参与去重计算的字段名列表，仅在 FieldCombination 策略下生效
    /// </summary>
    public List<string>? DeduplicationFields { get; set; }

    /// <summary>
    /// 主键字段名列表，仅在 PrimaryKey 策略下生效
    /// </summary>
    public List<string>? PrimaryKeyFields { get; set; }
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
