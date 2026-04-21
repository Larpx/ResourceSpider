using Microsoft.AspNetCore.SignalR.Client;
using ResourceSpider.Agent.Config;

namespace ResourceSpider.Agent.Services;

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

    public async Task StartAsync(CancellationToken ct = default)
    {
        var hubUrl = $"{_options.ServerUrl.TrimEnd('/')}/hubs/spider";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_options.AgentToken);
            })
            .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30) })
            .Build();

        _hubConnection.On<TaskSignalMessage>("TaskAssign", message =>
        {
            _logger.LogInformation("收到任务分配信号：{TaskId}", message.TaskId);
            OnTaskReceived?.Invoke(this, message);
        });

        _hubConnection.On<ConfigSignalMessage>("ConfigUpdate", message =>
        {
            _logger.LogInformation("收到配置更新信号");
            OnConfigReceived?.Invoke(this, message);
        });

        _hubConnection.On<ControlSignalMessage>("ControlCommand", message =>
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
            return _hubConnection.InvokeAsync("JoinAgentGroup", _options.AgentId);
        };

        _hubConnection.Closed += error =>
        {
            _logger.LogWarning(error, "SignalR 连接已关闭");
            return Task.CompletedTask;
        };

        try
        {
            await _hubConnection.StartAsync(ct);
            await _hubConnection.InvokeAsync("JoinAgentGroup", _options.AgentId, cancellationToken: ct);
            _logger.LogInformation("SignalR 连接成功：{HubUrl}", hubUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR 连接失败");
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
        }
    }

    public async Task SendAsync(string method, object? arg = null)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync(method, arg);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}

public class TaskSignalMessage
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string? ExpressionId { get; set; }
}

public class ConfigSignalMessage
{
    public string? AgentId { get; set; }
    public Dictionary<string, object>? Config { get; set; }
}

public class ControlSignalMessage
{
    public string Command { get; set; } = string.Empty;
    public object? Data { get; set; }
}
