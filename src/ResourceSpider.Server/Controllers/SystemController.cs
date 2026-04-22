using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Observability;
using ResourceSpider.Server.Repositories;
using ResourceSpider.Server.Services;
using StackExchange.Redis;

namespace ResourceSpider.Server.Controllers;

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
    /// Agent 仓储实例
    /// </summary>
    private readonly IAgentRepository _agentRepository;

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
    /// 初始化系统控制器
    /// </summary>
    /// <param name="systemLogService">系统日志服务</param>
    /// <param name="agentRepository">Agent 仓储</param>
    /// <param name="startupState">启动状态</param>
    /// <param name="hostEnvironment">主机环境</param>
    /// <param name="logger">日志记录器</param>
    public SystemController(
        ISystemLogService systemLogService,
        IAgentRepository agentRepository,
        StartupState startupState,
        IHostEnvironment hostEnvironment,
        ILogger<SystemController> logger)
    {
        _systemLogService = systemLogService;
        _agentRepository = agentRepository;
        _startupState = startupState;
        _hostEnvironment = hostEnvironment;
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

        string redisStatus;
        try
        {
            var redis = HttpContext.RequestServices.GetService<IConnectionMultiplexer>();
            redisStatus = redis?.IsConnected == true ? "Connected" : "Unavailable";
        }
        catch (Exception ex)
        {
            redisStatus = $"Unavailable: {ex.Message}";
            _logger.LogWarning(ex, "Redis 状态检查失败");
        }

        var components = new Dictionary<string, string>
        {
            { "database", dbStatus },
            { "redis", redisStatus },
            { "startup", _startupState.DatabaseInitializationSucceeded ? "Ready" : "Partial" }
        };

        var overall = _startupState.DatabaseInitializationSucceeded && redisStatus == "Connected"
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
    /// 获取系统运行时监控详情
    /// </summary>
    /// <returns>系统当前负载、运行状态和日志输出</returns>
    [HttpGet("runtime")]
    [ProducesResponseType(typeof(ApiResponse<SystemRuntimeStatusDto>), 200)]
    public async Task<IActionResult> RuntimeStatus()
    {
        var agents = await _agentRepository.GetAllAsync();
        var onlineAgents = agents.Where(a => a.Status is 1 or 2).ToList();

        var agentLoad = new AgentLoadSnapshotDto(
            TotalAgents: agents.Count,
            OnlineAgents: onlineAgents.Count,
            BusyAgents: agents.Count(a => a.Status == 2),
            TotalRunningTasks: agents.Sum(a => a.TaskCount),
            AverageCpuUsage: onlineAgents.Count == 0 ? 0 : decimal.Round(onlineAgents.Average(a => a.CpuUsage ?? 0), 2),
            AverageMemoryUsage: onlineAgents.Count == 0 ? 0 : decimal.Round(onlineAgents.Average(a => a.MemoryUsage ?? 0), 2));

        var runtimeAgents = agents
            .OrderByDescending(a => a.LastHeartbeat)
            .Take(20)
            .Select(a => new RuntimeAgentStatusDto(
                a.AgentId,
                a.AgentName,
                MapAgentStatus(a.Status),
                a.CpuUsage,
                a.MemoryUsage,
                a.TaskCount,
                a.LastHeartbeat))
            .ToList();

        var recentLogs = await _systemLogService.GetListAsync(pageIndex: 1, pageSize: 20);
        var outputLogs = await ReadOutputLogsAsync();

        var overall = _startupState.DatabaseInitializationSucceeded ? "Healthy" : "Degraded";

        var payload = new SystemRuntimeStatusDto(
            Status: overall,
            Version: GetAppVersion(),
            Environment: _hostEnvironment.EnvironmentName,
            MachineName: Environment.MachineName,
            Framework: RuntimeInformation.FrameworkDescription,
            OsDescription: RuntimeInformation.OSDescription,
            ProcessId: Environment.ProcessId,
            Uptime: DateTime.UtcNow - _startedAt,
            CurrentLoad: BuildLoadSnapshot(),
            AgentLoad: agentLoad,
            Agents: runtimeAgents,
            RecentLogs: recentLogs.Logs,
            OutputLogs: outputLogs,
            TimestampUtc: DateTime.UtcNow);

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

    private static string MapAgentStatus(int status)
    {
        return status switch
        {
            1 => "Online",
            2 => "Busy",
            3 => "Error",
            _ => "Offline"
        };
    }

    private async Task<List<RuntimeLogFileDto>> ReadOutputLogsAsync()
    {
        var logsPath = Path.Combine(_hostEnvironment.ContentRootPath, "logs");
        if (!Directory.Exists(logsPath))
        {
            return [];
        }

        var files = new DirectoryInfo(logsPath)
            .GetFiles("server-*.txt", SearchOption.TopDirectoryOnly)
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .Take(2)
            .ToList();

        var result = new List<RuntimeLogFileDto>(files.Count);
        foreach (var file in files)
        {
            var lines = await ReadTailLinesAsync(file.FullName, 80);
            result.Add(new RuntimeLogFileDto(file.Name, file.LastWriteTimeUtc, lines));
        }

        return result;
    }

    private static async Task<List<string>> ReadTailLinesAsync(string path, int take)
    {
        try
        {
            var lines = await System.IO.File.ReadAllLinesAsync(path);
            return lines
                .TakeLast(take)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();
        }
        catch
        {
            return [];
        }
    }
}
