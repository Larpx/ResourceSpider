using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using ResourceSpider.Server.DTOs;

namespace ResourceSpider.Server.Components.Services;

/// <summary>
/// 后台管理页面 API 客户端，仅调用管理 API 路由
/// </summary>
public class AdminApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AdminSessionState _session;

    public AdminApiClient(
        IHttpClientFactory httpClientFactory,
        NavigationManager navigationManager,
        AdminSessionState session)
    {
        _session = session;
        _httpClient = httpClientFactory.CreateClient(nameof(AdminApiClient));
        _httpClient.BaseAddress = new Uri(navigationManager.BaseUri);
    }

    public async Task<(bool Success, string Message)> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/admin/auth/login", new LoginRequest(username, password));

        ApiResponse<AuthResponse>? payload = null;
        try
        {
            payload = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        }
        catch
        {
        }

        if (response.IsSuccessStatusCode && payload?.Code == 200 && payload.Data != null)
        {
            _session.SetSession(payload.Data);
            return (true, payload.Message);
        }

        var errorMessage = payload?.Message;
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            try
            {
                var errorPayload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                errorMessage = errorPayload?.Message;
            }
            catch
            {
            }
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            errorMessage = response.ReasonPhrase ?? "登录失败";
        }

        return (false, errorMessage);
    }

    public void Logout()
    {
        _session.Clear();
    }

    #region 系统监控

    /// <summary>
    /// 获取当前管理员信息
    /// </summary>
    public async Task<UserInfoDto?> GetCurrentAdminInfoAsync()
    {
        var payload = await GetAuthorizedAsync<UserInfoDto>("api/admin/auth/me");
        return payload?.Data;
    }

    /// <summary>
    /// 更新管理员资料
    /// </summary>
    /// <param name="request">更新资料请求</param>
    public async Task<(bool Success, string Message)> UpdateAdminProfileAsync(UpdateAdminProfileRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Put, "api/admin/auth/profile", request);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 修改管理员密码
    /// </summary>
    /// <param name="request">修改密码请求</param>
    public async Task<(bool Success, string Message)> ChangeAdminPasswordAsync(ChangeAdminPasswordRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Post, "api/admin/auth/change-password", request);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 获取系统统计概览
    /// </summary>
    public async Task<SystemStatisticsDto?> GetSystemStatisticsAsync()
    {
        var payload = await GetAuthorizedAsync<SystemStatisticsDto>("api/admin/statistics/system");
        return payload?.Data;
    }

    /// <summary>
    /// 获取系统健康状态
    /// </summary>
    public async Task<SystemHealthDto?> GetSystemHealthAsync()
    {
        var payload = await GetAuthorizedAsync<SystemHealthDto>("api/admin/system/health");
        return payload?.Data;
    }

    /// <summary>
    /// 获取系统运行时状态
    /// </summary>
    public async Task<SystemRuntimeStatusDto?> GetSystemRuntimeStatusAsync()
    {
        var payload = await GetAuthorizedAsync<SystemRuntimeStatusDto>("api/admin/system/runtime");
        return payload?.Data;
    }

    /// <summary>
    /// 获取系统日志（分页）
    /// </summary>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="level">日志级别筛选</param>
    /// <param name="category">分类筛选</param>
    /// <param name="startDate">起始时间</param>
    /// <param name="endDate">结束时间</param>
    public async Task<SystemLogListResponse?> GetSystemLogsAsync(int pageIndex = 1, int pageSize = 10, string? level = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = $"api/admin/system/logs?pageIndex={pageIndex}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(level)) query += $"&level={Uri.EscapeDataString(level)}";
        if (!string.IsNullOrWhiteSpace(category)) query += $"&category={Uri.EscapeDataString(category)}";
        if (startDate.HasValue) query += $"&startDate={Uri.EscapeDataString(startDate.Value.ToString("O"))}";
        if (endDate.HasValue) query += $"&endDate={Uri.EscapeDataString(endDate.Value.ToString("O"))}";
        var payload = await GetAuthorizedAsync<SystemLogListResponse>(query);
        return payload?.Data;
    }

    /// <summary>
    /// 获取 Redis 功能开关状态
    /// </summary>
    public async Task<RedisFeatureStatusDto?> GetRedisFeatureStatusAsync()
    {
        var payload = await GetAuthorizedAsync<RedisFeatureStatusDto>("api/admin/system/redis");
        return payload?.Data;
    }

    /// <summary>
    /// 更新 Redis 功能开关状态
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public async Task<RedisFeatureStatusDto?> UpdateRedisFeatureStatusAsync(bool enabled)
    {
        var result = await SendAuthorizedAsync<RedisFeatureStatusDto>(HttpMethod.Put, "api/admin/system/redis", new UpdateRedisFeatureRequest(enabled));
        return result.Payload?.Data;
    }

    #endregion

    #region Agent 管理

    /// <summary>
    /// 获取所有代理列表
    /// </summary>
    public async Task<List<AgentDto>> GetAgentsAsync()
    {
        var payload = await GetAuthorizedAsync<List<AgentDto>>("api/admin/agents");
        return payload?.Data ?? new List<AgentDto>();
    }

    /// <summary>
    /// 根据代理 ID 获取代理详情
    /// </summary>
    /// <param name="agentId">代理 ID</param>
    public async Task<AgentDto?> GetAgentByIdAsync(string agentId)
    {
        var payload = await GetAuthorizedAsync<AgentDto>($"api/admin/agents/{agentId}");
        return payload?.Data;
    }

    /// <summary>
    /// 更新代理配置
    /// </summary>
    /// <param name="agentId">代理 ID</param>
    /// <param name="request">更新代理请求</param>
    public async Task<(bool Success, string Message)> UpdateAgentAsync(string agentId, UpdateAgentRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Put, $"api/admin/agents/{agentId}", request);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 删除/注销代理
    /// </summary>
    /// <param name="agentId">代理 ID</param>
    public async Task<(bool Success, string Message)> DeleteAgentAsync(string agentId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Delete, $"api/admin/agents/{agentId}");
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 发送重启命令给代理
    /// </summary>
    /// <param name="agentId">代理 ID</param>
    public async Task<(bool Success, string Message)> RestartAgentAsync(string agentId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Post, $"api/admin/agents/{agentId}/restart");
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 发送更新命令给代理
    /// </summary>
    /// <param name="agentId">代理 ID</param>
    public async Task<(bool Success, string Message)> UpdateAgentVersionAsync(string agentId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Post, $"api/admin/agents/{agentId}/update");
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 发送紧急停止命令给代理
    /// </summary>
    /// <param name="agentId">代理 ID</param>
    public async Task<(bool Success, string Message)> StopAllAgentTasksAsync(string agentId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Post, $"api/admin/agents/{agentId}/stop-all");
        return (result.Success, result.Message);
    }

    #endregion

    #region Agent 分组管理

    /// <summary>
    /// 获取所有代理分组列表
    /// </summary>
    public async Task<List<AgentGroupDto>> GetAgentGroupsAsync()
    {
        var payload = await GetAuthorizedAsync<List<AgentGroupDto>>("api/admin/agent-groups");
        return payload?.Data ?? new List<AgentGroupDto>();
    }

    /// <summary>
    /// 根据分组 ID 获取分组详情
    /// </summary>
    /// <param name="groupId">分组 ID</param>
    public async Task<AgentGroupDto?> GetAgentGroupByIdAsync(string groupId)
    {
        var payload = await GetAuthorizedAsync<AgentGroupDto>($"api/admin/agent-groups/{groupId}");
        return payload?.Data;
    }

    /// <summary>
    /// 创建代理分组
    /// </summary>
    /// <param name="request">创建分组请求</param>
    public async Task<(bool Success, string Message)> CreateAgentGroupAsync(CreateAgentGroupRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<AgentGroupDto>(HttpMethod.Post, "api/admin/agent-groups", request);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 更新代理分组
    /// </summary>
    /// <param name="groupId">分组 ID</param>
    /// <param name="request">更新分组请求</param>
    public async Task<(bool Success, string Message)> UpdateAgentGroupAsync(string groupId, UpdateAgentGroupRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Put, $"api/admin/agent-groups/{groupId}", request);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 删除代理分组
    /// </summary>
    /// <param name="groupId">分组 ID</param>
    public async Task<(bool Success, string Message)> DeleteAgentGroupAsync(string groupId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Delete, $"api/admin/agent-groups/{groupId}");
        return (result.Success, result.Message);
    }

    #endregion

    #region 任务管理

    /// <summary>
    /// 获取任务列表（分页+筛选）
    /// </summary>
    public async Task<TaskListResponse?> GetTasksAsync(int pageIndex = 1, int pageSize = 20, int? status = null, string? keyword = null)
    {
        var query = $"api/admin/tasks?pageIndex={pageIndex}&pageSize={pageSize}";
        if (status.HasValue)
        {
            query += $"&status={status.Value}";
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query += $"&keyword={Uri.EscapeDataString(keyword)}";
        }

        var payload = await GetAuthorizedAsync<TaskListResponse>(query);
        return payload?.Data;
    }

    /// <summary>
    /// 根据任务 ID 获取任务详情
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public async Task<TaskDto?> GetTaskByIdAsync(string taskId)
    {
        var payload = await GetAuthorizedAsync<TaskDto>($"api/admin/tasks/{taskId}");
        return payload?.Data;
    }

    /// <summary>
    /// 创建任务
    /// </summary>
    /// <param name="request">创建任务请求</param>
    public async Task<(bool Success, string Message)> CreateTaskAsync(CreateTaskRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<TaskDto>(HttpMethod.Post, "api/admin/tasks", request);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 更新任务
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="request">更新任务请求</param>
    public async Task<(bool Success, string Message)> UpdateTaskAsync(string taskId, UpdateTaskRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Put, $"api/admin/tasks/{taskId}", request);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 删除任务
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public async Task<(bool Success, string Message)> DeleteTaskAsync(string taskId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Delete, $"api/admin/tasks/{taskId}");
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 触发任务执行
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public async Task<(bool Success, string Message)> ExecuteTaskAsync(string taskId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Post, $"api/admin/tasks/{taskId}/execute");
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 暂停任务
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public async Task<(bool Success, string Message)> PauseTaskAsync(string taskId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Post, $"api/admin/tasks/{taskId}/pause");
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 恢复任务
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public async Task<(bool Success, string Message)> ResumeTaskAsync(string taskId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Post, $"api/admin/tasks/{taskId}/resume");
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 终止任务
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public async Task<(bool Success, string Message)> StopTaskAsync(string taskId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Post, $"api/admin/tasks/{taskId}/stop");
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 获取任务执行历史
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public async Task<List<TaskExecutionDto>?> GetTaskExecutionsAsync(string taskId)
    {
        var payload = await GetAuthorizedAsync<List<TaskExecutionDto>>($"api/admin/tasks/{taskId}/executions");
        return payload?.Data;
    }

    /// <summary>
    /// 获取任务配置快照
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public async Task<TaskConfigurationSnapshot?> GetTaskConfigSnapshotAsync(string taskId)
    {
        var payload = await GetAuthorizedAsync<TaskConfigurationSnapshot>($"api/admin/tasks/{taskId}/config/snapshot");
        return payload?.Data;
    }

    /// <summary>
    /// 获取任务配置版本历史
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public async Task<List<ConfigVersionDto>?> GetTaskConfigVersionsAsync(string taskId)
    {
        var payload = await GetAuthorizedAsync<List<ConfigVersionDto>>($"api/admin/tasks/{taskId}/config/versions");
        return payload?.Data;
    }

    /// <summary>
    /// 回滚任务配置到指定版本
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="version">目标版本号</param>
    public async Task<(bool Success, string Message)> RollbackTaskConfigAsync(string taskId, int version)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Post, $"api/admin/tasks/{taskId}/config/rollback/{version}");
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 获取任务步骤状态
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public async Task<List<TaskStepDto>?> GetTaskStepsStatusAsync(string taskId)
    {
        var payload = await GetAuthorizedAsync<List<TaskStepDto>>($"api/admin/tasks/{taskId}/steps/status");
        return payload?.Data;
    }

    /// <summary>
    /// 获取任务步骤列表
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public async Task<List<TaskStepDto>> GetTaskStepsAsync(string taskId)
    {
        var payload = await GetAuthorizedAsync<List<TaskStepDto>>($"api/admin/tasks/{taskId}/steps");
        return payload?.Data ?? new List<TaskStepDto>();
    }

    /// <summary>
    /// 创建任务步骤
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="request">创建步骤请求</param>
    public async Task<(bool Success, string Message)> CreateTaskStepAsync(string taskId, CreateTaskStepRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<TaskStepDto>(HttpMethod.Post, $"api/admin/tasks/{taskId}/steps", request);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 更新任务步骤
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="stepId">步骤 ID</param>
    /// <param name="request">更新步骤请求</param>
    public async Task<(bool Success, string Message)> UpdateTaskStepAsync(string taskId, string stepId, UpdateTaskStepRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Put, $"api/admin/tasks/{taskId}/steps/{stepId}", request);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 删除任务步骤
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="stepId">步骤 ID</param>
    public async Task<(bool Success, string Message)> DeleteTaskStepAsync(string taskId, string stepId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Delete, $"api/admin/tasks/{taskId}/steps/{stepId}");
        return (result.Success, result.Message);
    }

    #endregion

    #region 采集结果管理

    /// <summary>
    /// 获取采集结果列表（多条件筛选+分页）
    /// </summary>
    public async Task<CollectionResultListResponse?> GetResultsAsync(
        string? taskId = null,
        string? stepId = null,
        string? agentId = null,
        string? keyword = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        bool? isDuplicate = null,
        int pageIndex = 1,
        int pageSize = 20)
    {
        var query = $"api/admin/results?pageIndex={pageIndex}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(taskId)) query += $"&taskId={Uri.EscapeDataString(taskId)}";
        if (!string.IsNullOrWhiteSpace(stepId)) query += $"&stepId={Uri.EscapeDataString(stepId)}";
        if (!string.IsNullOrWhiteSpace(agentId)) query += $"&agentId={Uri.EscapeDataString(agentId)}";
        if (!string.IsNullOrWhiteSpace(keyword)) query += $"&keyword={Uri.EscapeDataString(keyword)}";
        if (startTime.HasValue) query += $"&startTime={Uri.EscapeDataString(startTime.Value.ToString("O"))}";
        if (endTime.HasValue) query += $"&endTime={Uri.EscapeDataString(endTime.Value.ToString("O"))}";
        if (isDuplicate.HasValue) query += $"&isDuplicate={isDuplicate.Value.ToString().ToLowerInvariant()}";

        var payload = await GetAuthorizedAsync<CollectionResultListResponse>(query);
        return payload?.Data;
    }

    /// <summary>
    /// 根据结果 ID 获取单条采集结果详情
    /// </summary>
    /// <param name="resultId">结果 ID</param>
    public async Task<CollectionResultDto?> GetResultByIdAsync(string resultId)
    {
        var payload = await GetAuthorizedAsync<CollectionResultDto>($"api/admin/results/{resultId}");
        return payload?.Data;
    }

    /// <summary>
    /// 批量删除采集结果
    /// </summary>
    /// <param name="resultIds">待删除的结果 ID 列表</param>
    public async Task<(bool Success, string Message)> BatchDeleteResultsAsync(List<string> resultIds)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Delete, "api/admin/results", resultIds);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 导出采集结果
    /// </summary>
    /// <param name="request">导出请求</param>
    public async Task<ExportResultDto?> ExportResultsAsync(ExportRequest request)
    {
        var result = await SendAuthorizedAsync<ExportResultDto>(HttpMethod.Post, "api/admin/results/export", request);
        return result.Payload?.Data;
    }

    /// <summary>
    /// 导入采集结果
    /// </summary>
    /// <param name="request">导入请求</param>
    public async Task<(bool Success, string Message)> ImportResultsAsync(ImportCollectionResultsRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<ImportCollectionResultsResponse>(HttpMethod.Post, "api/admin/results/import", request);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 获取采集结果统计信息
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public async Task<object?> GetResultStatsAsync(string? taskId = null)
    {
        var query = "api/admin/results/stats";
        if (!string.IsNullOrWhiteSpace(taskId)) query += $"?taskId={Uri.EscapeDataString(taskId)}";
        var payload = await GetAuthorizedAsync<object>(query);
        return payload?.Data;
    }

    /// <summary>
    /// 按任务 ID 查询采集结果
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">每页数量</param>
    public async Task<CollectionResultListResponse?> GetResultsByTaskIdAsync(string taskId, int pageIndex = 1, int pageSize = 20)
    {
        var payload = await GetAuthorizedAsync<CollectionResultListResponse>($"api/admin/collection-results/task/{taskId}?pageIndex={pageIndex}&pageSize={pageSize}");
        return payload?.Data;
    }

    /// <summary>
    /// 按表达式 ID 查询采集结果
    /// </summary>
    /// <param name="expressionId">表达式 ID</param>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">每页数量</param>
    public async Task<CollectionResultListResponse?> GetResultsByExpressionIdAsync(string expressionId, int pageIndex = 1, int pageSize = 20)
    {
        var payload = await GetAuthorizedAsync<CollectionResultListResponse>($"api/admin/collection-results/expression/{expressionId}?pageIndex={pageIndex}&pageSize={pageSize}");
        return payload?.Data;
    }

    #endregion

    #region 表达式管理

    /// <summary>
    /// 获取表达式列表（分页+筛选）
    /// </summary>
    public async Task<ExpressionListResponse?> GetExpressionsAsync(int pageIndex = 1, int pageSize = 20, int? status = null, string? keyword = null)
    {
        var query = $"api/admin/expressions?pageIndex={pageIndex}&pageSize={pageSize}";
        if (status.HasValue)
        {
            query += $"&status={status.Value}";
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query += $"&keyword={Uri.EscapeDataString(keyword)}";
        }

        var payload = await GetAuthorizedAsync<ExpressionListResponse>(query);
        return payload?.Data;
    }

    /// <summary>
    /// 根据表达式 ID 获取表达式详情
    /// </summary>
    /// <param name="expressionId">表达式 ID</param>
    public async Task<ExpressionDto?> GetExpressionByIdAsync(string expressionId)
    {
        var payload = await GetAuthorizedAsync<ExpressionDto>($"api/admin/expressions/{expressionId}");
        return payload?.Data;
    }

    /// <summary>
    /// 创建表达式
    /// </summary>
    /// <param name="request">创建表达式请求</param>
    public async Task<(bool Success, string Message)> CreateExpressionAsync(CreateExpressionRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<ExpressionDto>(HttpMethod.Post, "api/admin/expressions", request);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 更新表达式
    /// </summary>
    /// <param name="expressionId">表达式 ID</param>
    /// <param name="request">更新表达式请求</param>
    public async Task<(bool Success, string Message)> UpdateExpressionAsync(string expressionId, UpdateExpressionRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Put, $"api/admin/expressions/{expressionId}", request);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 删除表达式
    /// </summary>
    /// <param name="expressionId">表达式 ID</param>
    public async Task<(bool Success, string Message)> DeleteExpressionAsync(string expressionId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Delete, $"api/admin/expressions/{expressionId}");
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 获取表达式配置（供 Agent 使用）
    /// </summary>
    /// <param name="expressionId">表达式 ID</param>
    public async Task<ExpressionConfigDto?> GetExpressionConfigAsync(string expressionId)
    {
        var payload = await GetAuthorizedAsync<ExpressionConfigDto>($"api/admin/expressions/{expressionId}/config");
        return payload?.Data;
    }

    /// <summary>
    /// 使过期表达式失效
    /// </summary>
    public async Task<(bool Success, string Message)> InvalidateExpressionsAsync()
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Post, "api/admin/expressions/invalidate");
        return (result.Success, result.Message);
    }

    #endregion

    #region 代理池管理

    /// <summary>
    /// 获取代理列表（分页+筛选）
    /// </summary>
    public async Task<ProxyListResponse?> GetProxiesAsync(int pageIndex = 1, int pageSize = 20, int? status = null, string? keyword = null)
    {
        var query = $"api/admin/proxies?pageIndex={pageIndex}&pageSize={pageSize}";
        if (status.HasValue)
        {
            query += $"&status={status.Value}";
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query += $"&keyword={Uri.EscapeDataString(keyword)}";
        }

        var payload = await GetAuthorizedAsync<ProxyListResponse>(query);
        return payload?.Data;
    }

    /// <summary>
    /// 添加代理
    /// </summary>
    /// <param name="request">创建代理请求</param>
    public async Task<(bool Success, string Message)> CreateProxyAsync(CreateProxyRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<ProxyDto>(HttpMethod.Post, "api/admin/proxies", request);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 更新代理
    /// </summary>
    /// <param name="proxyId">代理 ID</param>
    /// <param name="request">更新代理请求</param>
    public async Task<(bool Success, string Message)> UpdateProxyAsync(string proxyId, UpdateProxyRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Put, $"api/admin/proxies/{proxyId}", request);
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 删除代理
    /// </summary>
    /// <param name="proxyId">代理 ID</param>
    public async Task<(bool Success, string Message)> DeleteProxyAsync(string proxyId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Delete, $"api/admin/proxies/{proxyId}");
        return (result.Success, result.Message);
    }

    /// <summary>
    /// 测试代理可用性
    /// </summary>
    /// <param name="request">代理测试请求</param>
    public async Task<ProxyTestResponse?> TestProxyAsync(ProxyTestRequest request)
    {
        var result = await SendAuthorizedAsync<ProxyTestResponse>(HttpMethod.Post, "api/admin/proxies/test", request);
        return result.Payload?.Data;
    }

    #endregion

    #region 统计分析

    /// <summary>
    /// 获取代理统计数据
    /// </summary>
    public async Task<List<AgentStatisticsDto>?> GetAgentStatisticsAsync()
    {
        var payload = await GetAuthorizedAsync<List<AgentStatisticsDto>>("api/admin/statistics/agent");
        return payload?.Data;
    }

    /// <summary>
    /// 获取任务统计数据
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public async Task<TaskStatisticsDto?> GetTaskStatisticsAsync(string taskId)
    {
        var payload = await GetAuthorizedAsync<TaskStatisticsDto>($"api/admin/statistics/task/{taskId}");
        return payload?.Data;
    }

    /// <summary>
    /// 获取趋势数据
    /// </summary>
    /// <param name="startDate">起始日期</param>
    /// <param name="endDate">结束日期</param>
    public async Task<List<TrendDataPoint>?> GetTrendDataAsync(DateTime startDate, DateTime endDate)
    {
        var query = $"api/admin/statistics/trend?startDate={Uri.EscapeDataString(startDate.ToString("O"))}&endDate={Uri.EscapeDataString(endDate.ToString("O"))}";
        var payload = await GetAuthorizedAsync<List<TrendDataPoint>>(query);
        return payload?.Data;
    }

    #endregion

    #region 配置工具

    /// <summary>
    /// 测试提取表达式
    /// </summary>
    /// <param name="request">测试提取请求</param>
    public async Task<TestExtractionResponse?> TestExtractionAsync(TestExtractionRequest request)
    {
        var result = await SendAuthorizedAsync<TestExtractionResponse>(HttpMethod.Post, "api/admin/config/test-extraction", request);
        return result.Payload?.Data;
    }

    /// <summary>
    /// 获取配置模板列表
    /// </summary>
    public async Task<List<ConfigTemplateDto>?> GetConfigTemplatesAsync()
    {
        var payload = await GetAuthorizedAsync<List<ConfigTemplateDto>>("api/admin/config/templates");
        return payload?.Data;
    }

    #endregion

    #region SignalR 运行监控

    /// <summary>
    /// 创建并启动运行监控 SignalR 连接，同时接收系统快照与日志推送。
    /// </summary>
    /// <param name="onRuntimeSnapshot">收到系统快照时的回调</param>
    /// <param name="onRuntimeOutputLog">收到日志时的回调</param>
    /// <returns>已启动的 Hub 连接，调用方负责释放</returns>
    public async Task<HubConnection?> ConnectRuntimeStreamAsync(
        Func<SystemRuntimeStatusDto, Task> onRuntimeSnapshot,
        Func<RuntimeOutputLogDto, Task> onRuntimeOutputLog)
    {
        if (!_session.IsAuthenticated || string.IsNullOrWhiteSpace(_session.Token))
        {
            return null;
        }

        var hubUrl = new Uri(_httpClient.BaseAddress!, "hubs/spider").ToString();

        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_session.Token)!;
            })
            .WithAutomaticReconnect()
            .Build();

        connection.On<SystemRuntimeStatusDto>("RuntimeSnapshot", async snapshot =>
        {
            await onRuntimeSnapshot(snapshot);
        });

        connection.On<RuntimeOutputLogDto>("RuntimeOutputLog", async log =>
        {
            await onRuntimeOutputLog(log);
        });

        await connection.StartAsync();
        await connection.InvokeAsync("JoinAdminRuntimeGroup");

        return connection;
    }

    /// <summary>
    /// 设置当前 SignalR 连接的快照推送策略（秒）。
    /// </summary>
    /// <param name="connection">Hub 连接</param>
    /// <param name="intervalSeconds">目标间隔秒数</param>
    /// <returns>服务端最终生效间隔</returns>
    public async Task<int> SetRuntimeSnapshotIntervalAsync(HubConnection connection, int intervalSeconds)
    {
        return await connection.InvokeAsync<int>("SetAdminRuntimeSnapshotInterval", intervalSeconds);
    }

    #endregion

    private async Task<ApiResponse<T>?> GetAuthorizedAsync<T>(string url)
    {
        var result = await SendAuthorizedAsync<T>(HttpMethod.Get, url);
        return result.Payload;
    }

    private async Task<(bool Success, string Message, ApiResponse<T>? Payload)> SendAuthorizedAsync<T>(HttpMethod method, string url, object? body = null)
    {
        if (!_session.IsAuthenticated || string.IsNullOrWhiteSpace(_session.Token))
        {
            return (false, "未登录", null);
        }

        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.Token);
        if (body != null)
        {
            request.Content = JsonContent.Create(body);
        }

        using var response = await _httpClient.SendAsync(request);

        ApiResponse<T>? payload = null;
        try
        {
            payload = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
        }
        catch
        {
        }

        if (response.IsSuccessStatusCode && payload?.Code == 200)
        {
            return (true, payload.Message, payload);
        }

        var message = payload?.Message;
        if (string.IsNullOrWhiteSpace(message))
        {
            try
            {
                var errorPayload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                message = errorPayload?.Message;
            }
            catch
            {
            }
        }

        return (false, string.IsNullOrWhiteSpace(message) ? (response.ReasonPhrase ?? "请求失败") : message, payload);
    }

    private async Task<(bool Success, string Message)> SendAuthorizedForMessageAsync<T>(HttpMethod method, string url, object? body = null)
    {
        var result = await SendAuthorizedAsync<T>(method, url, body);
        return (result.Success, result.Message);
    }
}
