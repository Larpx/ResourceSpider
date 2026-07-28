using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using Larpx.PersonalTools.ResourceSpider.Server.Repositories;

namespace Larpx.PersonalTools.ResourceSpider.Server.Services;

/// <summary>
/// 认证服务接口，提供用户登录、注册及信息查询功能
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 用户登录，验证凭据并返回 JWT 令牌
    /// </summary>
    /// <param name="request">登录请求，包含用户名和密码</param>
    /// <returns>认证响应（含令牌），若验证失败返回 null</returns>
    Task<AuthResponse?> LoginAsync(LoginRequest request);

    /// <summary>
    /// 用户注册，创建新用户并返回 JWT 令牌
    /// </summary>
    /// <param name="request">注册请求，包含用户名、密码和角色</param>
    /// <returns>认证响应（含令牌），若用户名已存在返回 null</returns>
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// 根据用户标识获取用户信息
    /// </summary>
    /// <param name="userId">用户唯一标识</param>
    /// <returns>用户信息 DTO，若用户不存在返回 null</returns>
    Task<UserInfoDto?> GetUserInfoAsync(string userId);
}

/// <summary>
/// 认证服务实现，处理用户登录验证、注册及 JWT 令牌生成
/// </summary>
public class AuthService : IAuthService
{
    /// <summary>
    /// 用户数据仓库，用于用户数据的持久化操作
    /// </summary>
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// 应用配置，用于读取 JWT 密钥和过期时间等配置项
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// 日志记录器，用于记录认证相关的事件
    /// </summary>
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// 初始化认证服务实例
    /// </summary>
    /// <param name="userRepository">用户数据仓库</param>
    /// <param name="configuration">应用配置</param>
    /// <param name="logger">日志记录器</param>
    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("用户 {Username} 登录失败", request.Username);
            return null;
        }

        if (user.Status != 1)
        {
            _logger.LogWarning("用户 {Username} 已禁用", request.Username);
            return null;
        }

        var token = GenerateJwtToken(user);
        _logger.LogInformation("用户 {Username} 登录成功", request.Username);

        return new AuthResponse(token, user.Username, user.Role, DateTime.UtcNow.AddHours(24));
    }

    /// <inheritdoc />
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepository.GetByUsernameAsync(request.Username);
        if (existing != null)
        {
            return null;
        }

        var user = new UserEntity
        {
            UserId = Guid.NewGuid().ToString("N"),
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            Status = 1
        };

        await _userRepository.AddAsync(user);
        _logger.LogInformation("用户 {Username} 注册成功", request.Username);

        var token = GenerateJwtToken(user);
        return new AuthResponse(token, user.Username, user.Role, DateTime.UtcNow.AddHours(24));
    }

    /// <inheritdoc />
    public async Task<UserInfoDto?> GetUserInfoAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserInfoDto(user.UserId, user.Username, user.Role, user.Status, user.CreatedAt);
    }

    /// <summary>
    /// 根据用户实体生成 JWT 访问令牌
    /// </summary>
    /// <param name="user">用户实体，包含用户标识、名称和角色信息</param>
    /// <returns>编码后的 JWT 令牌字符串</returns>
    private string GenerateJwtToken(UserEntity user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "default-secret-key"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expiryHours = int.Parse(_configuration["Jwt:ExpiryHours"] ?? "24");
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
