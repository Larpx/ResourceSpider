namespace ResourceSpider.Core.Models;

/// <summary>
/// 步骤请求配置模型，定义步骤中 HTTP 请求的完整参数，包括 URL、方法、头部、超时和重试策略
/// </summary>
public class StepRequestConfig
{
    /// <summary>
    /// URL 模板，支持 {{变量名}} 占位符，如 "https://example.com/page/{{PAGE_NUM}}"
    /// </summary>
    public string UrlTemplate { get; set; } = string.Empty;

    /// <summary>
    /// HTTP 请求方法，如 "GET"、"POST"、"PUT"，默认 "GET"
    /// </summary>
    public string Method { get; set; } = "GET";

    /// <summary>
    /// 自定义 HTTP 请求头
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// 请求体内容
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// 请求体类型，如 "application/json"、"application/x-www-form-urlencoded"
    /// </summary>
    public string? BodyType { get; set; }

    /// <summary>
    /// 自定义 Cookie 字典
    /// </summary>
    public Dictionary<string, string>? Cookies { get; set; }

    /// <summary>
    /// 请求超时时间（毫秒），默认 60000ms
    /// </summary>
    public int Timeout { get; set; } = 60000;

    /// <summary>
    /// 连接超时时间（毫秒），默认 30000ms
    /// </summary>
    public int ConnectTimeout { get; set; } = 30000;

    /// <summary>
    /// 最大重定向次数，默认 10 次
    /// </summary>
    public int MaxRedirects { get; set; } = 10;

    /// <summary>
    /// 是否跟随重定向，默认启用
    /// </summary>
    public bool FollowRedirects { get; set; } = true;

    /// <summary>
    /// 请求重试策略
    /// </summary>
    public StepRetryPolicy? RetryPolicy { get; set; }

    /// <summary>
    /// 请求使用的代理配置
    /// </summary>
    public StepProxyConfig? ProxyConfig { get; set; }

    /// <summary>
    /// Playwright 浏览器自动化配置，当采集模式为 Playwright 时使用
    /// </summary>
    public PlaywrightConfig? PlaywrightConfig { get; set; }
}
