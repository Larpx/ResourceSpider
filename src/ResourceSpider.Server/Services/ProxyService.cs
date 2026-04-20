using System.Net;
using ResourceSpider.Core.Models;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface IProxyService
{
    Task<ProxyDto> AddAsync(CreateProxyRequest request);
    Task<List<ProxyDto>> GetListAsync(int pageIndex, int pageSize);
    Task<bool> DeleteAsync(string proxyId);
    Task<ProxyTestResponse> TestAsync(ProxyTestRequest request);
}

public class ProxyService : IProxyService
{
    private readonly IProxyRepository _proxyRepository;
    private readonly ILogger<ProxyService> _logger;

    public ProxyService(
        IProxyRepository proxyRepository,
        ILogger<ProxyService> logger)
    {
        _proxyRepository = proxyRepository;
        _logger = logger;
    }

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

    public async Task<List<ProxyDto>> GetListAsync(int pageIndex, int pageSize)
    {
        var proxies = await _proxyRepository.GetAllAsync(pageIndex, pageSize);
        return proxies.Select(MapToDto).ToList();
    }

    public async Task<bool> DeleteAsync(string proxyId)
    {
        var proxy = await _proxyRepository.GetByIdAsync(proxyId);
        if (proxy == null) return false;

        await _proxyRepository.DeleteAsync(proxyId);
        _logger.LogInformation("Proxy {ProxyId} deleted", proxyId);
        return true;
    }

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
