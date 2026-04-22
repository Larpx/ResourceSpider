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
    public async Task<List<ProxyDto>> GetListAsync(int pageIndex, int pageSize)
    {
        var proxies = await _proxyRepository.GetAllAsync(pageIndex, pageSize);
        return proxies.Select(MapToDto).ToList();
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
        try
        {
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(request.Host ?? string.Empty, request.Port ?? 0),
                UseProxy = true
            };

            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
            var startTime = DateTime.UtcNow;
            await client.GetAsync("https://httpbin.org/ip");
            var duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            return new ProxyTestResponse(
                IsAvailable: true,
                DurationMs: duration,
                Error: null
            );
        }
        catch (Exception ex)
        {
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
