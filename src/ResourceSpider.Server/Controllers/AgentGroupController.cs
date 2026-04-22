using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

/// <summary>
/// 代理分组控制器，提供代理分组的增删改查功能
/// 支持按分组管理代理节点，便于批量操作和资源隔离
/// </summary>
[ApiController]
[Route("api/admin/agent-groups")]
[Authorize]
public class AgentGroupController : ControllerBase
{
    /// <summary>
    /// 代理分组服务实例，处理分组的业务逻辑
    /// </summary>
    private readonly IAgentGroupService _agentGroupService;

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<AgentGroupController> _logger;

    /// <summary>
    /// 初始化代理分组控制器
    /// </summary>
    /// <param name="agentGroupService">代理分组服务</param>
    /// <param name="logger">日志记录器</param>
    public AgentGroupController(IAgentGroupService agentGroupService, ILogger<AgentGroupController> logger)
    {
        _agentGroupService = agentGroupService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有代理分组列表
    /// </summary>
    /// <returns>代理分组列表</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<AgentGroupDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _agentGroupService.GetAllAsync();
        return Ok(ApiResponse<List<AgentGroupDto>>.Success(result));
    }

    /// <summary>
    /// 根据分组 ID 获取分组详情
    /// </summary>
    /// <param name="groupId">分组 ID</param>
    /// <returns>分组存在返回详情，不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 创建新的代理分组
    /// </summary>
    /// <param name="request">创建分组请求，包含分组名称和描述</param>
    /// <returns>创建成功返回分组详情</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AgentGroupDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create([FromBody] CreateAgentGroupRequest request)
    {
        var result = await _agentGroupService.CreateAsync(request);
        return Ok(ApiResponse<AgentGroupDto>.Success(result, "分组创建成功"));
    }

    /// <summary>
    /// 更新指定代理分组的信息
    /// </summary>
    /// <param name="groupId">分组 ID</param>
    /// <param name="request">更新分组请求，包含需要更新的字段</param>
    /// <returns>更新成功返回确认，分组不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 删除指定代理分组
    /// </summary>
    /// <param name="groupId">分组 ID</param>
    /// <returns>删除成功返回确认，分组不存在返回 404 状态码</returns>
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
