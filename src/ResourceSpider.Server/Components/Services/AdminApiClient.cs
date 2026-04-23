using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
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

    public async Task<SystemStatisticsDto?> GetSystemStatisticsAsync()
    {
        var payload = await GetAuthorizedAsync<SystemStatisticsDto>("api/admin/statistics/system");
        return payload?.Data;
    }

    public async Task<SystemHealthDto?> GetSystemHealthAsync()
    {
        var payload = await GetAuthorizedAsync<SystemHealthDto>("api/admin/system/health");
        return payload?.Data;
    }

    public async Task<SystemRuntimeStatusDto?> GetSystemRuntimeStatusAsync()
    {
        var payload = await GetAuthorizedAsync<SystemRuntimeStatusDto>("api/admin/system/runtime");
        return payload?.Data;
    }

    public async Task<List<AgentDto>> GetAgentsAsync()
    {
        var payload = await GetAuthorizedAsync<List<AgentDto>>("api/admin/agents");
        return payload?.Data ?? new List<AgentDto>();
    }

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

    public async Task<(bool Success, string Message)> CreateTaskAsync(CreateTaskRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<TaskDto>(HttpMethod.Post, "api/admin/tasks", request);
        return (result.Success, result.Message);
    }

    public async Task<(bool Success, string Message)> UpdateTaskAsync(string taskId, UpdateTaskRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Put, $"api/admin/tasks/{taskId}", request);
        return (result.Success, result.Message);
    }

    public async Task<(bool Success, string Message)> DeleteTaskAsync(string taskId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Delete, $"api/admin/tasks/{taskId}");
        return (result.Success, result.Message);
    }

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

    public async Task<(bool Success, string Message)> CreateExpressionAsync(CreateExpressionRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<ExpressionDto>(HttpMethod.Post, "api/admin/expressions", request);
        return (result.Success, result.Message);
    }

    public async Task<(bool Success, string Message)> UpdateExpressionAsync(string expressionId, UpdateExpressionRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Put, $"api/admin/expressions/{expressionId}", request);
        return (result.Success, result.Message);
    }

    public async Task<(bool Success, string Message)> DeleteExpressionAsync(string expressionId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Delete, $"api/admin/expressions/{expressionId}");
        return (result.Success, result.Message);
    }

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

    public async Task<(bool Success, string Message)> CreateProxyAsync(CreateProxyRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<ProxyDto>(HttpMethod.Post, "api/admin/proxies", request);
        return (result.Success, result.Message);
    }

    public async Task<(bool Success, string Message)> UpdateProxyAsync(string proxyId, UpdateProxyRequest request)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Put, $"api/admin/proxies/{proxyId}", request);
        return (result.Success, result.Message);
    }

    public async Task<(bool Success, string Message)> DeleteProxyAsync(string proxyId)
    {
        var result = await SendAuthorizedForMessageAsync<object>(HttpMethod.Delete, $"api/admin/proxies/{proxyId}");
        return (result.Success, result.Message);
    }

    public async Task<SystemLogListResponse?> GetSystemLogsAsync(int pageIndex = 1, int pageSize = 10)
    {
        var payload = await GetAuthorizedAsync<SystemLogListResponse>($"api/admin/system/logs?pageIndex={pageIndex}&pageSize={pageSize}");
        return payload?.Data;
    }

    public async Task<RedisFeatureStatusDto?> GetRedisFeatureStatusAsync()
    {
        var payload = await GetAuthorizedAsync<RedisFeatureStatusDto>("api/admin/system/redis");
        return payload?.Data;
    }

    public async Task<RedisFeatureStatusDto?> UpdateRedisFeatureStatusAsync(bool enabled)
    {
        var result = await SendAuthorizedAsync<RedisFeatureStatusDto>(HttpMethod.Put, "api/admin/system/redis", new UpdateRedisFeatureRequest(enabled));
        return result.Payload?.Data;
    }

    public async Task<PostgreSqlResultStorageStatusDto?> GetPostgreSqlResultStorageStatusAsync()
    {
        var payload = await GetAuthorizedAsync<PostgreSqlResultStorageStatusDto>("api/admin/system/postgresql-results");
        return payload?.Data;
    }

    public async Task<PostgreSqlResultStorageStatusDto?> UpdatePostgreSqlResultStorageStatusAsync(bool enabled)
    {
        var result = await SendAuthorizedAsync<PostgreSqlResultStorageStatusDto>(
            HttpMethod.Put,
            "api/admin/system/postgresql-results",
            new UpdatePostgreSqlResultStorageRequest(enabled));

        return result.Payload?.Data;
    }

    public async Task<CollectionResultListResponse?> GetTaskResultsAsync(string taskId, int pageIndex = 1, int pageSize = 20, string? keyword = null)
    {
        var query = $"api/admin/results?taskId={Uri.EscapeDataString(taskId)}&pageIndex={pageIndex}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query += $"&keyword={Uri.EscapeDataString(keyword)}";
        }

        var payload = await GetAuthorizedAsync<CollectionResultListResponse>(query);
        return payload?.Data;
    }

    public async Task<CollectionResultDto?> GetTaskResultByIdAsync(string resultId)
    {
        var payload = await GetAuthorizedAsync<CollectionResultDto>($"api/admin/results/{Uri.EscapeDataString(resultId)}");
        return payload?.Data;
    }

    public async Task<ExportResultDto?> ExportTaskResultsAsync(
        string taskId,
        ExportFormat format,
        DateTime? startTime = null,
        DateTime? endTime = null,
        List<string>? fields = null)
    {
        var request = new ExportRequest(
            TaskId: taskId,
            Format: format,
            StepId: null,
            StartTime: startTime,
            EndTime: endTime,
            Fields: fields);

        var result = await SendAuthorizedAsync<ExportResultDto>(HttpMethod.Post, "api/admin/results/export", request);
        return result.Payload?.Data;
    }

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
