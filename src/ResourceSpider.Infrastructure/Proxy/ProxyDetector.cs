using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ProxyModel = ResourceSpider.Core.Models.Proxy;

namespace ResourceSpider.Infrastructure.Proxy;

public interface IProxySupplier
{
    Task<IEnumerable<ProxyModel>> GetProxiesAsync(CancellationToken ct = default);
}

public interface IProxyValidator
{
    Task<bool> IsValidAsync(ProxyModel proxy, CancellationToken ct = default);
}

public class ProxyDetectorOptions
{
    public string TestUrl { get; set; } = "https://httpbin.org/ip";
    public int TimeoutSeconds { get; set; } = 10;
    public int ConcurrentCount { get; set; } = 5;
}

public class ProxyDetector
{
    private readonly IProxyValidator _validator;
    private readonly IProxyPool _proxyPool;
    private readonly ILogger<ProxyDetector> _logger;
    private readonly ProxyDetectorOptions _options;

    public ProxyDetector(
        IProxyValidator validator,
        IProxyPool proxyPool,
        IOptions<ProxyDetectorOptions> options,
        ILogger<ProxyDetector> logger)
    {
        _validator = validator;
        _proxyPool = proxyPool;
        _options = options.Value;
        _logger = logger;
    }

    public async Task DetectAsync(ProxyModel proxy, CancellationToken ct = default)
    {
        var isValid = await _validator.IsValidAsync(proxy, ct);
        
        proxy.LastCheckedAt = DateTime.UtcNow;
        proxy.IsAvailable = isValid;

        if (isValid)
        {
            await _proxyPool.AddProxyAsync(proxy, ct);
            _logger.LogInformation("Proxy {ProxyId} is available", proxy.ProxyId);
        }
        else
        {
            proxy.FailureCount++;
            _logger.LogWarning("Proxy {ProxyId} is unavailable", proxy.ProxyId);
        }
    }

    public async Task DetectAllAsync(IEnumerable<ProxyModel> proxies, CancellationToken ct = default)
    {
        var semaphore = new SemaphoreSlim(_options.ConcurrentCount);
        var tasks = proxies.Select(async proxy =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                await DetectAsync(proxy, ct);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }
}
