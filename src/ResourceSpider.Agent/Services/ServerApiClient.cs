using System.Net.Http.Json;
using ResourceSpider.Agent.Config;
using ResourceSpider.Core;
using ResourceSpider.Core.Exceptions;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Agent.Services;

/// <summary>
/// 服务端 API 客户端接口，定义 Agent 与服务端通信的所有方法
/// </summary>
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

/// <summary>
/// 服务端 API 客户端实现，通过 HTTP 调用服务端 RESTful API
/// </summary>
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
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentRegister, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterResponse>>();
        return result?.Data ?? throw new SpiderException("Agent 注册失败");
    }

    public async Task<HeartbeatResponse> HeartbeatAsync(HeartbeatRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentHeartbeat, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<HeartbeatResponse>>();
        return result?.Data ?? throw new SpiderException("心跳发送失败");
    }

    public async Task<PullTasksResponse> PullTasksAsync(PullTasksRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentPullTasks, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<TaskDto>>>();
        return new PullTasksResponse
        {
            Tasks = result?.Data ?? [],
            ServerTime = DateTime.UtcNow
        };
    }

    public async Task<ReportResponse> ReportTaskAsync(ReportTaskRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentReportTask, request);
        response.EnsureSuccessStatusCode();
        return new ReportResponse { Ack = true };
    }

    public async Task UnregisterAsync(UnregisterAgentRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentUnregister, request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ExpressionConfigDto?> PullExpressionAsync(string expressionId)
    {
        var request = new { AgentId = _options.AgentId, AgentToken = _options.AgentToken, ExpressionId = expressionId };
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentPullExpression, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ExpressionConfigDto>>();
        return result?.Data;
    }

    public async Task<List<ExpressionConfigDto>> PullActiveExpressionsAsync()
    {
        var request = new { AgentId = _options.AgentId, AgentToken = _options.AgentToken };
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentActiveExpressions, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ExpressionConfigDto>>>();
        return result?.Data ?? [];
    }

    public async Task<bool> StoreResultsAsync(StoreResultsRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentStoreResults, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return result?.Code == 200;
    }

    public async Task<bool> ReportExpressionAvailabilityAsync(ReportAvailabilityRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentExpressionAvailability, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return result?.Code == 200;
    }
}

/// <summary>
/// Agent 注册请求
/// </summary>
public record RegisterRequest(string AgentId, string AgentName, string IpAddress, int Port, List<string>? Capabilities);

/// <summary>
/// Agent 注册响应
/// </summary>
public record RegisterResponse(string AgentToken, int HeartbeatInterval, string ServerVersion);

/// <summary>
/// 心跳请求
/// </summary>
public record HeartbeatRequest(string AgentId, string AgentToken, decimal? CpuUsage, decimal? MemoryUsage, int TaskCount, int Status);

/// <summary>
/// 心跳响应
/// </summary>
public record HeartbeatResponse(bool Ack, List<TaskDto>? NewTasks, Dictionary<string, object>? ConfigUpdate);

/// <summary>
/// 拉取任务请求
/// </summary>
public record PullTasksRequest(string AgentId, string AgentToken, int MaxCount);

/// <summary>
/// 拉取任务响应
/// </summary>
public record PullTasksResponse
{
    public List<TaskDto> Tasks { get; set; } = [];
    public DateTime ServerTime { get; set; }
}

/// <summary>
/// 上报任务结果请求
/// </summary>
public record ReportTaskRequest(string AgentId, string AgentToken, string TaskId, int Status, int DataCount, int Duration);

/// <summary>
/// 上报任务结果响应
/// </summary>
public record ReportResponse
{
    public bool Ack { get; set; }
    public string? NextAction { get; set; }
}

/// <summary>
/// Agent 注销请求
/// </summary>
public record UnregisterAgentRequest(string AgentId, string AgentToken, string? Reason);

/// <summary>
/// 任务数据传输对象
/// </summary>
public class TaskDto
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string TaskType { get; set; } = Constants.Defaults.DefaultTaskType;
    public int Priority { get; set; } = Constants.Defaults.DefaultPriority;
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

/// <summary>
/// 表达式配置数据传输对象
/// </summary>
public class ExpressionConfigDto
{
    public string ExpressionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SelectorType { get; set; } = Constants.Defaults.DefaultSelectorType;
    public string ContainerExpression { get; set; } = string.Empty;
    public List<ExpressionFieldConfigDto> Fields { get; set; } = [];
}

/// <summary>
/// 表达式字段配置数据传输对象
/// </summary>
public class ExpressionFieldConfigDto
{
    public string FieldName { get; set; } = string.Empty;
    public string SelectorType { get; set; } = Constants.Defaults.DefaultSelectorType;
    public string Expression { get; set; } = string.Empty;
    public string? AttributeName { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public string? Formatter { get; set; }
    public string? FormatterArgs { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// 存储采集结果请求
/// </summary>
public class StoreResultsRequest
{
    public string AgentId { get; set; } = string.Empty;
    public string AgentToken { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string? ExpressionId { get; set; }
    public List<ResultItemDto> Results { get; set; } = [];
}

/// <summary>
/// 单条采集结果数据传输对象
/// </summary>
public class ResultItemDto
{
    public string? ResultId { get; set; }
    public string? SourceUrl { get; set; }
    public Dictionary<string, object?> Fields { get; set; } = new();
    public Dictionary<string, string>? FieldExpressionMap { get; set; }
    public DateTime? CollectedAt { get; set; }
}

/// <summary>
/// 上报表达式可用性请求
/// </summary>
public class ReportAvailabilityRequest
{
    public string AgentId { get; set; } = string.Empty;
    public string AgentToken { get; set; } = string.Empty;
    public string ExpressionId { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? FailureReason { get; set; }
}
