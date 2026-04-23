using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Observability;
using ResourceSpider.Server.Repositories;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

[ApiController]
[Route("api/admin/system")]
[Authorize]
public class SystemController : ControllerBase
{
    private readonly ISystemLogService _systemLogService;
    private readonly IAgentRepository _agentRepository;
    private readonly ILogger<SystemController> _logger;
    private readonly StartupState _startupState;
    private readonly IHostEnvironment _hostEnvironment;
    private static readonly DateTime _startedAt = DateTime.UtcNow;
    private readonly IRedisFeatureService _redisFeatureService;
    private readonly IPostgreSqlResultStorageFeatureService _postgreFeatureService;
    private readonly ISystemRuntimeSwitchService _runtimeSwitchService;

    public SystemController(
        ISystemLogService systemLogService,
        IAgentRepository agentRepository,
        StartupState startupState,
        IHostEnvironment hostEnvironment,
        IRedisFeatureService redisFeatureService,
        IPostgreSqlResultStorageFeatureService postgreFeatureService,
        ISystemRuntimeSwitchService runtimeSwitchService,
        ILogger<SystemController> logger)
    {
        _systemLogService = systemLogService;
        _agentRepository = agentRepository;
        _startupState = startupState;
        _hostEnvironment = hostEnvironment;
        _redisFeatureService = redisFeatureService;
        _postgreFeatureService = postgreFeatureService;
        _runtimeSwitchService = runtimeSwitchService;
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
        var postgreStatus = BuildPostgreSqlResultStorageStatus().Status;

        var components = new Dictionary<string, string>
        {
            { "database", dbStatus },
            { "redis", redisStatus },
            { "postgreSqlResultStorage", postgreStatus },
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
    public async Task<IActionResult> UpdateRedisStatus([FromBody] UpdateRedisFeatureRequest request)
    {
        var dto = await _runtimeSwitchService.UpdateRedisEnabledAsync(request.Enabled);
        return Ok(ApiResponse<RedisFeatureStatusDto>.Success(dto, "Redis 开关更新成功"));
    }

    /// <summary>
    /// 获取 PostgreSQL 结果存储功能开关的当前状态
    /// </summary>
    /// <returns>PostgreSQL 结果存储功能开关状态信息</returns>
    [HttpGet("postgresql-results")]
    [ProducesResponseType(typeof(ApiResponse<PostgreSqlResultStorageStatusDto>), 200)]
    public IActionResult GetPostgreSqlResultStorageStatus()
    {
        var dto = BuildPostgreSqlResultStorageStatus();
        return Ok(ApiResponse<PostgreSqlResultStorageStatusDto>.Success(dto));
    }

    /// <summary>
    /// 更新 PostgreSQL 结果存储功能开关的状态
    /// </summary>
    /// <param name="request">更新请求，包含启用/禁用信息</param>
    /// <returns>更新后的 PostgreSQL 结果存储功能开关状态</returns>
    [HttpPut("postgresql-results")]
    [ProducesResponseType(typeof(ApiResponse<PostgreSqlResultStorageStatusDto>), 200)]
    public async Task<IActionResult> UpdatePostgreSqlResultStorageStatus([FromBody] UpdatePostgreSqlResultStorageRequest request)
    {
        var dto = await _runtimeSwitchService.UpdatePostgreSqlResultStorageEnabledAsync(request.Enabled);
        return Ok(ApiResponse<PostgreSqlResultStorageStatusDto>.Success(dto, "PostgreSQL 结果存储开关更新成功"));
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
            if (take <= 0)
            {
                return [];
            }

            // 允许日志文件被写入进程共享读取，避免读取时发生文件占用冲突
            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                bufferSize: 4096,
                useAsync: true);

            if (stream.Length == 0)
            {
                return [];
            }

            // 从文件尾部开始，按窗口逐步扩大读取范围。
            // 这样在日志很大时，通常只需读取最后一小段即可拿到需要的尾部行，避免全量读取导致超时。
            var fileLength = stream.Length;
            long windowSize = 16 * 1024; // 16KB 起步
            var tailQueue = new Queue<string>(take);

            while (true)
            {
                var start = Math.Max(0, fileLength - windowSize);
                stream.Seek(start, SeekOrigin.Begin);

                using var reader = new StreamReader(
                    stream,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true,
                    bufferSize: 4096,
                    leaveOpen: true);

                // 非文件开头时先丢弃首个不完整行，避免截断导致脏数据
                if (start > 0)
                {
                    _ = await reader.ReadLineAsync();
                }

                tailQueue.Clear();
                while (true)
                {
                    var line = await reader.ReadLineAsync();
                    if (line is null)
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (tailQueue.Count == take)
                    {
                        tailQueue.Dequeue();
                    }

                    tailQueue.Enqueue(line);
                }

                // 已拿到足够行，或已经扩展到文件开头，则结束
                if (tailQueue.Count >= take || start == 0)
                {
                    return tailQueue.ToList();
                }

                windowSize = Math.Min(fileLength, windowSize * 2);
            }
        }
        catch
        {
            return [];
        }
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
            Status: status,
            LastError: _redisFeatureService.LastError,
            LastConfigWriteError: _redisFeatureService.LastConfigWriteError,
            EffectiveConfigFile: _redisFeatureService.EffectiveConfigFile);
    }

    private PostgreSqlResultStorageStatusDto BuildPostgreSqlResultStorageStatus()
    {
        var status = !_postgreFeatureService.IsConfigured
            ? "NotConfigured"
            : !_postgreFeatureService.IsEnabled
                ? "Disabled"
                : _postgreFeatureService.IsConnected
                    ? "Connected"
                    : "Unavailable";

        return new PostgreSqlResultStorageStatusDto(
            Enabled: _postgreFeatureService.IsEnabled,
            Configured: _postgreFeatureService.IsConfigured,
            Connected: _postgreFeatureService.IsConnected,
            Status: status,
            LastError: _postgreFeatureService.LastError,
            LastConfigWriteError: _postgreFeatureService.LastConfigWriteError,
            EffectiveConfigFile: _postgreFeatureService.EffectiveConfigFile);
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
