using Microsoft.AspNetCore.SignalR.Client;
using ResourceSpider.Agent.Config;
using ResourceSpider.Core;

namespace ResourceSpider.Agent.Services;

/// <summary>
/// SignalR 客户端接口，定义与服务端实时通信的方法
/// </summary>
public interface ISignalRClient
{
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync();
    Task SendAsync(string method, object? arg = null);
    bool IsConnected { get; }
    event EventHandler<TaskSignalMessage>? OnTaskReceived;
    event EventHandler<ConfigSignalMessage>? OnConfigReceived;
    event EventHandler<ControlSignalMessage>? OnControlCommand;
}

/// <summary>
/// SignalR 客户端实现，负责与服务端建立 WebSocket 连接并处理消息收发
/// 支持自动重连、心跳检测和消息确认机制
/// </summary>
public class SignalRClient : ISignalRClient, IAsyncDisposable
{
    private readonly OnlineModeOptions _options;
    private readonly ILogger<SignalRClient> _logger;
    private HubConnection? _hubConnection;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public event EventHandler<TaskSignalMessage>? OnTaskReceived;
    public event EventHandler<ConfigSignalMessage>? OnConfigReceived;
    public event EventHandler<ControlSignalMessage>? OnControlCommand;

    public SignalRClient(OnlineModeOptions options, ILogger<SignalRClient> logger)
    {
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// 启动 SignalR 连接，注册消息处理器并连接到服务端 Hub
    /// </summary>
    public async Task StartAsync(CancellationToken ct = default)
    {
        var hubUrl = $"{_options.ServerUrl.TrimEnd('/')}{Constants.Hub.SpiderHubPath}";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(_options.AgentToken);
            })
            .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30) })
            .Build();

        _hubConnection.On<TaskSignalMessage>(Constants.Hub.MethodTaskAssign, message =>
        {
            _logger.LogInformation("收到任务分配信号：{TaskId}", message.TaskId);
            OnTaskReceived?.Invoke(this, message);
        });

        _hubConnection.On<ConfigSignalMessage>(Constants.Hub.MethodConfigUpdate, message =>
        {
            _logger.LogInformation("收到配置更新信号");
            OnConfigReceived?.Invoke(this, message);
        });

        _hubConnection.On<ControlSignalMessage>(Constants.Hub.MethodControlCommand, message =>
        {
            _logger.LogInformation("收到控制指令：{Command}", message.Command);
            OnControlCommand?.Invoke(this, message);
        });

        _hubConnection.Reconnecting += error =>
        {
            _logger.LogWarning(error, "SignalR 正在重连...");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            _logger.LogInformation("SignalR 重连成功：{ConnectionId}", connectionId);
            return _hubConnection.InvokeAsync(Constants.Hub.MethodJoinAgentGroup, _options.AgentId);
        };

        _hubConnection.Closed += error =>
        {
            _logger.LogWarning(error, "SignalR 连接已关闭");
            return Task.CompletedTask;
        };

        try
        {
            await _hubConnection.StartAsync(ct);
            await _hubConnection.InvokeAsync(Constants.Hub.MethodJoinAgentGroup, _options.AgentId, cancellationToken: ct);
            _logger.LogInformation("SignalR 连接成功：{HubUrl}", hubUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR 连接失败");
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
    /// 向服务端发送消息
    /// </summary>
    public async Task SendAsync(string method, object? arg = null)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync(method, arg);
        }
    }

    /// <summary>
    /// 异步释放资源，断开 SignalR 连接
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
/// 任务分配信号消息，由服务端通过 SignalR 下发给 Agent
/// </summary>
public class TaskSignalMessage
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string? ExpressionId { get; set; }
}

/// <summary>
/// 配置更新信号消息，通知 Agent 采集规则已变更
/// </summary>
public class ConfigSignalMessage
{
    public string? AgentId { get; set; }
    public Dictionary<string, object>? Config { get; set; }
}

/// <summary>
/// 控制指令信号消息，用于远程控制 Agent（暂停/恢复/终止等）
/// </summary>
public class ControlSignalMessage
{
    public string Command { get; set; } = string.Empty;
    public object? Data { get; set; }
}
