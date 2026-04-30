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
    Task<TaskDto?> GetTaskContentAsync(string taskId);
    Task<ExpressionConfigDto?> PullExpressionAsync(string expressionId);
    Task<List<ExpressionConfigDto>> PullActiveExpressionsAsync();
    Task<bool> StoreResultsAsync(StoreResultsRequest request);
    Task<bool> ReportExpressionAvailabilityAsync(ReportAvailabilityRequest request);
    Task<bool> ReportStepStatusAsync(ReportStepStatusRequest request);
    Task<List<StepResourceItem>> PullStepResourcesAsync(string taskId, string stepId, int take);
    Task<bool> PrefetchTasksAsync(int count);
    Task<object?> GetConfigAsync();
    Task<RegisterResponse?> RegisterAsync(object request);
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

    public async Task<TaskDto?> GetTaskContentAsync(string taskId)
    {
        var request = new PullTaskContentRequest(_options.AgentId, _options.AgentToken, taskId);
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentTaskContent, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TaskDto>>();
        return result?.Data;
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

    public async Task<bool> ReportStepStatusAsync(ReportStepStatusRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentStepReport, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return result?.Code == 200;
    }

    public async Task<List<StepResourceItem>> PullStepResourcesAsync(string taskId, string stepId, int take)
    {
        var request = new { AgentId = _options.AgentId, AgentToken = _options.AgentToken, TaskId = taskId, StepId = stepId, Take = take };
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentResourcesPull, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StepResourceItem>>>();
        return result?.Data ?? [];
    }

    public async Task<bool> PrefetchTasksAsync(int count)
    {
        var request = new { AgentId = _options.AgentId, AgentToken = _options.AgentToken, Count = count };
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentPrefetch, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return result?.Code == 200;
    }

    public async Task<object?> GetConfigAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/config/agent");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            return result?.Data;
        }
        catch
        {
            return null;
        }
    }

    public async Task<RegisterResponse?> RegisterAsync(object request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentRegister, request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterResponse>>();
            return result?.Data;
        }
        catch
        {
            return null;
        }
    }
}

public record RegisterRequest(
    string AgentId,
    string AgentName,
    string IpAddress,
    int Port,
    List<string>? Capabilities,
    string? OS = null,
    string? Version = null);
public record RegisterResponse(string AgentToken, int HeartbeatInterval, string ServerVersion);
public record HeartbeatRequest(string AgentId, string AgentToken, decimal? CpuUsage, decimal? MemoryUsage, int TaskCount, int Status, string? OS = null, string? Version = null);

public record HeartbeatResponse
{
    public bool Ack { get; set; }
    public List<TaskDto>? NewTasks { get; set; }
    public Dictionary<string, object>? ConfigUpdate { get; set; }
    public string? NewToken { get; set; }
    public OtaUpdateInfo? OtaUpdate { get; set; }
}

public record OtaUpdateInfo
{
    public string LatestVersion { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string? Checksum { get; set; }
    public string? ReleaseNotes { get; set; }
    public bool ForceUpdate { get; set; }
}

public record PullTasksRequest(string AgentId, string AgentToken, int MaxCount);
public record PullTaskContentRequest(string AgentId, string AgentToken, string TaskId);

public record PullTasksResponse
{
    public List<TaskDto> Tasks { get; set; } = [];
    public DateTime ServerTime { get; set; }
}

public record ReportTaskRequest(string AgentId, string AgentToken, string TaskId, int Status, int DataCount, int Duration);

public record ReportResponse
{
    public bool Ack { get; set; }
    public string? NextAction { get; set; }
}

public record UnregisterAgentRequest(string AgentId, string AgentToken, string? Reason);

/// <summary>
/// 任务数据传输对象
/// </summary>
public class TaskDto
{
    /// <summary>
    /// 任务唯一标识
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 任务名称
    /// </summary>
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型
    /// </summary>
    public string TaskType { get; set; } = Constants.Defaults.DefaultTaskType;

    /// <summary>
    /// 任务优先级
    /// </summary>
    public int Priority { get; set; } = Constants.Defaults.DefaultPriority;

    /// <summary>
    /// 任务状态码
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 请求配置 JSON 字符串
    /// </summary>
    public string RequestConfig { get; set; } = "{}";

    /// <summary>
    /// 调度配置 JSON 字符串
    /// </summary>
    public string? ScheduleConfig { get; set; }

    /// <summary>
    /// 重试策略 JSON 字符串
    /// </summary>
    public string? RetryPolicy { get; set; }

    /// <summary>
    /// 分配执行的 Agent 标识
    /// </summary>
    public string? AssignedAgentId { get; set; }

    /// <summary>
    /// 执行进度百分比
    /// </summary>
    public decimal Progress { get; set; }

    /// <summary>
    /// 总请求数
    /// </summary>
    public int TotalRequests { get; set; }

    /// <summary>
    /// 已完成请求数
    /// </summary>
    public int CompletedRequests { get; set; }

    /// <summary>
    /// 失败请求数
    /// </summary>
    public int FailedRequests { get; set; }

    /// <summary>
    /// 任务开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 任务结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 任务创建者
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 任务创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 关联的表达式标识
    /// </summary>
    public string? ExpressionId { get; set; }

    /// <summary>
    /// 表达式配置对象
    /// </summary>
    public ExpressionConfigDto? ExpressionConfig { get; set; }
}

/// <summary>
/// 表达式配置数据传输对象
/// </summary>
public class ExpressionConfigDto
{
    /// <summary>
    /// 表达式唯一标识
    /// </summary>
    public string ExpressionId { get; set; } = string.Empty;

    /// <summary>
    /// 表达式名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 选择器类型（如 XPath、CssSelector、JsonPath）
    /// </summary>
    public string SelectorType { get; set; } = Constants.Defaults.DefaultSelectorType;

    /// <summary>
    /// 容器表达式，用于定位数据项的容器元素
    /// </summary>
    public string ContainerExpression { get; set; } = string.Empty;

    /// <summary>
    /// 字段配置列表
    /// </summary>
    public List<ExpressionFieldConfigDto> Fields { get; set; } = [];
}

/// <summary>
/// 表达式字段配置数据传输对象
/// </summary>
public class ExpressionFieldConfigDto
{
    /// <summary>
    /// 字段名称
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// 字段选择器类型
    /// </summary>
    public string SelectorType { get; set; } = Constants.Defaults.DefaultSelectorType;

    /// <summary>
    /// 字段提取表达式
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// HTML 属性名称（用于提取元素属性值）
    /// </summary>
    public string? AttributeName { get; set; }

    /// <summary>
    /// 是否为必填字段
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// 字段默认值
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// 数据格式化器名称
    /// </summary>
    public string? Formatter { get; set; }

    /// <summary>
    /// 格式化器参数
    /// </summary>
    public string? FormatterArgs { get; set; }

    /// <summary>
    /// 字段排序序号
    /// </summary>
    public int Order { get; set; }
}

/// <summary>
/// 存储采集结果请求
/// </summary>
public class StoreResultsRequest
{
    /// <summary>
    /// Agent 唯一标识
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Agent 认证令牌
    /// </summary>
    public string AgentToken { get; set; } = string.Empty;

    /// <summary>
    /// 任务唯一标识
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的表达式标识
    /// </summary>
    public string? ExpressionId { get; set; }

    /// <summary>
    /// 采集结果项列表
    /// </summary>
    public List<ResultItemDto> Results { get; set; } = [];
}

/// <summary>
/// 单条采集结果数据传输对象
/// </summary>
public class ResultItemDto
{
    /// <summary>
    /// 结果记录唯一标识
    /// </summary>
    public string? ResultId { get; set; }

    /// <summary>
    /// 数据来源 URL
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// 采集到的字段数据字典
    /// </summary>
    public Dictionary<string, object?> Fields { get; set; } = new();

    /// <summary>
    /// 字段与表达式的映射关系
    /// </summary>
    public Dictionary<string, string>? FieldExpressionMap { get; set; }

    /// <summary>
    /// 数据采集时间
    /// </summary>
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

public record ReportStepStatusRequest(string AgentId, string AgentToken, string TaskId, string StepId, int State, int DataCount = 0);

public class StepResourceItem
{
    public string ResourceId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string StepId { get; set; } = string.Empty;
    public string? SourceStepId { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}";
    public string? SourceUrl { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
