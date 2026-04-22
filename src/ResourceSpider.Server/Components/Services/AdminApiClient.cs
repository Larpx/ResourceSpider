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

    public async Task<TaskListResponse?> GetTasksAsync(int pageIndex = 1, int pageSize = 20)
    {
        var payload = await GetAuthorizedAsync<TaskListResponse>($"api/admin/tasks?pageIndex={pageIndex}&pageSize={pageSize}");
        return payload?.Data;
    }

    public async Task<ExpressionListResponse?> GetExpressionsAsync(int pageIndex = 1, int pageSize = 20)
    {
        var payload = await GetAuthorizedAsync<ExpressionListResponse>($"api/admin/expressions?pageIndex={pageIndex}&pageSize={pageSize}");
        return payload?.Data;
    }

    public async Task<List<ProxyDto>> GetProxiesAsync(int pageIndex = 1, int pageSize = 20)
    {
        var payload = await GetAuthorizedAsync<List<ProxyDto>>($"api/admin/proxies?pageIndex={pageIndex}&pageSize={pageSize}");
        return payload?.Data ?? new List<ProxyDto>();
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
        if (!_session.IsAuthenticated || string.IsNullOrWhiteSpace(_session.Token))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Put, "api/admin/system/redis")
        {
            Content = JsonContent.Create(new UpdateRedisFeatureRequest(enabled))
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.Token);

        using var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<RedisFeatureStatusDto>>();
        return payload?.Data;
    }

    private async Task<ApiResponse<T>?> GetAuthorizedAsync<T>(string url)
    {
        if (!_session.IsAuthenticated || string.IsNullOrWhiteSpace(_session.Token))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.Token);

        using var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
    }
}
