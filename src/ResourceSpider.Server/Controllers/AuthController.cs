using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

/// <summary>
/// 认证控制器，提供用户登录和注册功能
/// 处理 JWT 令牌的颁发和用户账户的创建
/// </summary>
[ApiController]
[Route("api/admin/auth")]
public class AuthController : ControllerBase
{
    /// <summary>
    /// 认证服务实例，处理登录和注册的业务逻辑
    /// </summary>
    private readonly IAuthService _authService;

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// 初始化认证控制器
    /// </summary>
    /// <param name="authService">认证服务</param>
    /// <param name="logger">日志记录器</param>
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// 用户登录接口，验证用户名和密码后颁发 JWT 令牌
    /// </summary>
    /// <param name="request">登录请求，包含用户名和密码</param>
    /// <returns>成功返回认证响应（含令牌），失败返回 401 状态码</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            if (result == null)
            {
                return Unauthorized(ApiResponse<object>.Error(401, "用户名或密码错误"));
            }

            return Ok(ApiResponse<AuthResponse>.Success(result, "登录成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登录时发生异常");
            return StatusCode(500, ApiResponse<object>.Error(500, "服务器错误，登录失败"));
        }
    }

    /// <summary>
    /// 用户注册接口，创建新用户账户并颁发 JWT 令牌
    /// </summary>
    /// <param name="request">注册请求，包含用户名和密码</param>
    /// <returns>成功返回认证响应（含令牌），用户名已存在返回 400 状态码</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result == null)
        {
            return BadRequest(ApiResponse<object>.Error(10403, "用户名已存在"));
        }
        return Ok(ApiResponse<AuthResponse>.Success(result, "注册成功"));
    }
}
