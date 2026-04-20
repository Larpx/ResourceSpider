using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

[ApiController]
[Route("api/statistics")]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [HttpGet("agent")]
    [ProducesResponseType(typeof(ApiResponse<List<AgentStatisticsDto>>), 200)]
    public async Task<IActionResult> GetAgentStatistics()
    {
        var result = await _statisticsService.GetAgentStatisticsAsync();
        return Ok(ApiResponse<List<AgentStatisticsDto>>.Success(result));
    }

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

    [HttpGet("system")]
    [ProducesResponseType(typeof(ApiResponse<SystemStatisticsDto>), 200)]
    public async Task<IActionResult> GetSystemStatistics()
    {
        var result = await _statisticsService.GetSystemStatisticsAsync();
        return Ok(ApiResponse<SystemStatisticsDto>.Success(result));
    }

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
