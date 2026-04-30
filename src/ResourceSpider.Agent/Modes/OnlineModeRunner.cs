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

/// <summary>
/// 在线模式运行器，继承 BackgroundService，用于连接服务端并接收任务调度
/// 通过 SignalR 与服务端通信，支持任务分配、心跳上报、控制指令和配置更新
/// </summary>
public class OnlineModeRunner : BackgroundService
{
    /// <summary>
    /// SignalR 客户端，用于与服务端实时通信
    /// </summary>
    private readonly SignalRClient _signalRClient;

    /// <summary>
    /// 服务端 API 客户端，用于 HTTP API 调用
    /// </summary>
    private readonly ServerApiClient _serverApiClient;

    /// <summary>
    /// 任务执行器
    /// </summary>
    private readonly ITaskExecutor _taskExecutor;

    /// <summary>
    /// 结果上报器
    /// </summary>
    private readonly ResultReporter _resultReporter;

    /// <summary>
    /// 离线任务存储
    /// </summary>
    private readonly OfflineTaskStore _offlineTaskStore;

    /// <summary>
    /// Agent 配置选项
    /// </summary>
    private readonly AgentOptions _agentOptions;

    /// <summary>
    /// 在线模式配置选项
    /// </summary>
    private readonly OnlineModeOptions _options;

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<OnlineModeRunner> _logger;

    /// <summary>
    /// 运行中的任务取消令牌的字典
    /// </summary>
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _runningTasks = new();

    /// <summary>
    /// 并发控制信号量
    /// </summary>
    private readonly SemaphoreSlim _concurrencySemaphore;

    /// <summary>
    /// 待处理任务队列
    /// </summary>
    private readonly ConcurrentQueue<string> _taskQueue = new();

    /// <summary>
    /// 任务队列锁
    /// </summary>
    private readonly object _queueLock = new();

    /// <summary>
    /// Agent 标识符
    /// </summary>
    private string _agentId = string.Empty;

    /// <summary>
    /// Agent 是否已注册到服务端
    /// </summary>
    private bool _isRegistered;

    /// <summary>
    /// 初始化在线模式运行器
    /// </summary>
    /// <param name="signalRClient">SignalR 客户端</param>
    /// <param name="serverApiClient">服务端 API 客户端</param>
    /// <param name="taskExecutor">任务执行器</param>
    /// <param name="resultReporter">结果上报器</param>
    /// <param name="offlineTaskStore">离线任务存储</param>
    /// <param name="agentOptions">Agent 配置选项</param>
    /// <param name="logger">日志记录器</param>
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

    /// <summary>
    /// 后台服务主循环，连接服务端并启动心跳、任务队列处理和离线恢复任务
    /// </summary>
    /// <param name="stoppingToken">取消令牌</param>
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

    /// <summary>
    /// 向服务端注册 Agent，上报系统信息和能力
    /// </summary>
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

    /// <summary>
    /// 定期发送心跳到服务端，上报系统指标和任务状态
    /// </summary>
    /// <param name="ct">取消令牌</param>
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

                var heartbeatResult = await _serverApiClient.HeartbeatAsync(
                    new HeartbeatRequest(
                        _agentId,
                        _options.AgentToken ?? string.Empty,
                        (decimal?)metrics.CpuUsagePercent,
                        (decimal?)metrics.MemoryUsageMB,
                        metrics.RunningTaskCount,
                        1,
                        $"{Environment.OSVersion}",
                        null));

                if (heartbeatResult?.NewToken != null)
                {
                    _options.AgentToken = heartbeatResult.NewToken;
                    _logger.LogInformation("Agent Token 已更新（服务端轮换）");
                }

                if (heartbeatResult?.OtaUpdate != null)
                {
                    _logger.LogInformation(
                        "检测到 OTA 更新: {LatestVersion}，下载地址: {DownloadUrl}，强制更新: {ForceUpdate}",
                        heartbeatResult.OtaUpdate.LatestVersion,
                        heartbeatResult.OtaUpdate.DownloadUrl,
                        heartbeatResult.OtaUpdate.ForceUpdate);

                    if (heartbeatResult.OtaUpdate.ForceUpdate)
                    {
                        _logger.LogWarning("服务端要求强制更新到版本 {LatestVersion}，Agent 将在当前任务完成后退出", heartbeatResult.OtaUpdate.LatestVersion);
                    }
                }
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

    /// <summary>
    /// 收集当前 Agent 的系统指标，包括 CPU、内存、运行任务数等
    /// </summary>
    /// <returns>Agent 指标数据</returns>
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

    /// <summary>
    /// 获取 CPU 使用率百分比
    /// </summary>
    /// <returns>CPU 使用率</returns>
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

    /// <summary>
    /// 获取总物理内存大小（MB）
    /// </summary>
    /// <returns>总物理内存大小</returns>
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

    /// <summary>
    /// 获取可用物理内存大小（MB）
    /// </summary>
    /// <returns>可用物理内存大小</returns>
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

    /// <summary>
    /// 获取本机局域网 IP 地址
    /// </summary>
    /// <returns>IP 地址字符串</returns>
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

    /// <summary>
    /// 处理服务端分配的任务，将任务加入队列并开始处理
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="taskContent">任务配置内容</param>
    private void HandleTaskAssigned(string taskId, string taskContent)
    {
        _logger.LogInformation("收到任务分配: {TaskId}", taskId);

        lock (_queueLock)
        {
            _taskQueue.Enqueue(taskId);
        }

        _ = ProcessTaskQueueItemAsync(taskId, taskContent);
    }

    /// <summary>
    /// 处理单个任务项，包括反序列化、执行、结果上报和离线存储清理
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="taskContent">任务配置内容</param>
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

    /// <summary>
    /// 后台任务队列处理循环，从队列中取出任务并调用 API 获取任务内容
    /// </summary>
    /// <param name="ct">取消令牌</param>
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

    /// <summary>
    /// 恢复离线任务，重新处理之前因网络问题未能上报的任务
    /// </summary>
    /// <param name="ct">取消令牌</param>
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

    /// <summary>
    /// 处理服务端下发的控制指令，如紧急停止、暂停、恢复、重启等
    /// </summary>
    /// <param name="command">控制命令</param>
    /// <param name="targetTaskId">目标任务 ID，可为 null 表示全部</param>
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

    /// <summary>
    /// 紧急停止指定任务或全部任务，通过取消令牌实现
    /// </summary>
    /// <param name="targetTaskId">目标任务 ID，为 null 时停止全部任务</param>
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

    /// <summary>
    /// 暂停指定任务（当前实现仅记录日志）
    /// </summary>
    /// <param name="targetTaskId">目标任务 ID</param>
    private void PauseTask(string? targetTaskId)
    {
        if (!string.IsNullOrEmpty(targetTaskId) && _runningTasks.TryGetValue(targetTaskId, out var cts))
        {
            _logger.LogInformation("暂停任务: {TaskId}", targetTaskId);
        }
    }

    /// <summary>
    /// 恢复指定任务（当前实现仅记录日志）
    /// </summary>
    /// <param name="targetTaskId">目标任务 ID</param>
    private void ResumeTask(string? targetTaskId)
    {
        if (!string.IsNullOrEmpty(targetTaskId))
        {
            _logger.LogInformation("恢复任务: {TaskId}", targetTaskId);
        }
    }

    /// <summary>
    /// 重启 Agent，先停止所有任务，然后启动新进程替换当前进程
    /// </summary>
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

    /// <summary>
    /// 刷新 Agent 配置，从服务端获取最新配置
    /// </summary>
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

    /// <summary>
    /// 处理配置更新信号
    /// </summary>
    /// <param name="configJson">配置 JSON 字符串</param>
    private void HandleConfigUpdate(string configJson)
    {
        _logger.LogInformation("收到配置更新: {Config}", configJson);
    }

    /// <summary>
    /// SignalR 重连成功后的处理，重新注册 Agent
    /// </summary>
    private async Task HandleReconnected()
    {
        _logger.LogInformation("SignalR 重新连接成功，重新注册 Agent");
        await RegisterAgentAsync();
    }

    /// <summary>
    /// 清理资源，停止所有任务并从服务端注销 Agent
    /// </summary>
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

/// <summary>
/// Agent 指标数据模型，用于心跳上报和状态监控
/// </summary>
public class AgentMetrics
{
    /// <summary>
    /// CPU 使用率百分比
    /// </summary>
    public double CpuUsagePercent { get; set; }

    /// <summary>
    /// 内存使用量（MB）
    /// </summary>
    public double MemoryUsageMB { get; set; }

    /// <summary>
    /// CPU 核心数
    /// </summary>
    public int CpuCores { get; set; }

    /// <summary>
    /// 总物理内存（MB）
    /// </summary>
    public double TotalMemoryMB { get; set; }

    /// <summary>
    /// 可用物理内存（MB）
    /// </summary>
    public double AvailableMemoryMB { get; set; }

    /// <summary>
    /// 当前运行中的任务数
    /// </summary>
    public int RunningTaskCount { get; set; }

    /// <summary>
    /// 队列中的任务数
    /// </summary>
    public int QueuedTaskCount { get; set; }

    /// <summary>
    /// Agent 运行时间（秒）
    /// </summary>
    public int UptimeSeconds { get; set; }
}
