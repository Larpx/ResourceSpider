using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Larpx.PersonalTools.ResourceSpider.Server.Hubs;
using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Services;

namespace Larpx.PersonalTools.ResourceSpider.Server.Controllers;

/// <summary>
/// 代理管理控制器，提供代理节点的管理功能
/// 包括查看代理列表、更新代理配置、发送控制命令（重启、更新、紧急停止）等
/// </summary>
[ApiController]
[Route("api/admin/agents")]
[Authorize]
public class AgentManagementController : ControllerBase
{
    /// <summary>
    /// 代理注册服务实例，处理代理的注册和状态管理
    /// </summary>
    private readonly IAgentRegisterService _agentRegisterService;

    /// <summary>
    /// SignalR Hub 上下文，用于向代理推送实时消息
    /// </summary>
    private readonly IHubContext<SpiderHub> _hubContext;

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<AgentManagementController> _logger;

    /// <summary>
    /// 初始化代理管理控制器
    /// </summary>
    /// <param name="agentRegisterService">代理注册服务</param>
    /// <param name="hubContext">SignalR Hub 上下文</param>
    /// <param name="logger">日志记录器</param>
    public AgentManagementController(
        IAgentRegisterService agentRegisterService,
        IHubContext<SpiderHub> hubContext,
        ILogger<AgentManagementController> logger)
    {
        _agentRegisterService = agentRegisterService;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有已注册代理的列表
    /// </summary>
    /// <param name="agentRepository">代理仓储，通过 DI 注入</param>
    /// <returns>代理列表</returns>
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

    /// <summary>
    /// 根据代理 ID 获取代理详情
    /// </summary>
    /// <param name="agentId">代理 ID</param>
    /// <param name="agentRepository">代理仓储，通过 DI 注入</param>
    /// <returns>代理存在返回详情，不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 更新指定代理的配置信息，并通过 SignalR 实时推送配置变更
    /// </summary>
    /// <param name="agentId">代理 ID</param>
    /// <param name="request">更新代理请求，包含名称、标签、分组和配置</param>
    /// <param name="agentRepository">代理仓储，通过 DI 注入</param>
    /// <returns>更新成功返回确认，代理不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 删除指定代理，将其从系统中注销
    /// </summary>
    /// <param name="agentId">代理 ID</param>
    /// <param name="agentRepository">代理仓储，通过 DI 注入</param>
    /// <returns>删除成功返回确认，代理不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 向指定代理发送重启命令，通过 SignalR 实时推送
    /// </summary>
    /// <param name="agentId">代理 ID</param>
    /// <returns>命令发送成功返回确认</returns>
    [HttpPost("{agentId}/restart")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Restart(string agentId)
    {
        await SpiderHubMethods.SendControlCommand(_hubContext, agentId, "Restart");
        _logger.LogInformation("发送重启指令给 Agent {AgentId}", agentId);
        return Ok(ApiResponse<object>.Success(new { }, "重启指令已发送"));
    }

    /// <summary>
    /// 向指定代理发送更新命令，通过 SignalR 实时推送
    /// </summary>
    /// <param name="agentId">代理 ID</param>
    /// <param name="versionInfo">版本信息，可选</param>
    /// <returns>命令发送成功返回确认</returns>
    [HttpPost("{agentId}/update")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Update(string agentId, [FromBody] object? versionInfo = null)
    {
        await SpiderHubMethods.SendControlCommand(_hubContext, agentId, "Update", versionInfo);
        _logger.LogInformation("发送更新指令给 Agent {AgentId}", agentId);
        return Ok(ApiResponse<object>.Success(new { }, "更新指令已发送"));
    }

    /// <summary>
    /// 向指定代理发送紧急停止命令，终止所有正在执行的任务
    /// </summary>
    /// <param name="agentId">代理 ID</param>
    /// <returns>命令发送成功返回确认</returns>
    [HttpPost("{agentId}/stop-all")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> StopAll(string agentId)
    {
        await SpiderHubMethods.SendControlCommand(_hubContext, agentId, "StopAll");
        _logger.LogInformation("发送紧急停止指令给 Agent {AgentId}", agentId);
        return Ok(ApiResponse<object>.Success(new { }, "紧急停止指令已发送"));
    }
}
