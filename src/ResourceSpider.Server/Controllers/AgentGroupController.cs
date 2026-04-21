using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

[ApiController]
[Route("api/agents/groups")]
[Authorize]
public class AgentGroupController : ControllerBase
{
    private readonly IAgentGroupService _agentGroupService;
    private readonly ILogger<AgentGroupController> _logger;

    public AgentGroupController(IAgentGroupService agentGroupService, ILogger<AgentGroupController> logger)
    {
        _agentGroupService = agentGroupService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<AgentGroupDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _agentGroupService.GetAllAsync();
        return Ok(ApiResponse<List<AgentGroupDto>>.Success(result));
    }

    [HttpGet("{groupId}")]
    [ProducesResponseType(typeof(ApiResponse<AgentGroupDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(string groupId)
    {
        var result = await _agentGroupService.GetByIdAsync(groupId);
        if (result == null)
        {
            return NotFound(ApiResponse<object>.Error(1001, "分组不存在"));
        }
        return Ok(ApiResponse<AgentGroupDto>.Success(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AgentGroupDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create([FromBody] CreateAgentGroupRequest request)
    {
        var result = await _agentGroupService.CreateAsync(request);
        return Ok(ApiResponse<AgentGroupDto>.Success(result, "分组创建成功"));
    }

    [HttpPut("{groupId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Update(string groupId, [FromBody] UpdateAgentGroupRequest request)
    {
        var result = await _agentGroupService.UpdateAsync(groupId, request);
        if (!result)
        {
            return NotFound(ApiResponse<object>.Error(1001, "分组不存在"));
        }
        return Ok(ApiResponse<object>.Success(new { }, "分组更新成功"));
    }

    [HttpDelete("{groupId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(string groupId)
    {
        var result = await _agentGroupService.DeleteAsync(groupId);
        if (!result)
        {
            return NotFound(ApiResponse<object>.Error(1001, "分组不存在"));
        }
        return Ok(ApiResponse<object>.Success(new { }, "分组删除成功"));
    }
}
