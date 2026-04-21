using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ResourceSpider.Server.Hubs;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

[ApiController]
[Route("api/agents")]
[Authorize]
public class AgentManagementController : ControllerBase
{
    private readonly IAgentRegisterService _agentRegisterService;
    private readonly IHubContext<SpiderHub> _hubContext;
    private readonly ILogger<AgentManagementController> _logger;

    public AgentManagementController(
        IAgentRegisterService agentRegisterService,
        IHubContext<SpiderHub> hubContext,
        ILogger<AgentManagementController> logger)
    {
        _agentRegisterService = agentRegisterService;
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<AgentDto>>), 200)]
    public async Task<IActionResult> GetList([FromServices] Repositories.IAgentRepository agentRepository)
    {
        var agents = await agentRepository.GetAllAsync();
        var dtos = agents.Select(a => new AgentDto(
            a.AgentId, a.AgentName, a.IpAddress, a.Port, a.Status,
            a.CpuUsage, a.MemoryUsage, a.TaskCount, a.LastHeartbeat,
            !string.IsNullOrEmpty(a.Tags) ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(a.Tags) : null,
            a.GroupId, a.OS, a.Version, a.CreatedAt)).ToList();
        return Ok(ApiResponse<List<AgentDto>>.Success(dtos));
    }

    [HttpGet("{agentId}")]
    [ProducesResponseType(typeof(ApiResponse<AgentDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(string agentId, [FromServices] Repositories.IAgentRepository agentRepository)
    {
        var agent = await agentRepository.GetByIdAsync(agentId);
        if (agent == null)
        {
            return NotFound(ApiResponse<object>.Error(1001, "Agent 不存在"));
        }

        var dto = new AgentDto(
            agent.AgentId, agent.AgentName, agent.IpAddress, agent.Port, agent.Status,
            agent.CpuUsage, agent.MemoryUsage, agent.TaskCount, agent.LastHeartbeat,
            !string.IsNullOrEmpty(agent.Tags) ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(agent.Tags) : null,
            agent.GroupId, agent.OS, agent.Version, agent.CreatedAt);

        return Ok(ApiResponse<AgentDto>.Success(dto));
    }

    [HttpPut("{agentId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Update(string agentId, [FromBody] UpdateAgentRequest request,
        [FromServices] Repositories.IAgentRepository agentRepository)
    {
        var agent = await agentRepository.GetByIdAsync(agentId);
        if (agent == null)
        {
            return NotFound(ApiResponse<object>.Error(1001, "Agent 不存在"));
        }

        if (request.AgentName != null) agent.AgentName = request.AgentName;
        if (request.Tags != null) agent.Tags = Newtonsoft.Json.JsonConvert.SerializeObject(request.Tags);
        if (request.GroupId != null) agent.GroupId = request.GroupId;
        if (request.Config != null) agent.Config = Newtonsoft.Json.JsonConvert.SerializeObject(request.Config);

        agent.UpdatedAt = DateTime.UtcNow;
        await agentRepository.UpdateAsync(agent);

        await SpiderHubMethods.SendConfigUpdate(_hubContext, agentId, new { agent.Config });
        return Ok(ApiResponse<object>.Success(new { }, "Agent 配置更新成功"));
    }

    [HttpDelete("{agentId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Delete(string agentId, [FromServices] Repositories.IAgentRepository agentRepository)
    {
        var agent = await agentRepository.GetByIdAsync(agentId);
        if (agent == null)
        {
            return NotFound(ApiResponse<object>.Error(1001, "Agent 不存在"));
        }

        await agentRepository.DeleteAsync(agentId);
        return Ok(ApiResponse<object>.Success(new { }, "Agent 注销成功"));
    }

    [HttpPost("{agentId}/restart")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Restart(string agentId)
    {
        await SpiderHubMethods.SendControlCommand(_hubContext, agentId, "Restart");
        _logger.LogInformation("发送重启指令给 Agent {AgentId}", agentId);
        return Ok(ApiResponse<object>.Success(new { }, "重启指令已发送"));
    }

    [HttpPost("{agentId}/update")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Update(string agentId, [FromBody] object? versionInfo = null)
    {
        await SpiderHubMethods.SendControlCommand(_hubContext, agentId, "Update", versionInfo);
        _logger.LogInformation("发送更新指令给 Agent {AgentId}", agentId);
        return Ok(ApiResponse<object>.Success(new { }, "更新指令已发送"));
    }

    [HttpPost("{agentId}/stop-all")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> StopAll(string agentId)
    {
        await SpiderHubMethods.SendControlCommand(_hubContext, agentId, "StopAll");
        _logger.LogInformation("发送紧急停止指令给 Agent {AgentId}", agentId);
        return Ok(ApiResponse<object>.Success(new { }, "紧急停止指令已发送"));
    }
}
