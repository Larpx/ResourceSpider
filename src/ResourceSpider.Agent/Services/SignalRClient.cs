using Microsoft.AspNetCore.SignalR.Client;
using ResourceSpider.Agent.Config;
using ResourceSpider.Core;

namespace ResourceSpider.Agent.Services;

public interface ISignalRClient
{
    Task ConnectAsync(string serverUrl, string agentId, CancellationToken ct = default);
    Task StopAsync();
    Task SendAsync(string method, object? arg = null);
    Task SendHeartbeatAsync(object metrics, CancellationToken ct = default);
    bool IsConnected { get; }

    event Action<string, string>? OnTaskAssigned;
    event Action<string, string?>? OnControlCommand;
    event Action<string>? OnConfigUpdate;
    event Action? OnReconnected;
}

public class SignalRClient : ISignalRClient, IAsyncDisposable
{
    private readonly OnlineModeOptions _options;
    private readonly ILogger<SignalRClient> _logger;
    private HubConnection? _hubConnection;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public event Action<string, string>? OnTaskAssigned;
    public event Action<string, string?>? OnControlCommand;
    public event Action<string>? OnConfigUpdate;
    public event Action? OnReconnected;

    public SignalRClient(OnlineModeOptions options, ILogger<SignalRClient> logger)
    {
        _options = options;
        _logger = logger;
    }

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

internal sealed class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(60);

    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        var retryCount = retryContext.PreviousRetryCount;
        var seconds = Math.Min(Math.Pow(2, retryCount), MaxDelay.TotalSeconds);
        return TimeSpan.FromSeconds(seconds);
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
