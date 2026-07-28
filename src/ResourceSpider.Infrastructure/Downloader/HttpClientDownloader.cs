using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Larpx.PersonalTools.ResourceSpider.Core;
using Larpx.PersonalTools.ResourceSpider.Core.Enums;
using Larpx.PersonalTools.ResourceSpider.Core.Exceptions;
using Larpx.PersonalTools.ResourceSpider.Core.Interfaces;
using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Downloader;

public class DownloaderOptions
{
    public int ConnectionTimeout { get; set; } = Constants.Defaults.DefaultConnectionTimeout;

    public int RequestTimeout { get; set; } = Constants.Defaults.DefaultRequestTimeout;

    public int MaxConcurrentRequests { get; set; } = Constants.Defaults.DefaultMaxConcurrentRequests;

    public int RetryCount { get; set; } = Constants.Defaults.DefaultRetryCount;

    public int RetryDelayMs { get; set; } = Constants.Defaults.DefaultRetryDelayMs;

    public bool SkipTlsVerification { get; set; }

    public int MaxRedirects { get; set; } = 10;

    public bool FollowRedirects { get; set; } = true;
}

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
        _options = options.Value;
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.RequestTimeout);

        if (_options.SkipTlsVerification)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
                AllowAutoRedirect = _options.FollowRedirects,
                MaxAutomaticRedirections = _options.MaxRedirects,
                AutomaticDecompression = DecompressionMethods.All
            };
        }
    }

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
                var httpResponse = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseContentRead, ct);
                var duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

                response.StatusCode = (int)httpResponse.StatusCode;
                response.Duration = duration;
                response.ContentType = httpResponse.Content.Headers.ContentType?.ToString() ?? string.Empty;

                foreach (var header in httpResponse.Headers)
                {
                    response.Headers[header.Key] = string.Join(", ", header.Value);
                }

                var rawBytes = await httpResponse.Content.ReadAsByteArrayAsync(ct);
                response.Content = DetectAndConvertEncoding(rawBytes, httpResponse.Content.Headers.ContentType?.CharSet);
                response.ContentLength = response.Content.Length;
                response.Status = httpResponse.IsSuccessStatusCode
                    ? RequestStatus.Success
                    : RequestStatus.Failed;

                if (!httpResponse.IsSuccessStatusCode && retries < _options.RetryCount && IsRetryableStatusCode((int)httpResponse.StatusCode))
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
            catch (TaskCanceledException ex)
            {
                lastException = ex;
                response.Status = RequestStatus.Failed;
                response.Error = "请求超时";
                response.ErrorType = ErrorType.Timeout;
                _logger.LogWarning(ex, "下载 {Url} 超时", request.Url);

                if (retries >= _options.RetryCount) return response;
                retries++;
                await Task.Delay(CalculateRetryDelay(retries - 1), ct);
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                retries++;

                if (retries > _options.RetryCount)
                {
                    response.Status = RequestStatus.Failed;
                    response.Error = ex.Message;
                    response.ErrorType = ErrorType.NetworkError;
                    _logger.LogError(ex, "下载 {Url} 失败，已重试 {Retries} 次", request.Url, retries);
                    return response;
                }

                var delay = CalculateRetryDelay(retries - 1);
                _logger.LogWarning(ex, "下载 {Url} 出错，{Delay}ms 后重试...", request.Url, delay);
                await Task.Delay(delay, ct);
            }
            catch (Exception ex)
            {
                lastException = ex;
                response.Status = RequestStatus.Failed;
                response.Error = ex.Message;
                response.ErrorType = ErrorType.NetworkError;
                _logger.LogError(ex, "下载 {Url} 发生未知错误", request.Url);
                return response;
            }
        }

        response.Status = RequestStatus.Failed;
        response.Error = lastException?.Message;
        response.ErrorType = ErrorType.NetworkError;
        return response;
    }

    private static HttpRequestMessage BuildHttpRequest(Request request)
    {
        var message = new HttpRequestMessage(
            new HttpMethod(request.Method),
            request.Url);

        if (request.Body is { Length: > 0 })
        {
            message.Content = new ByteArrayContent(request.Body);
            if (request.Headers.TryGetValue("Content-Type", out var contentType))
            {
                message.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }
        }

        foreach (var header in request.Headers)
        {
            if (!header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                message.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return message;
    }

    private static byte[] DetectAndConvertEncoding(byte[] rawBytes, string? charSet)
    {
        if (rawBytes.Length == 0) return rawBytes;

        try
        {
            var encoding = DetectEncoding(rawBytes, charSet);
            if (encoding == Encoding.UTF8) return rawBytes;

            var text = encoding.GetString(rawBytes);
            return Encoding.UTF8.GetBytes(text);
        }
        catch
        {
            return rawBytes;
        }
    }

    private static Encoding DetectEncoding(byte[] rawBytes, string? charSet)
    {
        if (!string.IsNullOrEmpty(charSet))
        {
            try
            {
                return Encoding.GetEncoding(charSet.Trim('"'));
            }
            catch { }
        }

        if (rawBytes.Length >= 3 && rawBytes[0] == 0xEF && rawBytes[1] == 0xBB && rawBytes[2] == 0xBF)
            return Encoding.UTF8;

        if (rawBytes.Length >= 2)
        {
            if (rawBytes[0] == 0xFF && rawBytes[1] == 0xFE) return Encoding.Unicode;
            if (rawBytes[0] == 0xFE && rawBytes[1] == 0xFF) return Encoding.BigEndianUnicode;
        }

        try
        {
            var content = Encoding.ASCII.GetString(rawBytes);
            var metaMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"charset=[""']?([^""'\s>]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (metaMatch.Success)
            {
                return Encoding.GetEncoding(metaMatch.Groups[1].Value);
            }
        }
        catch { }

        return Encoding.UTF8;
    }

    private static bool IsRetryableStatusCode(int statusCode)
    {
        return statusCode is >= 500 or 408 or 429;
    }

    private int CalculateRetryDelay(int retryAttempt)
    {
        var delay = (int)(_options.RetryDelayMs * Math.Pow(2, retryAttempt));
        return Math.Min(delay, 60000);
    }
}
