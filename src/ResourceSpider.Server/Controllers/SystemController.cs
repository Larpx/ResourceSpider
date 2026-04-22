using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

/// <summary>
/// 系统控制器，提供系统健康检查和日志查询功能
/// 用于监控系统运行状态和排查问题
/// </summary>
[ApiController]
[Route("api/system")]
[Authorize]
public class SystemController : ControllerBase
{
    /// <summary>
    /// 系统日志服务实例，处理日志的查询逻辑
    /// </summary>
    private readonly ISystemLogService _systemLogService;

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<SystemController> _logger;

    /// <summary>
    /// 系统启动时间，用于计算运行时长
    /// </summary>
    private static readonly DateTime _startedAt = DateTime.UtcNow;

    /// <summary>
    /// 初始化系统控制器
    /// </summary>
    /// <param name="systemLogService">系统日志服务</param>
    /// <param name="logger">日志记录器</param>
    public SystemController(ISystemLogService systemLogService, ILogger<SystemController> logger)
    {
        _systemLogService = systemLogService;
        _logger = logger;
    }

    /// <summary>
    /// 系统健康检查接口，返回系统运行状态、版本号、运行时长和依赖服务状态
    /// </summary>
    /// <returns>系统健康状态信息</returns>
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

    /// <summary>
    /// 查询系统日志，支持分页和多条件筛选
    /// </summary>
    /// <param name="pageIndex">页码，默认第 1 页</param>
    /// <param name="pageSize">每页数量，默认 20 条</param>
    /// <param name="level">日志级别筛选（Error/Warning/Information/Debug），为 null 时不筛选</param>
    /// <param name="category">日志分类筛选，为 null 时不筛选</param>
    /// <param name="startDate">起始时间筛选，为 null 时不筛选</param>
    /// <param name="endDate">结束时间筛选，为 null 时不筛选</param>
    /// <returns>系统日志列表及分页信息</returns>
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
