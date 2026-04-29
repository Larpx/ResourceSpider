using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface ISystemMonitorService
{
    Task<SystemMonitorMetrics> GetCurrentMetricsAsync();
    Task<HealthCheckResult> CheckHealthAsync();
}

public class SystemMonitorService : ISystemMonitorService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SystemMonitorService> _logger;

    public SystemMonitorService(
        IServiceProvider serviceProvider,
        ILogger<SystemMonitorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task<SystemMonitorMetrics> GetCurrentMetricsAsync()
    {
        var process = Process.GetCurrentProcess();
        var gcInfo = GC.GetGCMemoryInfo();

        var metrics = new SystemMonitorMetrics
        {
            CpuUsagePercent = GetCpuUsage(process),
            MemoryUsageMB = process.WorkingSet64 / (1024.0 * 1024.0),
            GcTotalMemoryMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0),
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount,
            Uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime(),
            TotalAllocatedMemoryMB = gcInfo.TotalAvailableMemoryBytes / (1024.0 * 1024.0)
        };

        return Task.FromResult(metrics);
    }

    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        var result = new HealthCheckResult { Status = "Healthy", Checks = new Dictionary<string, string>() };

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var agentRepository = scope.ServiceProvider.GetRequiredService<IAgentRepository>();
            var onlineAgents = await agentRepository.GetOnlineAgentsAsync();
            result.Checks["Agents"] = $"Online: {onlineAgents.Count}";
        }
        catch (Exception ex)
        {
            result.Checks["Agents"] = $"Error: {ex.Message}";
            result.Status = "Degraded";
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            var pendingTasks = await taskRepository.GetPendingTasksAsync(1);
            result.Checks["Database"] = "Connected";
        }
        catch (Exception ex)
        {
            result.Checks["Database"] = $"Error: {ex.Message}";
            result.Status = "Unhealthy";
        }

        return result;
    }

    private static double GetCpuUsage(Process process)
    {
        try
        {
            var cpuTime = process.TotalProcessorTime;
            var upTime = DateTime.UtcNow - process.StartTime.ToUniversalTime();
            if (upTime.TotalMilliseconds > 0)
            {
                return Math.Round(cpuTime.TotalMilliseconds / (upTime.TotalMilliseconds * Environment.ProcessorCount) * 100, 2);
            }
        }
        catch { }
        return 0;
    }
}

public class SystemMonitorMetrics
{
    public double CpuUsagePercent { get; set; }
    public double MemoryUsageMB { get; set; }
    public double GcTotalMemoryMB { get; set; }
    public double TotalAllocatedMemoryMB { get; set; }
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public TimeSpan Uptime { get; set; }
}

public class HealthCheckResult
{
    public string Status { get; set; } = "Healthy";
    public Dictionary<string, string> Checks { get; set; } = new();
}
