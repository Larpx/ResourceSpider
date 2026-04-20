using System.Net;
using Microsoft.Extensions.Logging;
using ResourceSpider.Core.Models;
using ProxyModel = ResourceSpider.Core.Models.Proxy;

namespace ResourceSpider.Infrastructure.Proxy;

public class HttpProxyValidator : IProxyValidator
{
    private readonly ILogger<HttpProxyValidator> _logger;
    private readonly string _testUrl;
    private readonly int _timeoutSeconds;

    public HttpProxyValidator(
        ILogger<HttpProxyValidator> logger,
        string testUrl = "https://httpbin.org/ip",
        int timeoutSeconds = 10)
    {
        _logger = logger;
        _testUrl = testUrl;
        _timeoutSeconds = timeoutSeconds;
    }

    public async Task<bool> IsValidAsync(ProxyModel proxy, CancellationToken ct = default)
    {
        try
        {
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(proxy.Address),
                UseProxy = true
            };

            using var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(_timeoutSeconds)
            };

            var response = await client.GetAsync(_testUrl, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Proxy {Address} validation failed", proxy.Address);
            return false;
        }
    }
}
