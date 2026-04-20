using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ResourceSpider.Core.Exceptions;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Downloader;

public class DownloaderOptions
{
    public int ConnectionTimeout { get; set; } = 30;
    public int RequestTimeout { get; set; } = 60;
    public int MaxConcurrentRequests { get; set; } = 10;
    public int RetryCount { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
}

public class HttpClientDownloader : IDownloader, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly DownloaderOptions _options;
    private readonly ILogger<HttpClientDownloader> _logger;
    private bool _disposed;

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

    public async Task<Response> DownloadAsync(Request request, CancellationToken ct = default)
    {
        var response = new Response
        {
            RequestId = request.RequestId,
            Url = request.Url
        };

        int retries = 0;
        Exception? lastException = null;

        while (retries <= _options.RetryCount)
        {
            try
            {
                var httpRequest = BuildHttpRequest(request);
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
                    ? Core.Enums.RequestStatus.Success 
                    : Core.Enums.RequestStatus.Failed;

                if (!httpResponse.IsSuccessStatusCode && retries < _options.RetryCount)
                {
                    var delay = CalculateRetryDelay(retries);
                    _logger.LogWarning(
                        "Request {Url} failed with status {Status}. Retrying in {Delay}ms...",
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
                    response.Status = Core.Enums.RequestStatus.Failed;
                    response.Error = ex.Message;
                    response.ErrorType = Core.Enums.ErrorType.NetworkError;
                    _logger.LogError(ex, "Download failed for {Url} after {Retries} retries", 
                        request.Url, retries);
                    return response;
                }

                var delay = CalculateRetryDelay(retries - 1);
                _logger.LogWarning(ex, "Download error for {Url}. Retrying in {Delay}ms...", 
                    request.Url, delay);
                await Task.Delay(delay, ct);
            }
        }

        response.Status = Core.Enums.RequestStatus.Failed;
        response.Error = lastException?.Message;
        response.ErrorType = Core.Enums.ErrorType.NetworkError;
        return response;
    }

    private HttpRequestMessage BuildHttpRequest(Request request)
    {
        var message = new HttpRequestMessage(
            new HttpMethod(request.Method), 
            request.Url);

        if (request.Body != null && request.Body.Length > 0)
        {
            message.Content = new ByteArrayContent(request.Body);
        }

        foreach (var header in request.Headers)
        {
            message.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return message;
    }

    private int CalculateRetryDelay(int retryAttempt)
    {
        return (int)(_options.RetryDelayMs * Math.Pow(2, retryAttempt));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _httpClient.Dispose();
        _disposed = true;
    }
}
