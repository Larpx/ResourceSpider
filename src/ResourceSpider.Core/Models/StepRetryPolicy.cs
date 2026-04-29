namespace ResourceSpider.Core.Models;

/// <summary>
/// 步骤重试策略配置模型，定义请求失败后的重试行为
/// </summary>
public class StepRetryPolicy
{
    /// <summary>
    /// 最大重试次数，默认 3 次
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// 重试间隔时间（毫秒），默认 1000ms，采用指数退避策略
    /// </summary>
    public int RetryIntervalMs { get; set; } = 1000;

    /// <summary>
    /// 超时错误是否重试，默认启用
    /// </summary>
    public bool RetryOnTimeout { get; set; } = true;

    /// <summary>
    /// 网络错误是否重试，默认启用
    /// </summary>
    public bool RetryOnNetworkError { get; set; } = true;

    /// <summary>
    /// 触发重试的 HTTP 状态码列表，如 [500, 502, 503]
    /// </summary>
    public List<int>? RetryOnHttpStatusCodes { get; set; }
}
