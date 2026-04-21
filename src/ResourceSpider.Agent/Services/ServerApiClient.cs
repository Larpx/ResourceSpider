using System.Net.Http.Json;
using ResourceSpider.Agent.Config;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Agent.Services;

public interface IServerApiClient
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<HeartbeatResponse> HeartbeatAsync(HeartbeatRequest request);
    Task<PullTasksResponse> PullTasksAsync(PullTasksRequest request);
    Task<ReportResponse> ReportTaskAsync(ReportTaskRequest request);
    Task UnregisterAsync(UnregisterAgentRequest request);
    Task<ExpressionConfigDto?> PullExpressionAsync(string expressionId);
    Task<List<ExpressionConfigDto>> PullActiveExpressionsAsync();
    Task<bool> StoreResultsAsync(StoreResultsRequest request);
    Task<bool> ReportExpressionAvailabilityAsync(ReportAvailabilityRequest request);
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
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<TaskDto>>>();
        return new PullTasksResponse
        {
            Tasks = result?.Data ?? new List<TaskDto>(),
            ServerTime = DateTime.UtcNow
        };
    }

    public async Task<ReportResponse> ReportTaskAsync(ReportTaskRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/agent/tasks/report", request);
        response.EnsureSuccessStatusCode();
        return new ReportResponse { Ack = true };
    }

    public async Task UnregisterAsync(UnregisterAgentRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/agent/unregister", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ExpressionConfigDto?> PullExpressionAsync(string expressionId)
    {
        var request = new { AgentId = _options.AgentId, AgentToken = _options.AgentToken, ExpressionId = expressionId };
        var response = await _httpClient.PostAsJsonAsync("/api/agent/expressions/pull", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ExpressionConfigDto>>();
        return result?.Data;
    }

    public async Task<List<ExpressionConfigDto>> PullActiveExpressionsAsync()
    {
        var request = new { AgentId = _options.AgentId, AgentToken = _options.AgentToken };
        var response = await _httpClient.PostAsJsonAsync("/api/agent/expressions/active", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ExpressionConfigDto>>>();
        return result?.Data ?? new List<ExpressionConfigDto>();
    }

    public async Task<bool> StoreResultsAsync(StoreResultsRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/agent/results/store", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return result?.Code == 200;
    }

    public async Task<bool> ReportExpressionAvailabilityAsync(ReportAvailabilityRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/agent/expressions/availability", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return result?.Code == 200;
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
public record HeartbeatResponse(bool Ack, List<TaskDto>? NewTasks, Dictionary<string, object>? ConfigUpdate);
public record PullTasksRequest(string AgentId, string AgentToken, int MaxCount);
public record PullTasksResponse { public List<TaskDto> Tasks { get; set; } = new(); public DateTime ServerTime { get; set; } }
public record ReportTaskRequest(string AgentId, string AgentToken, string TaskId, int Status, int DataCount, int Duration);
public record ReportResponse { public bool Ack { get; set; } public string? NextAction { get; set; } }
public record UnregisterAgentRequest(string AgentId, string AgentToken, string? Reason);

public class TaskDto
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int Status { get; set; }
    public string RequestConfig { get; set; } = "{}";
    public string? ScheduleConfig { get; set; }
    public string? RetryPolicy { get; set; }
    public string? AssignedAgentId { get; set; }
    public decimal Progress { get; set; }
    public int TotalRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int FailedRequests { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ExpressionId { get; set; }
    public ExpressionConfigDto? ExpressionConfig { get; set; }
}

public class ExpressionConfigDto
{
    public string ExpressionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SelectorType { get; set; } = "XPath";
    public string ContainerExpression { get; set; } = string.Empty;
    public List<ExpressionFieldConfigDto> Fields { get; set; } = new();
}

public class ExpressionFieldConfigDto
{
    public string FieldName { get; set; } = string.Empty;
    public string SelectorType { get; set; } = "XPath";
    public string Expression { get; set; } = string.Empty;
    public string? AttributeName { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public string? Formatter { get; set; }
    public string? FormatterArgs { get; set; }
    public int Order { get; set; }
}

public class StoreResultsRequest
{
    public string AgentId { get; set; } = string.Empty;
    public string AgentToken { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string? ExpressionId { get; set; }
    public List<ResultItemDto> Results { get; set; } = new();
}

public class ResultItemDto
{
    public string? ResultId { get; set; }
    public string? SourceUrl { get; set; }
    public Dictionary<string, object?> Fields { get; set; } = new();
    public Dictionary<string, string>? FieldExpressionMap { get; set; }
    public DateTime? CollectedAt { get; set; }
}

public class ReportAvailabilityRequest
{
    public string AgentId { get; set; } = string.Empty;
    public string AgentToken { get; set; } = string.Empty;
    public string ExpressionId { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? FailureReason { get; set; }
}
