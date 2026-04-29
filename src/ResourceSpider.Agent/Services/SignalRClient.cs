using Microsoft.AspNetCore.SignalR.Client;
using ResourceSpider.Agent.Config;
using ResourceSpider.Core;

namespace ResourceSpider.Agent.Services;

/// <summary>
/// SignalR 客户端接口，定义 Agent 与服务端之间的实时通信操作
/// </summary>
public interface ISignalRClient
{
    /// <summary>
    /// 连接到服务端 SignalR Hub
    /// </summary>
    /// <param name="serverUrl">服务端地址</param>
    /// <param name="agentId">Agent 标识</param>
    /// <param name="ct">取消令牌</param>
    Task ConnectAsync(string serverUrl, string agentId, CancellationToken ct = default);

    /// <summary>
    /// 停止 SignalR 连接
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// 向服务端发送指定方法调用
    /// </summary>
    /// <param name="method">方法名称</param>
    /// <param name="arg">方法参数</param>
    Task SendAsync(string method, object? arg = null);

    /// <summary>
    /// 发送心跳数据到服务端
    /// </summary>
    /// <param name="metrics">Agent 指标数据</param>
    /// <param name="ct">取消令牌</param>
    Task SendHeartbeatAsync(object metrics, CancellationToken ct = default);

    /// <summary>
    /// 获取当前是否已连接到服务端
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 任务分配事件，当服务端分配新任务时触发
    /// </summary>
    event Action<string, string>? OnTaskAssigned;

    /// <summary>
    /// 控制指令事件，当服务端下发控制命令时触发
    /// </summary>
    event Action<string, string?>? OnControlCommand;

    /// <summary>
    /// 配置更新事件，当服务端推送配置变更时触发
    /// </summary>
    event Action<string>? OnConfigUpdate;

    /// <summary>
    /// 重连成功事件，当 SignalR 连接断开后重新连接成功时触发
    /// </summary>
    event Action? OnReconnected;
}

/// <summary>
/// SignalR 客户端实现，负责 Agent 与服务端的实时通信
/// 支持自动重连、任务分配监听、心跳上报和控制指令接收
/// </summary>
public class SignalRClient : ISignalRClient, IAsyncDisposable
{
    /// <summary>
    /// 在线模式配置选项
    /// </summary>
    private readonly OnlineModeOptions _options;

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<SignalRClient> _logger;

    /// <summary>
    /// SignalR Hub 连接实例
    /// </summary>
    private HubConnection? _hubConnection;

    /// <summary>
    /// 获取当前是否已连接到服务端
    /// </summary>
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    /// <summary>
    /// 任务分配事件，当服务端分配新任务时触发
    /// </summary>
    public event Action<string, string>? OnTaskAssigned;

    /// <summary>
    /// 控制指令事件，当服务端下发控制命令时触发
    /// </summary>
    public event Action<string, string?>? OnControlCommand;

    /// <summary>
    /// 配置更新事件，当服务端推送配置变更时触发
    /// </summary>
    public event Action<string>? OnConfigUpdate;

    /// <summary>
    /// 重连成功事件，当 SignalR 连接断开后重新连接成功时触发
    /// </summary>
    public event Action? OnReconnected;

    /// <summary>
    /// 初始化 SignalR 客户端
    /// </summary>
    /// <param name="options">在线模式配置选项</param>
    /// <param name="logger">日志记录器</param>
    public SignalRClient(OnlineModeOptions options, ILogger<SignalRClient> logger)
    {
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// 连接到服务端 SignalR Hub，注册消息监听和重连策略
    /// </summary>
    /// <param name="serverUrl">服务端地址</param>
    /// <param name="agentId">Agent 标识</param>
    /// <param name="ct">取消令牌</param>
    public async Task ConnectAsync(string serverUrl, string agentId, CancellationToken ct = default)
    {
        var hubUrl = $"{serverUrl.TrimEnd('/')}{Constants.Hub.SpiderHubPath}";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(_options.AgentToken);
            })
            .WithAutomaticReconnect(new ExponentialBackoffRetryPolicy())
            .Build();

        _hubConnection.On<TaskSignalMessage>(Constants.Hub.MethodTaskAssign, message =>
        {
            _logger.LogInformation("收到任务分配信号：{TaskId}", message.TaskId);
            OnTaskAssigned?.Invoke(message.TaskId, System.Text.Json.JsonSerializer.Serialize(message));
        });

        _hubConnection.On<ConfigSignalMessage>(Constants.Hub.MethodConfigUpdate, message =>
        {
            _logger.LogInformation("收到配置更新信号");
            OnConfigUpdate?.Invoke(System.Text.Json.JsonSerializer.Serialize(message.Config));
        });

        _hubConnection.On<ControlSignalMessage>(Constants.Hub.MethodControlCommand, message =>
        {
            _logger.LogInformation("收到控制指令：{Command}", message.Command);
            OnControlCommand?.Invoke(message.Command, message.Data?.ToString());
        });

        _hubConnection.Reconnecting += error =>
        {
            _logger.LogWarning(error, "SignalR 正在重连...");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            _logger.LogInformation("SignalR 重连成功：{ConnectionId}", connectionId);
            OnReconnected?.Invoke();
            return _hubConnection.InvokeAsync(Constants.Hub.MethodJoinAgentGroup, agentId);
        };

        _hubConnection.Closed += error =>
        {
            _logger.LogWarning(error, "SignalR 连接已关闭");
            return Task.CompletedTask;
        };

        try
        {
            await _hubConnection.StartAsync(ct);
            await _hubConnection.InvokeAsync(Constants.Hub.MethodJoinAgentGroup, agentId, cancellationToken: ct);
            _logger.LogInformation("SignalR 连接成功：{HubUrl}", hubUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR 连接失败");
        }
    }

    /// <summary>
    /// 发送心跳数据到服务端，仅在已连接状态下发送
    /// </summary>
    /// <param name="metrics">Agent 指标数据</param>
    /// <param name="ct">取消令牌</param>
    public async Task SendHeartbeatAsync(object metrics, CancellationToken ct = default)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.SendAsync("Heartbeat", metrics, cancellationToken: ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "发送心跳失败");
            }
        }
    }

    /// <summary>
    /// 停止 SignalR 连接
    /// </summary>
    public async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
        }
    }

    /// <summary>
    /// 向服务端发送指定方法调用，仅在已连接状态下发送
    /// </summary>
    /// <param name="method">方法名称</param>
    /// <param name="arg">方法参数</param>
    public async Task SendAsync(string method, object? arg = null)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync(method, arg);
        }
    }

    /// <summary>
    /// 异步释放 SignalR 连接资源
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}

/// <summary>
/// 指数退避重连策略，重连间隔随重试次数指数增长，最大延迟 60 秒
/// </summary>
internal sealed class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    /// <summary>
    /// 最大重连延迟时间
    /// </summary>
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(60);

    /// <summary>
    /// 计算下一次重连的延迟时间
    /// </summary>
    /// <param name="retryContext">重试上下文，包含前一次重试次数等信息</param>
    /// <returns>下一次重连的延迟时间</returns>
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        var retryCount = retryContext.PreviousRetryCount;
        var seconds = Math.Min(Math.Pow(2, retryCount), MaxDelay.TotalSeconds);
        return TimeSpan.FromSeconds(seconds);
    }
}

/// <summary>
/// 任务分配信号消息，服务端通过 SignalR 推送给 Agent 的新任务信息
/// </summary>
public class TaskSignalMessage
{
    /// <summary>
    /// 任务 ID
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 任务名称
    /// </summary>
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型
    /// </summary>
    public string TaskType { get; set; } = string.Empty;

    /// <summary>
    /// 关联的表达式 ID
    /// </summary>
    public string? ExpressionId { get; set; }
}

/// <summary>
/// 配置更新信号消息，服务端通过 SignalR 推送给 Agent 的配置变更
/// </summary>
public class ConfigSignalMessage
{
    /// <summary>
    /// 目标 Agent ID，为 null 时表示广播给所有 Agent
    /// </summary>
    public string? AgentId { get; set; }

    /// <summary>
    /// 配置项字典，包含需要更新的配置键值对
    /// </summary>
    public Dictionary<string, object>? Config { get; set; }
}

/// <summary>
/// 控制指令信号消息，服务端通过 SignalR 下发给 Agent 的控制命令
/// </summary>
public class ControlSignalMessage
{
    /// <summary>
    /// 控制命令，如 emergency_stop、pause、resume、restart、update_config
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// 命令附加数据，如目标任务 ID 等
    /// </summary>
    public object? Data { get; set; }
}
