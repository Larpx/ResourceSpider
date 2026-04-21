using ResourceSpider.Agent.Config;
using ResourceSpider.Agent.Services;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ResourceSpider.Infrastructure.Downloader;

namespace ResourceSpider.Agent.Modes;

public class OnlineModeRunner : IHostedService, IDisposable
{
    private readonly OnlineModeOptions _options;
    private readonly IServerApiClient _serverApi;
    private readonly ITaskExecutor _taskExecutor;
    private readonly IResultReporter _resultReporter;
    private readonly ISignalRClient _signalRClient;
    private readonly ILogger<OnlineModeRunner> _logger;
    private Timer? _heartbeatTimer;
    private Timer? _taskPullTimer;
    private Timer? _expressionSyncTimer;
    private string _agentToken = string.Empty;
    private bool _disposed;
    private bool _isRegistered;
    private readonly Dictionary<string, ExpressionConfigDto> _expressionCache = new();
    private readonly object _cacheLock = new();

    public OnlineModeRunner(
        OnlineModeOptions options,
        IServerApiClient serverApi,
        ITaskExecutor taskExecutor,
        IResultReporter resultReporter,
        ISignalRClient signalRClient,
        ILogger<OnlineModeRunner> logger)
    {
        _options = options;
        _serverApi = serverApi;
        _taskExecutor = taskExecutor;
        _resultReporter = resultReporter;
        _signalRClient = signalRClient;
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

        _signalRClient.OnTaskReceived += async (_, message) =>
        {
            _logger.LogInformation("通过 SignalR 收到任务分配：{TaskId}", message.TaskId);
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
                Capabilities: new List<string> { "HttpClient", "Playwright" });

            var response = await _serverApi.RegisterAsync(request);
            _agentToken = response.AgentToken;
            _isRegistered = true;

            _logger.LogInformation("Agent 注册成功，Token: {Token}",
                _agentToken.Substring(0, Math.Min(8, _agentToken.Length)));
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
                Status: 1);

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
        try
        {
            var request = new PullTasksRequest(
                AgentId: _options.AgentId,
                AgentToken: _agentToken,
                MaxCount: 5);

            var response = await _serverApi.PullTasksAsync(request);

            foreach (var taskDto in response.Tasks)
            {
                var task = new Core.Models.SpiderTask
                {
                    TaskId = taskDto.TaskId,
                    TaskName = taskDto.TaskName,
                    TaskType = Enum.TryParse<Core.Enums.TaskType>(taskDto.TaskType, out var tt)
                        ? tt : Core.Enums.TaskType.SinglePage,
                    RequestConfig = new Dictionary<string, object?>
                    {
                        ["Url"] = taskDto.RequestConfig
                    },
                    ExpressionId = taskDto.ExpressionId
                };

                if (taskDto.ExpressionConfig != null)
                {
                    task.ExpressionConfig = MapExpressionConfig(taskDto.ExpressionConfig);
                }
                else if (!string.IsNullOrEmpty(taskDto.ExpressionId))
                {
                    ExpressionConfigDto? cachedExpr;
                    lock (_cacheLock)
                    {
                        _expressionCache.TryGetValue(taskDto.ExpressionId, out cachedExpr);
                    }

                    if (cachedExpr == null)
                    {
                        cachedExpr = await _serverApi.PullExpressionAsync(taskDto.ExpressionId);
                    }

                    if (cachedExpr != null)
                    {
                        task.ExpressionConfig = MapExpressionConfig(cachedExpr);
                    }
                }

                _logger.LogInformation("执行任务：{TaskName}", task.TaskName);
                var result = await _taskExecutor.ExecuteAsync(task);
                await _resultReporter.ReportAsync(result);

                if (!string.IsNullOrEmpty(result.ExpressionId))
                {
                    var isAvailable = result.Status == "Success" && result.DataRecords.Count > 0;
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
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "拉取并执行任务失败");
        }
    }

    private static Core.Models.ExpressionConfig? MapExpressionConfig(ExpressionConfigDto? dto)
    {
        if (dto == null) return null;

        return new Core.Models.ExpressionConfig
        {
            ExpressionId = dto.ExpressionId,
            Name = dto.Name,
            SelectorType = Enum.TryParse<Core.Enums.ExpressionType>(dto.SelectorType, out var t)
                ? t : Core.Enums.ExpressionType.XPath,
            ContainerExpression = dto.ContainerExpression,
            Fields = dto.Fields.Select(f => new Core.Models.ExpressionField
            {
                FieldId = Guid.NewGuid().ToString("N"),
                ExpressionId = dto.ExpressionId,
                FieldName = f.FieldName,
                SelectorType = Enum.TryParse<Core.Enums.ExpressionType>(f.SelectorType, out var ft)
                    ? ft : Core.Enums.ExpressionType.XPath,
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
        _disposed = true;
    }
}
