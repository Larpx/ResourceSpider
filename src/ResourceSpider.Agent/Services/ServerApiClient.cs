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
    /// <summary>
    /// 向服务端注册 Agent 节点
    /// </summary>
    /// <param name="request">注册请求</param>
    /// <returns>注册响应，包含认证令牌和心跳间隔</returns>
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// 向服务端发送心跳，维持在线状态
    /// </summary>
    /// <param name="request">心跳请求</param>
    /// <returns>心跳响应，可能包含新任务或配置更新</returns>
    Task<HeartbeatResponse> HeartbeatAsync(HeartbeatRequest request);

    /// <summary>
    /// 从服务端拉取待执行的任务列表
    /// </summary>
    /// <param name="request">拉取任务请求</param>
    /// <returns>拉取任务响应，包含任务列表和服务器时间</returns>
    Task<PullTasksResponse> PullTasksAsync(PullTasksRequest request);

    /// <summary>
    /// 向服务端上报任务执行状态和结果
    /// </summary>
    /// <param name="request">上报任务请求</param>
    /// <returns>上报响应，包含确认标识</returns>
    Task<ReportResponse> ReportTaskAsync(ReportTaskRequest request);

    /// <summary>
    /// 向服务端注销 Agent 节点
    /// </summary>
    /// <param name="request">注销请求</param>
    Task UnregisterAsync(UnregisterAgentRequest request);

    /// <summary>
    /// 从服务端拉取指定表达式的配置
    /// </summary>
    /// <param name="expressionId">表达式唯一标识</param>
    /// <returns>表达式配置 DTO，不存在时返回 null</returns>
    Task<ExpressionConfigDto?> PullExpressionAsync(string expressionId);

    /// <summary>
    /// 从服务端拉取所有活跃表达式的配置列表
    /// </summary>
    /// <returns>活跃表达式配置列表</returns>
    Task<List<ExpressionConfigDto>> PullActiveExpressionsAsync();

    /// <summary>
    /// 向服务端存储采集结果数据
    /// </summary>
    /// <param name="request">存储结果请求</param>
    /// <returns>存储成功返回 true</returns>
    Task<bool> StoreResultsAsync(StoreResultsRequest request);

    /// <summary>
    /// 向服务端上报表达式的可用性状态
    /// </summary>
    /// <param name="request">可用性上报请求</param>
    /// <returns>上报成功返回 true</returns>
    Task<bool> ReportExpressionAvailabilityAsync(ReportAvailabilityRequest request);
}

/// <summary>
/// 服务端 API 客户端实现，通过 HTTP 调用服务端 RESTful API
/// </summary>
public class ServerApiClient : IServerApiClient
{
    /// <summary>
    /// HTTP 客户端实例
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// 在线模式配置选项
    /// </summary>
    private readonly OnlineModeOptions _options;

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<ServerApiClient> _logger;

    /// <summary>
    /// 初始化服务端 API 客户端实例
    /// </summary>
    /// <param name="httpClient">HTTP 客户端</param>
    /// <param name="options">在线模式配置选项</param>
    /// <param name="logger">日志记录器</param>
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

    /// <inheritdoc />
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentRegister, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterResponse>>();
        return result?.Data ?? throw new SpiderException("Agent 注册失败");
    }

    /// <inheritdoc />
    public async Task<HeartbeatResponse> HeartbeatAsync(HeartbeatRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentHeartbeat, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<HeartbeatResponse>>();
        return result?.Data ?? throw new SpiderException("心跳发送失败");
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<ReportResponse> ReportTaskAsync(ReportTaskRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentReportTask, request);
        response.EnsureSuccessStatusCode();
        return new ReportResponse { Ack = true };
    }

    /// <inheritdoc />
    public async Task UnregisterAsync(UnregisterAgentRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentUnregister, request);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc />
    public async Task<ExpressionConfigDto?> PullExpressionAsync(string expressionId)
    {
        var request = new { AgentId = _options.AgentId, AgentToken = _options.AgentToken, ExpressionId = expressionId };
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentPullExpression, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ExpressionConfigDto>>();
        return result?.Data;
    }

    /// <inheritdoc />
    public async Task<List<ExpressionConfigDto>> PullActiveExpressionsAsync()
    {
        var request = new { AgentId = _options.AgentId, AgentToken = _options.AgentToken };
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentActiveExpressions, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ExpressionConfigDto>>>();
        return result?.Data ?? [];
    }

    /// <inheritdoc />
    public async Task<bool> StoreResultsAsync(StoreResultsRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(Constants.ApiRoutes.AgentStoreResults, request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return result?.Code == 200;
    }

    /// <inheritdoc />
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
/// <param name="AgentId">Agent 唯一标识</param>
/// <param name="AgentName">Agent 显示名称</param>
/// <param name="IpAddress">Agent 节点 IP 地址</param>
/// <param name="Port">Agent 监听端口</param>
/// <param name="Capabilities">Agent 支持的能力列表</param>
public record RegisterRequest(string AgentId, string AgentName, string IpAddress, int Port, List<string>? Capabilities);

/// <summary>
/// Agent 注册响应
/// </summary>
/// <param name="AgentToken">服务端分配的认证令牌</param>
/// <param name="HeartbeatInterval">心跳间隔（秒）</param>
/// <param name="ServerVersion">服务端版本号</param>
public record RegisterResponse(string AgentToken, int HeartbeatInterval, string ServerVersion);

/// <summary>
/// 心跳请求
/// </summary>
/// <param name="AgentId">Agent 唯一标识</param>
/// <param name="AgentToken">Agent 认证令牌</param>
/// <param name="CpuUsage">CPU 使用率百分比</param>
/// <param name="MemoryUsage">内存使用率百分比</param>
/// <param name="TaskCount">当前执行中的任务数</param>
/// <param name="Status">Agent 状态码</param>
public record HeartbeatRequest(string AgentId, string AgentToken, decimal? CpuUsage, decimal? MemoryUsage, int TaskCount, int Status);

/// <summary>
/// 心跳响应
/// </summary>
public record HeartbeatResponse
{
    /// <summary>
    /// 服务端确认标识
    /// </summary>
    public bool Ack { get; set; }

    /// <summary>
    /// 服务端下发的新任务列表
    /// </summary>
    public List<TaskDto>? NewTasks { get; set; }

    /// <summary>
    /// 服务端下发的配置更新
    /// </summary>
    public Dictionary<string, object>? ConfigUpdate { get; set; }
}

/// <summary>
/// 拉取任务请求
/// </summary>
/// <param name="AgentId">Agent 唯一标识</param>
/// <param name="AgentToken">Agent 认证令牌</param>
/// <param name="MaxCount">最大拉取任务数量</param>
public record PullTasksRequest(string AgentId, string AgentToken, int MaxCount);

/// <summary>
/// 拉取任务响应
/// </summary>
public record PullTasksResponse
{
    /// <summary>
    /// 拉取到的任务列表
    /// </summary>
    public List<TaskDto> Tasks { get; set; } = [];

    /// <summary>
    /// 服务器当前时间
    /// </summary>
    public DateTime ServerTime { get; set; }
}

/// <summary>
/// 上报任务结果请求
/// </summary>
/// <param name="AgentId">Agent 唯一标识</param>
/// <param name="AgentToken">Agent 认证令牌</param>
/// <param name="TaskId">任务唯一标识</param>
/// <param name="Status">任务状态码（2-成功，3-失败）</param>
/// <param name="DataCount">采集数据条数</param>
/// <param name="Duration">执行耗时（毫秒）</param>
public record ReportTaskRequest(string AgentId, string AgentToken, string TaskId, int Status, int DataCount, int Duration);

/// <summary>
/// 上报任务结果响应
/// </summary>
public record ReportResponse
{
    /// <summary>
    /// 服务端确认标识
    /// </summary>
    public bool Ack { get; set; }

    /// <summary>
    /// 服务端建议的下一步操作
    /// </summary>
    public string? NextAction { get; set; }
}

/// <summary>
/// Agent 注销请求
/// </summary>
/// <param name="AgentId">Agent 唯一标识</param>
/// <param name="AgentToken">Agent 认证令牌</param>
/// <param name="Reason">注销原因</param>
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
    /// <summary>
    /// Agent 唯一标识
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Agent 认证令牌
    /// </summary>
    public string AgentToken { get; set; } = string.Empty;

    /// <summary>
    /// 表达式唯一标识
    /// </summary>
    public string ExpressionId { get; set; } = string.Empty;

    /// <summary>
    /// 表达式是否可用
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// 不可用时的失败原因
    /// </summary>
    public string? FailureReason { get; set; }
}
