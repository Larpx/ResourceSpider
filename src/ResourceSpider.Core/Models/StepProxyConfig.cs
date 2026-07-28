namespace Larpx.PersonalTools.ResourceSpider.Core.Models;

/// <summary>
/// 步骤代理配置模型，定义步骤请求使用的代理服务器设置
/// </summary>
public class StepProxyConfig
{
    /// <summary>
    /// 代理服务器主机地址
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// 代理服务器端口
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 代理认证用户名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 代理认证密码
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 代理类型，如 "HTTP"、"HTTPS"、"SOCKS5"，默认 "HTTP"
    /// </summary>
    public string ProxyType { get; set; } = "HTTP";

    /// <summary>
    /// 代理列表，用于代理轮换模式
    /// </summary>
    public List<ProxyEntry>? ProxyList { get; set; }

    /// <summary>
    /// 代理轮换策略，如 "RoundRobin"（轮询）或 "Random"（随机），默认 "RoundRobin"
    /// </summary>
    public string RotationStrategy { get; set; } = "RoundRobin";
}

/// <summary>
/// 代理条目模型，表示代理列表中的单个代理服务器
/// </summary>
public class ProxyEntry
{
    /// <summary>
    /// 代理服务器主机地址
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// 代理服务器端口
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 代理认证用户名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 代理认证密码
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 代理类型，如 "HTTP"、"HTTPS"、"SOCKS5"，默认 "HTTP"
    /// </summary>
    public string ProxyType { get; set; } = "HTTP";
}
