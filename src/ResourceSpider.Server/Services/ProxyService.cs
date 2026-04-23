using System.Diagnostics;
using System.Net;
using ResourceSpider.Core.Models;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

/// <summary>
/// 代理服务接口，提供代理的添加、查询、删除和连通性测试功能
/// </summary>
public interface IProxyService
{
    /// <summary>
    /// 添加新的代理服务器配置
    /// </summary>
    /// <param name="request">创建代理请求</param>
    /// <returns>创建后的代理 DTO</returns>
    Task<ProxyDto> AddAsync(CreateProxyRequest request);

    /// <summary>
    /// 更新指定代理服务器配置
    /// </summary>
    /// <param name="proxyId">代理唯一标识</param>
    /// <param name="request">更新代理请求</param>
    /// <returns>更新成功返回 true，代理不存在返回 false</returns>
    Task<bool> UpdateAsync(string proxyId, UpdateProxyRequest request);

    /// <summary>
    /// 分页获取代理列表，支持状态和关键字筛选
    /// </summary>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="status">状态筛选</param>
    /// <param name="keyword">关键字筛选（主机或 ID）</param>
    /// <returns>分页代理响应</returns>
    Task<ProxyListResponse> GetPagedAsync(int pageIndex, int pageSize, int? status = null, string? keyword = null);

    /// <summary>
    /// 分页获取代理列表
    /// </summary>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>代理 DTO 列表</returns>
    Task<List<ProxyDto>> GetListAsync(int pageIndex, int pageSize);

    /// <summary>
    /// 删除指定代理
    /// </summary>
    /// <param name="proxyId">代理唯一标识</param>
    /// <returns>删除成功返回 true，代理不存在返回 false</returns>
    Task<bool> DeleteAsync(string proxyId);

    /// <summary>
    /// 测试代理服务器的连通性和响应速度
    /// </summary>
    /// <param name="request">代理测试请求，包含主机和端口</param>
    /// <returns>代理测试响应，包含可用性、延迟和错误信息</returns>
    Task<ProxyTestResponse> TestAsync(ProxyTestRequest request);
}

/// <summary>
/// 代理服务实现，管理代理服务器的配置、查询、删除和连通性检测
/// </summary>
public class ProxyService : IProxyService
{
    /// <summary>
    /// 代理数据仓库，用于代理实体的持久化操作
    /// </summary>
    private readonly IProxyRepository _proxyRepository;

    /// <summary>
    /// 日志记录器，用于记录代理操作相关事件
    /// </summary>
    private readonly ILogger<ProxyService> _logger;

    /// <summary>
    /// 初始化代理服务实例
    /// </summary>
    /// <param name="proxyRepository">代理数据仓库</param>
    /// <param name="logger">日志记录器</param>
    public ProxyService(
        IProxyRepository proxyRepository,
        ILogger<ProxyService> logger)
    {
        _proxyRepository = proxyRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ProxyDto> AddAsync(CreateProxyRequest request)
    {
        var proxyId = Guid.NewGuid().ToString("N");
        var entity = new ProxyEntity
        {
            ProxyId = proxyId,
            Host = request.Host,
            Port = request.Port,
            Protocol = request.Protocol,
            Username = request.Username,
            Password = request.Password,
            Status = 0
        };

        await _proxyRepository.AddAsync(entity);
        _logger.LogInformation("Proxy {ProxyId} added: {Host}:{Port}", proxyId, request.Host, request.Port);

        return MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(string proxyId, UpdateProxyRequest request)
    {
        var entity = await _proxyRepository.GetByIdAsync(proxyId);
        if (entity == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Host))
        {
            entity.Host = request.Host;
        }

        if (request.Port.HasValue)
        {
            entity.Port = request.Port.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Protocol))
        {
            entity.Protocol = request.Protocol;
        }

        if (request.Username != null)
        {
            entity.Username = request.Username;
        }

        if (request.Password != null)
        {
            entity.Password = request.Password;
        }

        if (request.Status.HasValue)
        {
            entity.Status = request.Status.Value;
        }

        await _proxyRepository.UpdateAsync(entity);
        _logger.LogInformation("Proxy {ProxyId} updated", proxyId);
        return true;
    }

    /// <inheritdoc />
    public async Task<ProxyListResponse> GetPagedAsync(int pageIndex, int pageSize, int? status = null, string? keyword = null)
    {
        var proxies = await _proxyRepository.GetAllAsync(pageIndex, pageSize, status, keyword);
        var total = await _proxyRepository.CountAsync(status, keyword);

        return new ProxyListResponse(
            proxies.Select(MapToDto).ToList(),
            (int)total,
            pageIndex,
            pageSize);
    }

    /// <inheritdoc />
    public async Task<List<ProxyDto>> GetListAsync(int pageIndex, int pageSize)
    {
        var paged = await GetPagedAsync(pageIndex, pageSize);
        return paged.Proxies;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string proxyId)
    {
        var proxy = await _proxyRepository.GetByIdAsync(proxyId);
        if (proxy == null) return false;

        await _proxyRepository.DeleteAsync(proxyId);
        _logger.LogInformation("Proxy {ProxyId} deleted", proxyId);
        return true;
    }

    /// <inheritdoc />
    public async Task<ProxyTestResponse> TestAsync(ProxyTestRequest request)
    {
        string? host = request.Host;
        int? port = request.Port;
        string protocol = "http";
        string? username = null;
        string? password = null;

        if (!string.IsNullOrWhiteSpace(request.ProxyId))
        {
            var proxyEntity = await _proxyRepository.GetByIdAsync(request.ProxyId);
            if (proxyEntity is null)
            {
                return new ProxyTestResponse(
                    IsAvailable: false,
                    DurationMs: null,
                    Error: "代理不存在"
                );
            }

            host = proxyEntity.Host;
            port = proxyEntity.Port;
            protocol = proxyEntity.Protocol;
            username = proxyEntity.Username;
            password = proxyEntity.Password;
        }

        if (string.IsNullOrWhiteSpace(host) || port is null or <= 0 or > 65535)
        {
            return new ProxyTestResponse(
                IsAvailable: false,
                DurationMs: null,
                Error: "代理主机或端口无效"
            );
        }

        try
        {
            var proxyAddress = host.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                               || host.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? host
                : $"{protocol.ToLowerInvariant()}://{host}";

            var webProxy = new WebProxy(proxyAddress, port.Value);
            if (!string.IsNullOrWhiteSpace(username))
            {
                webProxy.Credentials = new NetworkCredential(username, password);
            }

            using var handler = new HttpClientHandler
            {
                Proxy = webProxy,
                UseProxy = true
            };

            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
            var stopwatch = Stopwatch.StartNew();
            using var response = await client.GetAsync("https://httpbin.org/ip", HttpCompletionOption.ResponseHeadersRead);
            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                return new ProxyTestResponse(
                    IsAvailable: false,
                    DurationMs: (int)stopwatch.ElapsedMilliseconds,
                    Error: $"代理连通失败，HTTP {(int)response.StatusCode}"
                );
            }

            return new ProxyTestResponse(
                IsAvailable: true,
                DurationMs: (int)stopwatch.ElapsedMilliseconds,
                Error: null
            );
        }
        catch (OperationCanceledException)
        {
            return new ProxyTestResponse(
                IsAvailable: false,
                DurationMs: null,
                Error: "代理测试超时"
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Proxy test failed for {Host}:{Port}", host, port);
            return new ProxyTestResponse(
                IsAvailable: false,
                DurationMs: null,
                Error: ex.Message
            );
        }
    }

    /// <summary>
    /// 将代理实体映射为代理 DTO
    /// </summary>
    /// <param name="entity">代理实体</param>
    /// <returns>代理 DTO</returns>
    private static ProxyDto MapToDto(ProxyEntity entity)
    {
        return new ProxyDto(
            ProxyId: entity.ProxyId,
            Host: entity.Host,
            Port: entity.Port,
            Protocol: entity.Protocol,
            Username: entity.Username,
            Status: entity.Status,
            SuccessCount: entity.SuccessCount,
            FailureCount: entity.FailureCount,
            LastCheckedAt: entity.LastCheckedAt,
            NextCheckAt: entity.NextCheckAt
        );
    }
}
