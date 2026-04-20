using System.Net.Http.Json;
using ResourceSpider.Agent.Config;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ResourceSpider.Infrastructure.Downloader;
using ResourceSpider.Infrastructure.Duplicate;
using ResourceSpider.Infrastructure.MessageQueue;
using ResourceSpider.Infrastructure.Parser;
using ResourceSpider.Infrastructure.Proxy;
using ResourceSpider.Infrastructure.Scheduler;
using ResourceSpider.Infrastructure.Storage;
using Serilog;

namespace ResourceSpider.Agent.Services;

public interface IServerApiClient
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<HeartbeatResponse> HeartbeatAsync(HeartbeatRequest request);
    Task<PullTasksResponse> PullTasksAsync(PullTasksRequest request);
    Task<ReportResponse> ReportTaskAsync(ReportTaskRequest request);
}

public class ServerApiClient : IServerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly OnlineModeOptions _options;
    private readonly ILogger<ServerApiClient> _logger;

    public ServerApiClient(
        HttpClient httpClient,
        OnlineModeOptions options,
        ILogger<ServerApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_options.ServerUrl);
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/agent/register", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterResponse>>();
        return result?.Data ?? throw new Exception("Registration failed");
    }

    public async Task<HeartbeatResponse> HeartbeatAsync(HeartbeatRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/agent/heartbeat", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<HeartbeatResponse>>();
        return result?.Data ?? throw new Exception("Heartbeat failed");
    }

    public async Task<PullTasksResponse> PullTasksAsync(PullTasksRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/agent/tasks/pull", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PullTaskDto>>>();
        return new PullTasksResponse
        {
            Tasks = result?.Data ?? new List<PullTaskDto>(),
            ServerTime = DateTime.UtcNow
        };
    }

    public async Task<ReportResponse> ReportTaskAsync(ReportTaskRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/agent/tasks/report", request);
        response.EnsureSuccessStatusCode();
        return new ReportResponse { Ack = true };
    }
}

public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public record RegisterRequest(string AgentId, string AgentName, string IpAddress, int Port, List<string>? Capabilities);
public record RegisterResponse(string AgentToken, int HeartbeatInterval, string ServerVersion);
public record HeartbeatRequest(string AgentId, string AgentToken, decimal? CpuUsage, decimal? MemoryUsage, int TaskCount, int Status);
public record HeartbeatResponse(bool Ack, List<PullTaskDto>? NewTasks, Dictionary<string, object>? ConfigUpdate);
public record PullTasksRequest(string AgentId, string AgentToken, int MaxCount);
public record PullTasksResponse { public List<PullTaskDto> Tasks { get; set; } = new(); public DateTime ServerTime { get; set; } }
public record PullTaskDto(string TaskId, string TaskName, string TaskType, string RequestConfig);
public record ReportTaskRequest(string AgentId, string TaskId, int Status, int DataCount, int Duration);
public record ReportResponse { public bool Ack { get; set; } public string? NextAction { get; set; } }
