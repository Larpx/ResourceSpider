using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Observability;
using Larpx.PersonalTools.ResourceSpider.Server.Services;
using StackExchange.Redis;

namespace Larpx.PersonalTools.ResourceSpider.Server.Controllers;

/// <summary>
/// 系统控制器，提供系统健康检查和日志查询功能
/// 用于监控系统运行状态和排查问题
/// </summary>
[ApiController]
[Route("api/admin/system")]
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
    /// 启动阶段状态
    /// </summary>
    private readonly StartupState _startupState;

    /// <summary>
    /// 主机环境信息
    /// </summary>
    private readonly IHostEnvironment _hostEnvironment;

    /// <summary>
    /// 系统启动时间，用于计算运行时长
    /// </summary>
    private static readonly DateTime _startedAt = DateTime.UtcNow;

    /// <summary>
    /// Redis 功能开关服务
    /// </summary>
    private readonly IRedisFeatureService _redisFeatureService;

    /// <summary>
    /// 运行快照服务
    /// </summary>
    private readonly IRuntimeSnapshotService _runtimeSnapshotService;

    /// <summary>
    /// 初始化系统控制器
    /// </summary>
    /// <param name="systemLogService">系统日志服务</param>
    /// <param name="startupState">启动状态</param>
    /// <param name="hostEnvironment">主机环境</param>
    /// <param name="redisFeatureService">Redis 功能开关服务</param>
    /// <param name="runtimeSnapshotService">运行快照服务</param>
    /// <param name="logger">日志记录器</param>
    public SystemController(
        ISystemLogService systemLogService,
        StartupState startupState,
        IHostEnvironment hostEnvironment,
        IRedisFeatureService redisFeatureService,
        IRuntimeSnapshotService runtimeSnapshotService,
        ILogger<SystemController> logger)
    {
        _systemLogService = systemLogService;
        _startupState = startupState;
        _hostEnvironment = hostEnvironment;
        _redisFeatureService = redisFeatureService;
        _runtimeSnapshotService = runtimeSnapshotService;
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
        var dbStatus = _startupState.DatabaseInitializationSucceeded
            ? "Connected"
            : $"Unavailable: {_startupState.DatabaseInitializationError}";

        var redisStatus = BuildRedisStatusText();

        var components = new Dictionary<string, string>
        {
            { "database", dbStatus },
            { "redis", redisStatus },
            { "startup", _startupState.DatabaseInitializationSucceeded ? "Ready" : "Partial" }
        };

        var overall = _startupState.DatabaseInitializationSucceeded
            ? "Healthy"
            : "Degraded";

        var health = new SystemHealthDto(
            overall,
            GetAppVersion(),
            DateTime.UtcNow - _startedAt,
            components,
            BuildLoadSnapshot(),
            _startedAt,
            DateTime.UtcNow,
            _hostEnvironment.EnvironmentName);

        return Ok(ApiResponse<SystemHealthDto>.Success(health));
    }

    /// <summary>
    /// 获取 Redis 功能开关的当前状态
    /// </summary>
    /// <returns>Redis 功能开关状态信息</returns>
    [HttpGet("redis")]
    [ProducesResponseType(typeof(ApiResponse<RedisFeatureStatusDto>), 200)]
    public IActionResult GetRedisStatus()
    {
        var dto = BuildRedisFeatureStatus();
        return Ok(ApiResponse<RedisFeatureStatusDto>.Success(dto));
    }

    /// <summary>
    /// 更新 Redis 功能开关的状态
    /// </summary>
    /// <param name="request">更新请求，包含启用/禁用信息</param>
    /// <returns>更新后的 Redis 功能开关状态</returns>
    [HttpPut("redis")]
    [ProducesResponseType(typeof(ApiResponse<RedisFeatureStatusDto>), 200)]
    public IActionResult UpdateRedisStatus([FromBody] UpdateRedisFeatureRequest request)
    {
        _redisFeatureService.SetEnabled(request.Enabled);
        var dto = BuildRedisFeatureStatus();
        return Ok(ApiResponse<RedisFeatureStatusDto>.Success(dto, "Redis 开关更新成功"));
    }

    /// <summary>
    /// 获取系统运行时监控详情
    /// </summary>
    /// <returns>系统当前负载、运行状态和日志输出</returns>
    [HttpGet("runtime")]
    [ProducesResponseType(typeof(ApiResponse<SystemRuntimeStatusDto>), 200)]
    public async Task<IActionResult> RuntimeStatus()
    {
        var payload = await _runtimeSnapshotService.GetSnapshotAsync();
        return Ok(ApiResponse<SystemRuntimeStatusDto>.Success(payload));
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

    private static string GetAppVersion()
    {
        return Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0";
    }

    private static SystemLoadSnapshotDto BuildLoadSnapshot()
    {
        using var process = Process.GetCurrentProcess();
        var uptimeSeconds = Math.Max((DateTime.UtcNow - _startedAt).TotalSeconds, 1);
        var cpu = process.TotalProcessorTime.TotalSeconds / (uptimeSeconds * Environment.ProcessorCount) * 100;

        ThreadPool.GetAvailableThreads(out var availableWorkers, out _);
        ThreadPool.GetMaxThreads(out var maxWorkers, out _);

        return new SystemLoadSnapshotDto(
            CpuLoadPercent: Math.Round(Math.Clamp(cpu, 0, 100), 2),
            WorkingSetMb: Math.Round(process.WorkingSet64 / 1024d / 1024d, 2),
            GcHeapMb: Math.Round(GC.GetTotalMemory(forceFullCollection: false) / 1024d / 1024d, 2),
            ThreadPoolAvailableWorkers: availableWorkers,
            ThreadPoolMaxWorkers: maxWorkers,
            PendingWorkItems: ThreadPool.PendingWorkItemCount);
    }

    private RedisFeatureStatusDto BuildRedisFeatureStatus()
    {
        var status = !_redisFeatureService.IsConfigured
            ? "NotConfigured"
            : !_redisFeatureService.IsEnabled
                ? "Disabled"
                : _redisFeatureService.IsConnected
                    ? "Connected"
                    : "Unavailable";

        return new RedisFeatureStatusDto(
            Enabled: _redisFeatureService.IsEnabled,
            Configured: _redisFeatureService.IsConfigured,
            Connected: _redisFeatureService.IsConnected,
            TaskContentTtlSeconds: _redisFeatureService.TaskContentTtlSeconds,
            Status: status);
    }

    private string BuildRedisStatusText()
    {
        try
        {
            return BuildRedisFeatureStatus().Status;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis 状态检查失败");
            return $"Unavailable: {ex.Message}";
        }
    }
}
