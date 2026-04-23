using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;


/// <summary>
/// 创建代理服务器请求
/// </summary>
/// <param name="Host">代理主机地址，最大长度 255</param>
/// <param name="Port">代理端口，范围 1-65535</param>
/// <param name="Protocol">代理协议，默认 HTTP，最大长度 16</param>
/// <param name="Username">认证用户名，可选</param>
/// <param name="Password">认证密码，可选</param>
public record CreateProxyRequest(
    [Required, StringLength(255)] string Host,
    [Required, Range(1, 65535)] int Port,
    [StringLength(16)] string Protocol = "HTTP",
    string? Username = null,
    string? Password = null
);

/// <summary>
/// 更新代理服务器请求
/// </summary>
/// <param name="Host">代理主机地址，可选，最大长度 255</param>
/// <param name="Port">代理端口，可选，范围 1-65535</param>
/// <param name="Protocol">代理协议，可选，最大长度 16</param>
/// <param name="Username">认证用户名，可选</param>
/// <param name="Password">认证密码，可选</param>
/// <param name="Status">代理状态，可选</param>
public record UpdateProxyRequest(
    [StringLength(255)] string? Host,
    [Range(1, 65535)] int? Port,
    [StringLength(16)] string? Protocol,
    string? Username,
    string? Password,
    int? Status
);

/// <summary>
/// 代理服务器数据传输对象
/// </summary>
/// <param name="ProxyId">代理 ID</param>
/// <param name="Host">主机地址</param>
/// <param name="Port">端口</param>
/// <param name="Protocol">协议</param>
/// <param name="Username">认证用户名</param>
/// <param name="Status">代理状态</param>
/// <param name="SuccessCount">成功使用次数</param>
/// <param name="FailureCount">失败次数</param>
/// <param name="LastCheckedAt">最后检查时间</param>
/// <param name="NextCheckAt">下次检查时间</param>
public record ProxyDto(
    string ProxyId,
    string Host,
    int Port,
    string Protocol,
    string? Username,
    int Status,
    int SuccessCount,
    int FailureCount,
    DateTime? LastCheckedAt,
    DateTime? NextCheckAt
);

/// <summary>
/// 代理列表响应，包含分页信息
/// </summary>
/// <param name="Proxies">代理列表</param>
/// <param name="Total">总数</param>
/// <param name="PageIndex">当前页码</param>
/// <param name="PageSize">每页数量</param>
public record ProxyListResponse(
    List<ProxyDto> Proxies,
    int Total,
    int PageIndex,
    int PageSize
);

/// <summary>
/// 代理测试请求，支持按 ID 或地址进行测试
/// </summary>
/// <param name="ProxyId">代理 ID，可选</param>
/// <param name="Host">主机地址，可选</param>
/// <param name="Port">端口，可选</param>
public record ProxyTestRequest(
    string? ProxyId = null,
    string? Host = null,
    int? Port = null
);

/// <summary>
/// 代理测试响应
/// </summary>
/// <param name="IsAvailable">代理是否可用</param>
/// <param name="DurationMs">响应时间（毫秒）</param>
/// <param name="Error">错误信息</param>
public record ProxyTestResponse(
    bool IsAvailable,
    int? DurationMs,
    string? Error
);
