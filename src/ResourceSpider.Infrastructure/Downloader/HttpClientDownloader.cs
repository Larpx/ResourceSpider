using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ResourceSpider.Core;
using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Exceptions;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Downloader;

/// <summary>
/// HTTP 下载器配置选项
/// </summary>
public class DownloaderOptions
{
    /// <summary>
    /// 连接超时时间（秒）
    /// </summary>
    public int ConnectionTimeout { get; set; } = Constants.Defaults.DefaultConnectionTimeout;

    /// <summary>
    /// 请求超时时间（秒）
    /// </summary>
    public int RequestTimeout { get; set; } = Constants.Defaults.DefaultRequestTimeout;

    /// <summary>
    /// 最大并发请求数
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = Constants.Defaults.DefaultMaxConcurrentRequests;

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; } = Constants.Defaults.DefaultRetryCount;

    /// <summary>
    /// 重试延迟基础时间（毫秒），实际延迟按指数退避计算
    /// </summary>
    public int RetryDelayMs { get; set; } = Constants.Defaults.DefaultRetryDelayMs;
}

/// <summary>
/// 基于 HttpClient 的 HTTP 下载器实现，支持自动重试和指数退避
/// 注意：HttpClient 由 IHttpClientFactory 管理，不应在此处 Dispose
/// </summary>
public class HttpClientDownloader : IDownloader
{
    private readonly HttpClient _httpClient;
    private readonly DownloaderOptions _options;
    private readonly ILogger<HttpClientDownloader> _logger;

    public HttpClientDownloader(
        HttpClient httpClient,
        IOptions<DownloaderOptions> options,
        ILogger<HttpClientDownloader> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.RequestTimeout);
    }

    /// <summary>
    /// 下载指定请求的内容，支持自动重试（指数退避策略）
    /// </summary>
    public async Task<Response> DownloadAsync(Request request, CancellationToken ct = default)
    {
        var response = new Response
        {
            RequestId = request.RequestId,
            Url = request.Url
        };

        var retries = 0;
        Exception? lastException = null;

        while (retries <= _options.RetryCount)
        {
            try
            {
                using var httpRequest = BuildHttpRequest(request);
                var startTime = DateTime.UtcNow;
                var httpResponse = await _httpClient.SendAsync(httpRequest, ct);
                var duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

                response.StatusCode = (int)httpResponse.StatusCode;
                response.Duration = duration;
                response.ContentType = httpResponse.Content.Headers.ContentType?.ToString() ?? string.Empty;

                foreach (var header in httpResponse.Headers)
                {
                    response.Headers[header.Key] = string.Join(", ", header.Value);
                }

                response.Content = await httpResponse.Content.ReadAsByteArrayAsync(ct);
                response.ContentLength = response.Content.Length;
                response.Status = httpResponse.IsSuccessStatusCode
                    ? RequestStatus.Success
                    : RequestStatus.Failed;

                if (!httpResponse.IsSuccessStatusCode && retries < _options.RetryCount)
                {
                    var delay = CalculateRetryDelay(retries);
                    _logger.LogWarning("请求 {Url} 失败，状态码 {Status}，{Delay}ms 后重试...",
                        request.Url, httpResponse.StatusCode, delay);
                    await Task.Delay(delay, ct);
                    retries++;
                    continue;
                }

                return response;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                retries++;

                if (retries > _options.RetryCount)
                {
                    response.Status = RequestStatus.Failed;
                    response.Error = ex.Message;
                    response.ErrorType = ErrorType.NetworkError;
                    _logger.LogError(ex, "下载 {Url} 失败，已重试 {Retries} 次",
                        request.Url, retries);
                    return response;
                }

                var delay = CalculateRetryDelay(retries - 1);
                _logger.LogWarning(ex, "下载 {Url} 出错，{Delay}ms 后重试...",
                    request.Url, delay);
                await Task.Delay(delay, ct);
            }
        }

        response.Status = RequestStatus.Failed;
        response.Error = lastException?.Message;
        response.ErrorType = ErrorType.NetworkError;
        return response;
    }

    /// <summary>
    /// 根据请求配置构建 HTTP 请求消息
    /// </summary>
    private static HttpRequestMessage BuildHttpRequest(Request request)
    {
        var message = new HttpRequestMessage(
            new HttpMethod(request.Method),
            request.Url);

        if (request.Body is { Length: > 0 })
        {
            message.Content = new ByteArrayContent(request.Body);
        }

        foreach (var header in request.Headers)
        {
            message.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return message;
    }

    /// <summary>
    /// 计算重试延迟时间（指数退避），上限 60 秒
    /// </summary>
    private int CalculateRetryDelay(int retryAttempt)
    {
        var delay = (int)(_options.RetryDelayMs * Math.Pow(2, retryAttempt));
        return Math.Min(delay, 60000);
    }
}
