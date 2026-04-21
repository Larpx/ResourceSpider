using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

[ApiController]
[Route("api/system")]
[Authorize]
public class SystemController : ControllerBase
{
    private readonly ISystemLogService _systemLogService;
    private readonly ILogger<SystemController> _logger;
    private static readonly DateTime _startedAt = DateTime.UtcNow;

    public SystemController(ISystemLogService systemLogService, ILogger<SystemController> logger)
    {
        _systemLogService = systemLogService;
        _logger = logger;
    }

    [HttpGet("health")]
    [ProducesResponseType(typeof(ApiResponse<SystemHealthDto>), 200)]
    public IActionResult Health()
    {
        var health = new SystemHealthDto(
            "Healthy",
            "1.0.0",
            DateTime.UtcNow - _startedAt,
            new Dictionary<string, string>
            {
                { "database", "Connected" },
                { "redis", "Connected" }
            });

        return Ok(ApiResponse<SystemHealthDto>.Success(health));
    }

    [HttpGet("logs")]
    [ProducesResponseType(typeof(ApiResponse<SystemLogListResponse>), 200)]
    public async Task<IActionResult> GetLogs(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? level = null,
        [FromQuery] string? category = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _systemLogService.GetListAsync(pageIndex, pageSize, level, category, startDate, endDate);
        return Ok(ApiResponse<SystemLogListResponse>.Success(result));
    }
}
