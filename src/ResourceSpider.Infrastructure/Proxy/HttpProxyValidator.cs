using System.Net;
using Microsoft.Extensions.Logging;
using Larpx.PersonalTools.ResourceSpider.Core.Models;
using ProxyModel = Larpx.PersonalTools.ResourceSpider.Core.Models.Proxy;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Proxy;

/// <summary>
/// HTTP 代理验证器，通过实际发送 HTTP 请求来验证代理的可用性
/// 使用配置的测试 URL 检测代理是否能正常转发请求
/// </summary>
public class HttpProxyValidator : IProxyValidator
{
    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<HttpProxyValidator> _logger;

    /// <summary>
    /// 用于验证代理的测试 URL，默认为 httpbin.org/ip
    /// </summary>
    private readonly string _testUrl;

    /// <summary>
    /// 代理验证请求超时时间（秒），默认为 10 秒
    /// </summary>
    private readonly int _timeoutSeconds;

    /// <summary>
    /// 初始化 HTTP 代理验证器
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="testUrl">测试 URL，默认为 https://httpbin.org/ip</param>
    /// <param name="timeoutSeconds">超时时间（秒），默认为 10</param>
    public HttpProxyValidator(
        ILogger<HttpProxyValidator> logger,
        string testUrl = "https://httpbin.org/ip",
        int timeoutSeconds = 10)
    {
        _logger = logger;
        _testUrl = testUrl;
        _timeoutSeconds = timeoutSeconds;
    }

    /// <summary>
    /// 验证代理是否可用，通过代理发送 HTTP 请求到测试 URL
    /// 如果响应状态码为成功则认为代理可用
    /// </summary>
    /// <param name="proxy">待验证的代理对象</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>代理可用返回 true，否则返回 false</returns>
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
