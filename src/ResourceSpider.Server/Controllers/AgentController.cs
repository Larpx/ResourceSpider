using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

/// <summary>
/// 代理节点控制器，处理代理的注册、心跳和注销操作
/// 供代理节点客户端调用，用于维护代理与服务端的连接状态
/// </summary>
[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    /// <summary>
    /// 代理注册服务实例，处理代理的注册和心跳逻辑
    /// </summary>
    private readonly IAgentRegisterService _agentRegisterService;

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<AgentController> _logger;

    /// <summary>
    /// 初始化代理控制器
    /// </summary>
    /// <param name="agentRegisterService">代理注册服务</param>
    /// <param name="logger">日志记录器</param>
    public AgentController(
        IAgentRegisterService agentRegisterService,
        ILogger<AgentController> logger)
    {
        _agentRegisterService = agentRegisterService;
        _logger = logger;
    }

    /// <summary>
    /// 代理注册接口，将新的代理节点注册到系统中并返回认证令牌
    /// </summary>
    /// <param name="request">代理注册请求，包含代理名称、IP 地址等信息</param>
    /// <returns>注册成功返回代理信息和令牌</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterAgentResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterAgentRequest request)
    {
        var result = await _agentRegisterService.RegisterAsync(request);
        return Ok(ApiResponse<RegisterAgentResponse>.Success(result, "Agent registered successfully"));
    }

    /// <summary>
    /// 代理心跳接口，代理定期调用以维持在线状态并报告资源使用情况
    /// </summary>
    /// <param name="request">心跳请求，包含代理 ID、令牌和状态信息</param>
    /// <returns>令牌有效返回心跳确认，令牌无效返回 401 状态码</returns>
    [HttpPost("heartbeat")]
    [ProducesResponseType(typeof(ApiResponse<HeartbeatResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Heartbeat([FromBody] HeartbeatRequest request)
    {
        var result = await _agentRegisterService.HeartbeatAsync(request);

        if (!result.Ack)
        {
            return Unauthorized(ApiResponse<object>.Error(1002, "Token invalid or expired"));
        }

        return Ok(ApiResponse<HeartbeatResponse>.Success(result));
    }

    /// <summary>
    /// 代理注销接口，将代理节点从系统中移除
    /// </summary>
    /// <param name="request">注销请求，包含代理 ID 和令牌</param>
    /// <returns>注销成功返回确认，令牌无效返回 401 状态码</returns>
    [HttpPost("unregister")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Unregister([FromBody] UnregisterAgentRequest request)
    {
        await _agentRegisterService.UnregisterAsync(request);
        return Ok(ApiResponse<object>.Success(new { }, "Agent unregistered successfully"));
    }
}
