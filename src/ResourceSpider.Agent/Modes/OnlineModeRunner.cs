using ResourceSpider.Agent.Config;
using ResourceSpider.Agent.Services;
using ResourceSpider.Core;
using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Agent.Modes;

public class OnlineModeRunner : IHostedService, IAsyncDisposable
{
    private readonly OnlineModeOptions _options;
    private readonly IServerApiClient _serverApi;
    private readonly ITaskExecutor _taskExecutor;
    private readonly IResultReporter _resultReporter;
    private readonly ISignalRClient _signalRClient;
    private readonly IOfflineTaskStore _offlineStore;
    private readonly ILogger<OnlineModeRunner> _logger;

    private Timer? _heartbeatTimer;
    private Timer? _taskPullTimer;
    private Timer? _expressionSyncTimer;
    private Timer? _offlineSyncTimer;

    private string _agentToken = string.Empty;
    private bool _disposed;
    private bool _isRegistered;

    private readonly Dictionary<string, ExpressionConfigDto> _expressionCache = new();
    private readonly object _cacheLock = new();
    private readonly SemaphoreSlim _taskExecutionLock = new(1, 1);

    public OnlineModeRunner(
        OnlineModeOptions options,
        IServerApiClient serverApi,
        ITaskExecutor taskExecutor,
        IResultReporter resultReporter,
        ISignalRClient signalRClient,
        IOfflineTaskStore offlineStore,
        ILogger<OnlineModeRunner> logger)
    {
        _options = options;
        _serverApi = serverApi;
        _taskExecutor = taskExecutor;
        _resultReporter = resultReporter;
        _signalRClient = signalRClient;
        _offlineStore = offlineStore;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("启动在线模式 Agent");

        await RegisterAsync(cancellationToken);

        if (!_isRegistered)
        {
            _logger.LogError("Agent 注册失败");
            return;
        }

        await SyncOfflineResultsAsync();

        _heartbeatTimer = new Timer(
            SendHeartbeat, null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(_options.HeartbeatInterval));

        _taskPullTimer = new Timer(
            PullAndExecuteTasks, null,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(30));

        _expressionSyncTimer = new Timer(
            SyncExpressions, null,
            TimeSpan.FromSeconds(10),
            TimeSpan.FromMinutes(5));

        _offlineSyncTimer = new Timer(
            SyncOfflineResults, null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(_options.OfflineSyncIntervalMinutes));

        _signalRClient.OnTaskReceived += async (_, message) =>
        {
            _logger.LogInformation("通过 SignalR 收到任务分配：{TaskId}", message.TaskId);
            await ExecuteAssignedTaskAsync(message.TaskId);
        };

        _signalRClient.OnControlCommand += (_, message) =>
        {
            _logger.LogInformation("收到控制指令：{Command}", message.Command);
        };

        try
        {
            await _signalRClient.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR 连接失败，将使用轮询模式");
        }
    }

    private async Task RegisterAsync(CancellationToken ct)
    {
        try
        {
            var request = new RegisterRequest(
                AgentId: _options.AgentId,
                AgentName: _options.AgentName,
                IpAddress: GetLocalIpAddress(),
                Port: 0,
                Capabilities: ["HttpClient", "Playwright", "BrowserAutomation"],
                OS: Environment.OSVersion.ToString(),
                Version: typeof(Program).Assembly.GetName().Version?.ToString());

            var response = await _serverApi.RegisterAsync(request);
            _agentToken = response.AgentToken;
            _options.AgentToken = response.AgentToken;

            if (response.HeartbeatInterval > 0)
            {
                _options.HeartbeatInterval = response.HeartbeatInterval;
            }

            _isRegistered = true;

            _logger.LogInformation("Agent 注册成功，Token: {Token}",
                _agentToken[..Math.Min(8, _agentToken.Length)]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent 注册失败");
        }
    }

    private async void SendHeartbeat(object? state)
    {
        try
        {
            var request = new HeartbeatRequest(
                AgentId: _options.AgentId,
                AgentToken: _agentToken,
                CpuUsage: 0,
                MemoryUsage: 0,
                TaskCount: 0,
                Status: (int)AgentStatus.Online,
                OS: Environment.OSVersion.ToString(),
                Version: typeof(Program).Assembly.GetName().Version?.ToString());

            await _serverApi.HeartbeatAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "心跳发送失败");
        }
    }

    private async void SyncExpressions(object? state)
    {
        try
        {
            var expressions = await _serverApi.PullActiveExpressionsAsync();

            lock (_cacheLock)
            {
                _expressionCache.Clear();
                foreach (var expr in expressions)
                {
                    _expressionCache[expr.ExpressionId] = expr;
                }
            }

            _logger.LogInformation("同步 {Count} 个活跃表达式", expressions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "表达式同步失败");
        }
    }

    private async void PullAndExecuteTasks(object? state)
    {
        if (!await _taskExecutionLock.WaitAsync(0)) return;

        try
        {
            var request = new PullTasksRequest(
                AgentId: _options.AgentId,
                AgentToken: _agentToken,
                MaxCount: _options.MaxConcurrentTasks);

            var response = await _serverApi.PullTasksAsync(request);

            foreach (var taskDto in response.Tasks)
            {
                await ExecuteTaskDtoAsync(taskDto);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "拉取并执行任务失败");
        }
        finally
        {
            _taskExecutionLock.Release();
        }
    }

    private async Task ExecuteAssignedTaskAsync(string taskId)
    {
        if (string.IsNullOrWhiteSpace(taskId))
        {
            return;
        }

        if (!await _taskExecutionLock.WaitAsync(0))
        {
            _logger.LogInformation("当前已有任务执行中，SignalR 下发任务 {TaskId} 将等待下一轮拉取补偿", taskId);
            return;
        }

        try
        {
            var taskDto = await _serverApi.GetTaskContentAsync(taskId);
            if (taskDto == null)
            {
                _logger.LogWarning("服务端未返回任务 {TaskId} 的完整内容", taskId);
                return;
            }

            await ExecuteTaskDtoAsync(taskDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行 SignalR 下发任务 {TaskId} 失败", taskId);
        }
        finally
        {
            _taskExecutionLock.Release();
        }
    }

    private async Task ExecuteTaskDtoAsync(TaskDto taskDto)
    {
        var task = MapTask(taskDto);
        await ResolveExpressionConfig(taskDto, task);

        _logger.LogInformation("执行任务：{TaskName}", task.TaskName);
        var result = await _taskExecutor.ExecuteAsync(task);

        await ReportStepResultsAsync(result);
        await _resultReporter.ReportAsync(result);
        await ReportExpressionAvailabilityIfNeeded(result);
    }

    private async Task ReportStepResultsAsync(ExecutionResult result)
    {
        foreach (var stepResult in result.StepResults)
        {
            try
            {
                var reportRequest = new ReportStepStatusRequest(
                    AgentId: _options.AgentId,
                    AgentToken: _agentToken,
                    TaskId: result.TaskId,
                    StepId: stepResult.StepId,
                    State: (int)stepResult.State,
                    DataCount: stepResult.DataCount
                );

                await _serverApi.ReportStepStatusAsync(reportRequest);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "上报步骤 {StepId} 状态失败", stepResult.StepId);
            }
        }
    }

    private async void SyncOfflineResults(object? state)
    {
        await SyncOfflineResultsAsync();
    }

    private async Task SyncOfflineResultsAsync()
    {
        try
        {
            var pendingResults = await _offlineStore.GetPendingResultsAsync();
            if (pendingResults.Count == 0) return;

            _logger.LogInformation("发现 {Count} 条待上传的离线结果", pendingResults.Count);

            foreach (var result in pendingResults)
            {
                try
                {
                    await _resultReporter.ReportAsync(result);
                    await _offlineStore.MarkResultUploadedAsync(result.TaskId);
                    _logger.LogInformation("离线结果已上传：{TaskId}", result.TaskId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "上传离线结果失败：{TaskId}", result.TaskId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "同步离线结果失败");
        }
    }

    private static SpiderTask MapTask(TaskDto taskDto)
    {
        return new SpiderTask
        {
            TaskId = taskDto.TaskId,
            TaskName = taskDto.TaskName,
            TaskType = Enum.TryParse<TaskType>(taskDto.TaskType, out var tt)
                ? tt : TaskType.SinglePage,
            RequestConfig = new Dictionary<string, object?>
            {
                ["Url"] = taskDto.RequestConfig
            },
            ExpressionId = taskDto.ExpressionId
        };
    }

    private async Task ResolveExpressionConfig(TaskDto taskDto, SpiderTask task)
    {
        if (taskDto.ExpressionConfig != null)
        {
            task.ExpressionConfig = MapExpressionConfig(taskDto.ExpressionConfig);
            return;
        }

        if (string.IsNullOrEmpty(taskDto.ExpressionId)) return;

        ExpressionConfigDto? cachedExpr;
        lock (_cacheLock)
        {
            _expressionCache.TryGetValue(taskDto.ExpressionId, out cachedExpr);
        }

        cachedExpr ??= await _serverApi.PullExpressionAsync(taskDto.ExpressionId);

        if (cachedExpr != null)
        {
            task.ExpressionConfig = MapExpressionConfig(cachedExpr);
        }
    }

    private async Task ReportExpressionAvailabilityIfNeeded(ExecutionResult result)
    {
        if (string.IsNullOrEmpty(result.ExpressionId)) return;

        var isAvailable = result.Status == Constants.ExecutionStatus.Success && result.DataRecords.Count > 0;
        var failureReason = isAvailable ? null : string.Join("; ", result.Errors.Take(3));

        await _serverApi.ReportExpressionAvailabilityAsync(
            new ReportAvailabilityRequest
            {
                AgentId = _options.AgentId,
                AgentToken = _agentToken,
                ExpressionId = result.ExpressionId,
                IsAvailable = isAvailable,
                FailureReason = failureReason
            });
    }

    private static ExpressionConfig? MapExpressionConfig(ExpressionConfigDto? dto)
    {
        if (dto == null) return null;

        return new ExpressionConfig
        {
            ExpressionId = dto.ExpressionId,
            Name = dto.Name,
            SelectorType = Enum.TryParse<ExpressionType>(dto.SelectorType, out var t)
                ? t : ExpressionType.XPath,
            ContainerExpression = dto.ContainerExpression,
            Fields = dto.Fields.Select(f => new ExpressionField
            {
                FieldId = Guid.NewGuid().ToString("N"),
                ExpressionId = dto.ExpressionId,
                FieldName = f.FieldName,
                SelectorType = Enum.TryParse<ExpressionType>(f.SelectorType, out var ft)
                    ? ft : ExpressionType.XPath,
                Expression = f.Expression,
                AttributeName = f.AttributeName,
                IsRequired = f.IsRequired,
                DefaultValue = f.DefaultValue,
                Formatter = f.Formatter,
                FormatterArgs = f.FormatterArgs,
                Order = f.Order
            }).ToList()
        };
    }

    private static string GetLocalIpAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("停止在线模式 Agent");
        _heartbeatTimer?.Change(Timeout.Infinite, 0);
        _taskPullTimer?.Change(Timeout.Infinite, 0);
        _expressionSyncTimer?.Change(Timeout.Infinite, 0);
        _offlineSyncTimer?.Change(Timeout.Infinite, 0);

        await _signalRClient.StopAsync();

        if (_isRegistered)
        {
            await UnregisterAsync(cancellationToken);
        }
    }

    private async Task UnregisterAsync(CancellationToken ct)
    {
        try
        {
            await _serverApi.UnregisterAsync(new UnregisterAgentRequest(
                AgentId: _options.AgentId,
                AgentToken: _agentToken,
                Reason: "Agent shutting down"));
            _isRegistered = false;
            _logger.LogInformation("Agent {AgentId} 已注销", _options.AgentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent {AgentId} 注销失败", _options.AgentId);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _heartbeatTimer?.Dispose();
        _taskPullTimer?.Dispose();
        _expressionSyncTimer?.Dispose();
        _offlineSyncTimer?.Dispose();
        _taskExecutionLock.Dispose();
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _heartbeatTimer?.Dispose();
        _taskPullTimer?.Dispose();
        _expressionSyncTimer?.Dispose();
        _offlineSyncTimer?.Dispose();
        _taskExecutionLock.Dispose();

        if (_signalRClient is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }

        _disposed = true;
    }
}
