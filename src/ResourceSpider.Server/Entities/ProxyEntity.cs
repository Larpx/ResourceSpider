using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Entities;

/// <summary>
/// 代理服务器实体，映射数据库 proxies 表
/// 管理代理服务器资源，包含连接信息、认证凭据和健康状态
/// </summary>
[SugarTable("proxies")]
public class ProxyEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 代理唯一标识符，业务主键
    /// </summary>
    [SugarColumn(Length = 64)]
    public string ProxyId { get; set; } = string.Empty;

    /// <summary>
    /// 代理服务器主机地址
    /// </summary>
    [SugarColumn(Length = 255)]
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// 代理服务器端口
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 代理协议：HTTP、HTTPS、SOCKS5
    /// </summary>
    [SugarColumn(Length = 16)]
    public string Protocol { get; set; } = "HTTP";

    /// <summary>
    /// 代理认证用户名，无需认证时为空
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = true)]
    public string? Username { get; set; }

    /// <summary>
    /// 代理认证密码，无需认证时为空
    /// </summary>
    [SugarColumn(Length = 256, IsNullable = true)]
    public string? Password { get; set; }

    /// <summary>
    /// 代理状态：0-不可用，1-可用，2-待验证
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 累计成功使用次数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 累计失败使用次数
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// 最后一次可用性检查时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? LastCheckedAt { get; set; }

    /// <summary>
    /// 下一次计划检查时间，用于定时验证代理可用性
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? NextCheckAt { get; set; }

    /// <summary>
    /// 代理记录创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 代理记录最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
