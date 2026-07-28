using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Observability;
using Larpx.PersonalTools.ResourceSpider.Server.Repositories;

namespace Larpx.PersonalTools.ResourceSpider.Server.Services;

/// <summary>
/// 运行时快照服务，统一生成运行监控页所需的系统状态数据。
/// </summary>
public interface IRuntimeSnapshotService
{
    /// <summary>
    /// 获取当前系统运行时快照。
    /// </summary>
    /// <returns>系统运行时快照</returns>
    Task<SystemRuntimeStatusDto> GetSnapshotAsync();
}

/// <summary>
/// 运行时快照服务实现。
/// </summary>
public class RuntimeSnapshotService : IRuntimeSnapshotService
{
    private readonly IAgentRepository _agentRepository;
    private readonly ISystemLogService _systemLogService;
    private readonly StartupState _startupState;
    private readonly IHostEnvironment _hostEnvironment;
    private static readonly DateTime _startedAt = DateTime.UtcNow;

    public RuntimeSnapshotService(
        IAgentRepository agentRepository,
        ISystemLogService systemLogService,
        StartupState startupState,
        IHostEnvironment hostEnvironment)
    {
        _agentRepository = agentRepository;
        _systemLogService = systemLogService;
        _startupState = startupState;
        _hostEnvironment = hostEnvironment;
    }

    /// <inheritdoc />
    public async Task<SystemRuntimeStatusDto> GetSnapshotAsync()
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
        var outputLogs = RuntimeOutputStream.Snapshot()
            .Select(x => new RuntimeOutputLogDto(
                x.Sequence,
                x.TimestampUtc,
                x.Level,
                x.Source,
                x.Message))
            .ToList();

        var overall = _startupState.DatabaseInitializationSucceeded ? "Healthy" : "Degraded";

        return new SystemRuntimeStatusDto(
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
            RuntimeOutputLogs: outputLogs,
            TimestampUtc: DateTime.UtcNow);
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
}
