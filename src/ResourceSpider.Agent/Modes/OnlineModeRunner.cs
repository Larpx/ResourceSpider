using System.Diagnostics;
using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ResourceSpider.Agent.Config;
using ResourceSpider.Agent.Services;
using ResourceSpider.Core;
using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Agent.Modes;

public class OnlineModeRunner : BackgroundService
{
    private readonly SignalRClient _signalRClient;
    private readonly ServerApiClient _serverApiClient;
    private readonly ITaskExecutor _taskExecutor;
    private readonly ResultReporter _resultReporter;
    private readonly OfflineTaskStore _offlineTaskStore;
    private readonly AgentOptions _agentOptions;
    private readonly OnlineModeOptions _options;
    private readonly ILogger<OnlineModeRunner> _logger;

    private readonly ConcurrentDictionary<string, CancellationTokenSource> _runningTasks = new();
    private readonly SemaphoreSlim _concurrencySemaphore;
    private readonly ConcurrentQueue<string> _taskQueue = new();
    private readonly object _queueLock = new();

    private string _agentId = string.Empty;
    private bool _isRegistered;

    public OnlineModeRunner(
        SignalRClient signalRClient,
        ServerApiClient serverApiClient,
        ITaskExecutor taskExecutor,
        ResultReporter resultReporter,
        OfflineTaskStore offlineTaskStore,
        IOptions<AgentOptions> agentOptions,
        ILogger<OnlineModeRunner> logger)
    {
        _signalRClient = signalRClient;
        _serverApiClient = serverApiClient;
        _taskExecutor = taskExecutor;
        _resultReporter = resultReporter;
        _offlineTaskStore = offlineTaskStore;
        _agentOptions = agentOptions.Value;
        _options = agentOptions.Value.ServerConfig;
        _logger = logger;
        _concurrencySemaphore = new SemaphoreSlim(
            _agentOptions.MaxConcurrentTasks > 0 ? _agentOptions.MaxConcurrentTasks : Constants.Defaults.DefaultMaxConcurrentTasks,
            _agentOptions.MaxConcurrentTasks > 0 ? _agentOptions.MaxConcurrentTasks : Constants.Defaults.DefaultMaxConcurrentTasks);

        _signalRClient.OnTaskAssigned += HandleTaskAssigned;
        _signalRClient.OnControlCommand += HandleControlCommand;
        _signalRClient.OnConfigUpdate += HandleConfigUpdate;
        _signalRClient.OnReconnected += () => _ = HandleReconnected();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Online 模式启动，连接服务器: {ServerUrl}", _agentOptions.OnlineMode?.ServerUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RegisterAgentAsync();
                await _signalRClient.ConnectAsync(_agentOptions.OnlineMode?.ServerUrl ?? "", _agentId, stoppingToken);
                _isRegistered = true;

                _logger.LogInformation("Agent {AgentId} 已连接服务器", _agentId);

                var heartbeatTask = StartHeartbeatAsync(stoppingToken);
                var queueProcessingTask = ProcessTaskQueueAsync(stoppingToken);
                var offlineRecoveryTask = RecoverOfflineTasksAsync(stoppingToken);

                await Task.WhenAny(heartbeatTask, queueProcessingTask, offlineRecoveryTask);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Online 模式运行出错，5 秒后重连");
                await Task.Delay(5000, stoppingToken);
            }
        }

        await CleanupAsync();
    }

    private async Task RegisterAgentAsync()
    {
        var systemInfo = CollectSystemMetrics();
        var registerRequest = new RegisterRequest(
            AgentId: _agentId,
            AgentName: $"{Environment.MachineName}-{Environment.UserName}",
            IpAddress: GetLocalIpAddress(),
            Port: 0,
            Capabilities: new[] { "HttpClient", "Playwright" }.ToList(),
            OS: $"{Environment.OSVersion}"
        );

        var result = await _serverApiClient.RegisterAsync(registerRequest);
        if (result != null)
        {
            _agentId = _options.AgentId ?? _agentId;
            _logger.LogInformation("Agent 注册成功，ID: {AgentId}", _agentId);
        }
    }

    private async Task StartHeartbeatAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var interval = _agentOptions.HeartbeatIntervalSeconds > 0
                    ? _agentOptions.HeartbeatIntervalSeconds
                    : Constants.Defaults.DefaultHeartbeatInterval;

                await Task.Delay(interval * 1000, ct);

                var metrics = CollectSystemMetrics();
                metrics.RunningTaskCount = _runningTasks.Count;
                metrics.QueuedTaskCount = _taskQueue.Count;

                await _signalRClient.SendHeartbeatAsync(metrics, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "心跳发送失败");
            }
        }
    }

    private AgentMetrics CollectSystemMetrics()
    {
        var process = Process.GetCurrentProcess();
        var cpuUsage = GetCpuUsagePercent();
        var memoryMB = process.WorkingSet64 / (1024.0 * 1024.0);

        return new AgentMetrics
        {
            CpuUsagePercent = cpuUsage,
            MemoryUsageMB = Math.Round(memoryMB, 2),
            CpuCores = Environment.ProcessorCount,
            TotalMemoryMB = GetTotalPhysicalMemoryMB(),
            AvailableMemoryMB = GetAvailablePhysicalMemoryMB(),
            RunningTaskCount = _runningTasks.Count,
            QueuedTaskCount = _taskQueue.Count,
            UptimeSeconds = (int)(DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalSeconds
        };
    }

    private static double GetCpuUsagePercent()
    {
        try
        {
            var process = Process.GetCurrentProcess();
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

    private static double GetTotalPhysicalMemoryMB()
    {
        try
        {
            var gcInfo = GC.GetGCMemoryInfo();
            return Math.Round(gcInfo.TotalAvailableMemoryBytes / (1024.0 * 1024.0), 2);
        }
        catch { }
        return 0;
    }

    private static double GetAvailablePhysicalMemoryMB()
    {
        try
        {
            var gcInfo = GC.GetGCMemoryInfo();
            return Math.Round(gcInfo.TotalAvailableMemoryBytes / (1024.0 * 1024.0) * 0.8, 2);
        }
        catch { }
        return 0;
    }

    private static string GetLocalIpAddress()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            return ip?.ToString() ?? "127.0.0.1";
        }
        catch { return "127.0.0.1"; }
    }

    private void HandleTaskAssigned(string taskId, string taskContent)
    {
        _logger.LogInformation("收到任务分配: {TaskId}", taskId);

        lock (_queueLock)
        {
            _taskQueue.Enqueue(taskId);
        }

        _ = ProcessTaskQueueItemAsync(taskId, taskContent);
    }

    private async Task ProcessTaskQueueItemAsync(string taskId, string taskContent)
    {
        await _concurrencySemaphore.WaitAsync();

        var cts = new CancellationTokenSource();
        if (!_runningTasks.TryAdd(taskId, cts))
        {
            _concurrencySemaphore.Release();
            return;
        }

        try
        {
            var task = System.Text.Json.JsonSerializer.Deserialize<SpiderTask>(taskContent);
            if (task == null)
            {
                _logger.LogWarning("任务 {TaskId} 反序列化失败", taskId);
                return;
            }

            task.AssignedAgentId = _agentId;
            var result = await _taskExecutor.ExecuteAsync(task, cts.Token);

            await _resultReporter.ReportAsync(result);

            _offlineTaskStore.Remove(taskId);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            _logger.LogWarning("任务 {TaskId} 被取消", taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "任务 {TaskId} 执行异常", taskId);
        }
        finally
        {
            _runningTasks.TryRemove(taskId, out _);
            cts.Dispose();
            _concurrencySemaphore.Release();
        }
    }

    private async Task ProcessTaskQueueAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (_taskQueue.TryDequeue(out var taskId))
                {
                    var taskContent = await _serverApiClient.GetTaskContentAsync(taskId);
                    if (taskContent != null)
                    {
                        var taskJson = System.Text.Json.JsonSerializer.Serialize(taskContent);
                        await ProcessTaskQueueItemAsync(taskId, taskJson);
                    }
                }

                await Task.Delay(1000, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "处理任务队列出错");
                await Task.Delay(5000, ct);
            }
        }
    }

    private async Task RecoverOfflineTasksAsync(CancellationToken ct)
    {
        try
        {
            var pendingTasks = _offlineTaskStore.GetAll();
            foreach (var (taskId, taskContent) in pendingTasks)
            {
                if (ct.IsCancellationRequested) break;
                _logger.LogInformation("恢复离线任务: {TaskId}", taskId);
                await ProcessTaskQueueItemAsync(taskId, taskContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "恢复离线任务出错");
        }
    }

    private void HandleControlCommand(string command, string? targetTaskId = null)
    {
        _logger.LogInformation("收到控制指令: {Command}, 目标任务: {TaskId}", command, targetTaskId ?? "全部");

        switch (command.ToLowerInvariant())
        {
            case "emergency_stop":
                EmergencyStop(targetTaskId);
                break;

            case "pause":
                PauseTask(targetTaskId);
                break;

            case "resume":
                ResumeTask(targetTaskId);
                break;

            case "restart":
                _ = RestartAgentAsync();
                break;

            case "update_config":
                _ = RefreshConfigAsync();
                break;
        }
    }

    private void EmergencyStop(string? targetTaskId)
    {
        if (!string.IsNullOrEmpty(targetTaskId))
        {
            if (_runningTasks.TryGetValue(targetTaskId, out var cts))
            {
                cts.Cancel();
                _logger.LogWarning("紧急停止任务: {TaskId}", targetTaskId);
            }
        }
        else
        {
            foreach (var (taskId, cts) in _runningTasks)
            {
                cts.Cancel();
                _logger.LogWarning("紧急停止任务: {TaskId}", taskId);
            }
        }
    }

    private void PauseTask(string? targetTaskId)
    {
        if (!string.IsNullOrEmpty(targetTaskId) && _runningTasks.TryGetValue(targetTaskId, out var cts))
        {
            _logger.LogInformation("暂停任务: {TaskId}", targetTaskId);
        }
    }

    private void ResumeTask(string? targetTaskId)
    {
        if (!string.IsNullOrEmpty(targetTaskId))
        {
            _logger.LogInformation("恢复任务: {TaskId}", targetTaskId);
        }
    }

    private async Task RestartAgentAsync()
    {
        _logger.LogWarning("收到重启指令，正在重启 Agent...");

        EmergencyStop(null);

        await Task.Delay(2000);

        try
        {
            Process.Start(Environment.ProcessPath ?? "", string.Join(" ", Environment.GetCommandLineArgs().Skip(1)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重启 Agent 失败");
        }

        Environment.Exit(0);
    }

    private async Task RefreshConfigAsync()
    {
        try
        {
            var config = await _serverApiClient.GetConfigAsync();
            if (config != null)
            {
                _logger.LogInformation("配置已更新");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "刷新配置失败");
        }
    }

    private void HandleConfigUpdate(string configJson)
    {
        _logger.LogInformation("收到配置更新: {Config}", configJson);
    }

    private async Task HandleReconnected()
    {
        _logger.LogInformation("SignalR 重新连接成功，重新注册 Agent");
        await RegisterAgentAsync();
    }

    private async Task CleanupAsync()
    {
        foreach (var (_, cts) in _runningTasks)
        {
            cts.Cancel();
        }

        if (_isRegistered && !string.IsNullOrEmpty(_agentId))
        {
            try
            {
                await _serverApiClient.UnregisterAsync(new UnregisterAgentRequest(_agentId, _options.AgentToken, "Agent shutting down"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "注销 Agent 失败");
            }
        }
    }
}

public class AgentMetrics
{
    public double CpuUsagePercent { get; set; }
    public double MemoryUsageMB { get; set; }
    public int CpuCores { get; set; }
    public double TotalMemoryMB { get; set; }
    public double AvailableMemoryMB { get; set; }
    public int RunningTaskCount { get; set; }
    public int QueuedTaskCount { get; set; }
    public int UptimeSeconds { get; set; }
}
