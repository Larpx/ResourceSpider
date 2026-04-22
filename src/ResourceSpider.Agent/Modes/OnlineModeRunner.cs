using ResourceSpider.Agent.Config;
using ResourceSpider.Agent.Services;
using ResourceSpider.Core;
using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Agent.Modes;

/// <summary>
/// 在线模式运行器，负责 Agent 与服务端的通信，包括注册、心跳、任务拉取、表达式同步和 SignalR 实时通信
/// </summary>
public class OnlineModeRunner : IHostedService, IAsyncDisposable
{
    /// <summary>
    /// 在线模式配置选项
    /// </summary>
    private readonly OnlineModeOptions _options;

    /// <summary>
    /// 服务端 API 客户端
    /// </summary>
    private readonly IServerApiClient _serverApi;

    /// <summary>
    /// 任务执行器
    /// </summary>
    private readonly ITaskExecutor _taskExecutor;

    /// <summary>
    /// 结果上报器
    /// </summary>
    private readonly IResultReporter _resultReporter;

    /// <summary>
    /// SignalR 客户端
    /// </summary>
    private readonly ISignalRClient _signalRClient;

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<OnlineModeRunner> _logger;

    /// <summary>
    /// 心跳定时器
    /// </summary>
    private Timer? _heartbeatTimer;

    /// <summary>
    /// 任务拉取定时器
    /// </summary>
    private Timer? _taskPullTimer;

    /// <summary>
    /// 表达式同步定时器
    /// </summary>
    private Timer? _expressionSyncTimer;

    /// <summary>
    /// 服务端分配的认证令牌
    /// </summary>
    private string _agentToken = string.Empty;

    /// <summary>
    /// 资源是否已释放
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Agent 是否已成功注册
    /// </summary>
    private bool _isRegistered;

    /// <summary>
    /// 表达式配置本地缓存
    /// </summary>
    private readonly Dictionary<string, ExpressionConfigDto> _expressionCache = new();

    /// <summary>
    /// 缓存读写锁
    /// </summary>
    private readonly object _cacheLock = new();

    /// <summary>
    /// 任务执行信号量，防止并发执行
    /// </summary>
    private readonly SemaphoreSlim _taskExecutionLock = new(1, 1);

    /// <summary>
    /// 初始化在线模式运行器实例
    /// </summary>
    /// <param name="options">在线模式配置选项</param>
    /// <param name="serverApi">服务端 API 客户端</param>
    /// <param name="taskExecutor">任务执行器</param>
    /// <param name="resultReporter">结果上报器</param>
    /// <param name="signalRClient">SignalR 客户端</param>
    /// <param name="logger">日志记录器</param>
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

    /// <summary>
    /// 启动在线模式，执行注册、初始化定时器和 SignalR 连接
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
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

        _signalRClient.OnTaskReceived += (_, message) =>
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

    /// <summary>
    /// 向服务端注册 Agent，获取认证 Token
    /// </summary>
    /// <param name="ct">取消令牌</param>
    private async Task RegisterAsync(CancellationToken ct)
    {
        try
        {
            var request = new RegisterRequest(
                AgentId: _options.AgentId,
                AgentName: _options.AgentName,
                IpAddress: GetLocalIpAddress(),
                Port: 0,
                Capabilities: ["HttpClient", "Playwright"]);

            var response = await _serverApi.RegisterAsync(request);
            _agentToken = response.AgentToken;
            _isRegistered = true;

            _logger.LogInformation("Agent 注册成功，Token: {Token}",
                _agentToken[..Math.Min(8, _agentToken.Length)]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent 注册失败");
        }
    }

    /// <summary>
    /// 定时发送心跳，维持 Agent 在线状态
    /// </summary>
    /// <param name="state">定时器状态对象</param>
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
                Status: (int)AgentStatus.Online);

            await _serverApi.HeartbeatAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "心跳发送失败");
        }
    }

    /// <summary>
    /// 定时同步活跃表达式到本地缓存
    /// </summary>
    /// <param name="state">定时器状态对象</param>
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

    /// <summary>
    /// 拉取并执行任务，使用信号量防止并发执行
    /// </summary>
    /// <param name="state">定时器状态对象</param>
    private async void PullAndExecuteTasks(object? state)
    {
        if (!await _taskExecutionLock.WaitAsync(0)) return;

        try
        {
            var request = new PullTasksRequest(
                AgentId: _options.AgentId,
                AgentToken: _agentToken,
                MaxCount: 5);

            var response = await _serverApi.PullTasksAsync(request);

            foreach (var taskDto in response.Tasks)
            {
                var task = MapTask(taskDto);
                await ResolveExpressionConfig(taskDto, task);

                _logger.LogInformation("执行任务：{TaskName}", task.TaskName);
                var result = await _taskExecutor.ExecuteAsync(task);
                await _resultReporter.ReportAsync(result);

                await ReportExpressionAvailabilityIfNeeded(result);
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

    /// <summary>
    /// 将服务端的任务 DTO 映射为本地任务模型
    /// </summary>
    /// <param name="taskDto">服务端任务 DTO</param>
    /// <returns>本地爬虫任务模型</returns>
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

    /// <summary>
    /// 解析任务的表达式配置，优先使用任务自带配置，其次从缓存或服务端拉取
    /// </summary>
    /// <param name="taskDto">服务端任务 DTO</param>
    /// <param name="task">本地任务模型</param>
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

    /// <summary>
    /// 如果任务关联了表达式，向服务端上报表达式可用性
    /// </summary>
    /// <param name="result">任务执行结果</param>
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

    /// <summary>
    /// 将服务端表达式配置 DTO 映射为本地表达式配置模型
    /// </summary>
    /// <param name="dto">服务端表达式配置 DTO</param>
    /// <returns>本地表达式配置模型，输入为 null 时返回 null</returns>
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

    /// <summary>
    /// 获取本机 IPv4 地址
    /// </summary>
    /// <returns>本机 IPv4 地址字符串</returns>
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

    /// <summary>
    /// 停止在线模式，释放定时器并注销 Agent
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
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

    /// <summary>
    /// 向服务端注销 Agent
    /// </summary>
    /// <param name="ct">取消令牌</param>
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

    /// <summary>
    /// 释放托管资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _heartbeatTimer?.Dispose();
        _taskPullTimer?.Dispose();
        _expressionSyncTimer?.Dispose();
        _taskExecutionLock.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// 异步释放所有资源，包括 SignalR 连接
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _heartbeatTimer?.Dispose();
        _taskPullTimer?.Dispose();
        _expressionSyncTimer?.Dispose();
        _taskExecutionLock.Dispose();

        if (_signalRClient is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }

        _disposed = true;
    }
}
