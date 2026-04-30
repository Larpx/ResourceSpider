using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

/// <summary>
/// 用户登录请求
/// </summary>
/// <param name="Username">用户名，最大长度 128</param>
/// <param name="Password">密码，最大长度 128</param>
public record LoginRequest(
    [Required, StringLength(128)] string Username,
    [Required, StringLength(128)] string Password
);

/// <summary>
/// 用户注册请求
/// </summary>
/// <param name="Username">用户名，最大长度 128</param>
/// <param name="Password">密码，最大长度 128</param>
/// <param name="Role">用户角色，默认为 Operator</param>
public record RegisterRequest(
    [Required, StringLength(128)] string Username,
    [Required, StringLength(128)] string Password,
    [StringLength(64)] string Role = "Operator"
);

/// <summary>
/// 认证响应，包含 JWT 令牌和用户信息
/// </summary>
/// <param name="Token">JWT 访问令牌</param>
/// <param name="Username">用户名</param>
/// <param name="Role">用户角色</param>
/// <param name="ExpiresAt">令牌过期时间</param>
public record AuthResponse(
    string Token,
    string Username,
    string Role,
    DateTime ExpiresAt
);

/// <summary>
/// 用户信息数据传输对象
/// </summary>
/// <param name="UserId">用户 ID</param>
/// <param name="Username">用户名</param>
/// <param name="Role">用户角色</param>
/// <param name="Status">用户状态</param>
/// <param name="CreatedAt">创建时间</param>
public record UserInfoDto(
    string UserId,
    string Username,
    string Role,
    int Status,
    DateTime CreatedAt
);

/// <summary>
/// 更新管理员资料请求
/// </summary>
/// <param name="Username">新用户名</param>
public record UpdateAdminProfileRequest(
    [Required, StringLength(128)] string Username
);

/// <summary>
/// 修改管理员密码请求
/// </summary>
/// <param name="CurrentPassword">当前密码</param>
/// <param name="NewPassword">新密码</param>
public record ChangeAdminPasswordRequest(
    [Required] string CurrentPassword,
    [Required] string NewPassword
);
