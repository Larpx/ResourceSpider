using ResourceSpider.Agent.Config;
using ResourceSpider.Agent.Services;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Infrastructure.Downloader;

namespace ResourceSpider.Agent.Modes;

public class OnlineModeRunner : IHostedService, IDisposable
{
    private readonly OnlineModeOptions _options;
    private readonly IServerApiClient _serverApi;
    private readonly ITaskExecutor _taskExecutor;
    private readonly IResultReporter _resultReporter;
    private readonly ILogger<OnlineModeRunner> _logger;
    private Timer? _heartbeatTimer;
    private Timer? _taskPullTimer;
    private string _agentToken = string.Empty;
    private bool _disposed;
    private bool _isRegistered;

    public OnlineModeRunner(
        OnlineModeOptions options,
        IServerApiClient serverApi,
        ITaskExecutor taskExecutor,
        IResultReporter resultReporter,
        ILogger<OnlineModeRunner> logger)
    {
        _options = options;
        _serverApi = serverApi;
        _taskExecutor = taskExecutor;
        _resultReporter = resultReporter;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Online Mode Agent");
        
        await RegisterAsync(cancellationToken);
        
        if (!_isRegistered)
        {
            _logger.LogError("Failed to register agent with server");
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
            
            _logger.LogInformation("Agent registered with token: {Token}", 
                _agentToken.Substring(0, Math.Min(8, _agentToken.Length)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register agent");
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
            _logger.LogError(ex, "Failed to send heartbeat");
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
                    TaskType = taskDto.TaskType,
                    RequestConfig = new Dictionary<string, object?>
                    {
                        ["Url"] = taskDto.RequestConfig
                    }
                };

                _logger.LogInformation("Executing task: {TaskName}", task.TaskName);
                var result = await _taskExecutor.ExecuteAsync(task);
                await _resultReporter.ReportAsync(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pull and execute tasks");
        }
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
        _logger.LogInformation("Stopping Online Mode Agent");
        _heartbeatTimer?.Change(Timeout.Infinite, 0);
        _taskPullTimer?.Change(Timeout.Infinite, 0);

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
            _logger.LogInformation("Agent {AgentId} unregistered from server", _options.AgentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unregister agent {AgentId}", _options.AgentId);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _heartbeatTimer?.Dispose();
        _taskPullTimer?.Dispose();
        _disposed = true;
    }
}
