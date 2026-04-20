using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ProxyModel = ResourceSpider.Core.Models.Proxy;

namespace ResourceSpider.Infrastructure.Proxy;

public class ProxyPool : IProxyPool
{
    private readonly ConcurrentDictionary<string, ProxyModel> _proxies = new();
    private readonly ILogger<ProxyPool> _logger;
    private readonly Random _random = new();

    public ProxyPool(ILogger<ProxyPool> logger)
    {
        _logger = logger;
    }

    public Task<ProxyModel?> GetProxyAsync(CancellationToken ct = default)
    {
        var available = _proxies.Values
            .Where(p => p.IsAvailable && p.HealthScore > 0.5)
            .ToList();

        if (!available.Any())
        {
            return Task.FromResult<ProxyModel?>(null);
        }

        var proxy = available[_random.Next(available.Count)];
        return Task.FromResult<ProxyModel?>(proxy);
    }

    public Task AddProxyAsync(ProxyModel proxy, CancellationToken ct = default)
    {
        _proxies[proxy.ProxyId] = proxy;
        _logger.LogInformation("Added proxy {ProxyId}", proxy.ProxyId);
        return Task.CompletedTask;
    }

    public Task RemoveProxyAsync(string proxyId, CancellationToken ct = default)
    {
        _proxies.TryRemove(proxyId, out _);
        return Task.CompletedTask;
    }

    public Task MarkSuccessAsync(string proxyId, CancellationToken ct = default)
    {
        if (_proxies.TryGetValue(proxyId, out var proxy))
        {
            proxy.SuccessCount++;
            proxy.HealthScore = CalculateHealthScore(proxy);
            proxy.IsAvailable = proxy.HealthScore > 0.3;
        }
        return Task.CompletedTask;
    }

    public Task MarkFailureAsync(string proxyId, CancellationToken ct = default)
    {
        if (_proxies.TryGetValue(proxyId, out var proxy))
        {
            proxy.FailureCount++;
            proxy.HealthScore = CalculateHealthScore(proxy);
            proxy.IsAvailable = proxy.HealthScore > 0.3;
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ProxyModel>> GetAllProxiesAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IEnumerable<ProxyModel>>(_proxies.Values.ToList());
    }

    private static double CalculateHealthScore(ProxyModel proxy)
    {
        var total = proxy.SuccessCount + proxy.FailureCount;
        if (total == 0) return 1.0;
        return (double)proxy.SuccessCount / total;
    }
}
