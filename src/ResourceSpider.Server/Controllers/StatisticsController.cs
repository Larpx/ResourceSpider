using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Services;

namespace Larpx.PersonalTools.ResourceSpider.Server.Controllers;

/// <summary>
/// 统计控制器，提供系统各维度的统计数据
/// 包括代理统计、任务统计、系统概览和趋势数据
/// </summary>
[ApiController]
[Route("api/admin/statistics")]
[Authorize]
public class StatisticsController : ControllerBase
{
    /// <summary>
    /// 统计服务实例，处理统计数据的查询逻辑
    /// </summary>
    private readonly IStatisticsService _statisticsService;

    /// <summary>
    /// 初始化统计控制器
    /// </summary>
    /// <param name="statisticsService">统计服务</param>
    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// 获取所有代理节点的统计数据，包括在线率、任务执行量等
    /// </summary>
    /// <returns>代理统计列表</returns>
    [HttpGet("agent")]
    [ProducesResponseType(typeof(ApiResponse<List<AgentStatisticsDto>>), 200)]
    public async Task<IActionResult> GetAgentStatistics()
    {
        var result = await _statisticsService.GetAgentStatisticsAsync();
        return Ok(ApiResponse<List<AgentStatisticsDto>>.Success(result));
    }

    /// <summary>
    /// 获取指定任务的统计数据，包括执行次数、成功率、数据量等
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <returns>任务统计详情，任务不存在返回 404 状态码</returns>
    [HttpGet("task/{taskId}")]
    [ProducesResponseType(typeof(ApiResponse<TaskStatisticsDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetTaskStatistics(string taskId)
    {
        var result = await _statisticsService.GetTaskStatisticsAsync(taskId);
        if (result == null)
        {
            return NotFound(ApiResponse<object>.Error(1003, "Task not found"));
        }
        return Ok(ApiResponse<TaskStatisticsDto>.Success(result));
    }

    /// <summary>
    /// 获取系统整体统计数据，包括代理总数、任务总数、数据总量等
    /// </summary>
    /// <returns>系统统计概览</returns>
    [HttpGet("system")]
    [ProducesResponseType(typeof(ApiResponse<SystemStatisticsDto>), 200)]
    public async Task<IActionResult> GetSystemStatistics()
    {
        var result = await _statisticsService.GetSystemStatisticsAsync();
        return Ok(ApiResponse<SystemStatisticsDto>.Success(result));
    }

    /// <summary>
    /// 获取指定时间范围内的趋势数据，用于绘制统计图表
    /// </summary>
    /// <param name="startDate">起始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>趋势数据点列表</returns>
    [HttpGet("trend")]
    [ProducesResponseType(typeof(ApiResponse<List<TrendDataPoint>>), 200)]
    public async Task<IActionResult> GetTrendData(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await _statisticsService.GetTrendDataAsync(startDate, endDate);
        return Ok(ApiResponse<List<TrendDataPoint>>.Success(result));
    }
}
