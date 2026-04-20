using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private readonly IAgentRegisterService _agentRegisterService;
    private readonly ILogger<AgentController> _logger;

    public AgentController(
        IAgentRegisterService agentRegisterService,
        ILogger<AgentController> logger)
    {
        _agentRegisterService = agentRegisterService;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterAgentResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterAgentRequest request)
    {
        var result = await _agentRegisterService.RegisterAsync(request);
        return Ok(ApiResponse<RegisterAgentResponse>.Success(result, "Agent registered successfully"));
    }

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

    [HttpPost("unregister")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Unregister([FromBody] UnregisterAgentRequest request)
    {
        await _agentRegisterService.UnregisterAsync(request);
        return Ok(ApiResponse<object>.Success(new { }, "Agent unregistered successfully"));
    }

    [HttpGet("tasks/pull")]
    [ProducesResponseType(typeof(ApiResponse<List<TaskDto>>), 200)]
    public async Task<IActionResult> PullTasks(
        [FromQuery] string agentId,
        [FromQuery] string agentToken,
        [FromQuery] int maxCount = 10)
    {
        var tasks = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken)
            ? new List<TaskDto>()
            : new List<TaskDto>();

        return Ok(ApiResponse<List<TaskDto>>.Success(tasks));
    }
}
