namespace Larpx.PersonalTools.ResourceSpider.Core.Models;

/// <summary>
/// 代理模型，表示一个 HTTP 代理服务器
/// </summary>
public class Proxy
{
    /// <summary>
    /// 代理唯一标识
    /// </summary>
    public string ProxyId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 代理服务器主机地址
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// 代理服务器端口号
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 代理协议类型，如 HTTP、HTTPS、SOCKS5
    /// </summary>
    public string Protocol { get; set; } = "HTTP";

    /// <summary>
    /// 认证用户名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 认证密码
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 代理是否可用
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// 使用成功次数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 使用失败次数
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// 健康分数，范围 0.0~1.0，越高表示代理越稳定
    /// </summary>
    public double HealthScore { get; set; } = 1.0;

    /// <summary>
    /// 最后检查时间
    /// </summary>
    public DateTime? LastCheckedAt { get; set; }

    /// <summary>
    /// 下次检查时间
    /// </summary>
    public DateTime? NextCheckAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 代理地址，格式为 Host:Port
    /// </summary>
    public string Address => $"{Host}:{Port}";
}
