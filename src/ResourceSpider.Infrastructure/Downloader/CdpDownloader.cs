using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Downloader;

public class CdpDownloader : IDownloader
{
    private readonly ILogger<CdpDownloader> _logger;

    public CdpDownloader(ILogger<CdpDownloader> logger)
    {
        _logger = logger;
    }

    public async Task<Response> DownloadAsync(Request request, CancellationToken ct = default)
    {
        var cdpUrl = request.Metadata.GetValueOrDefault("CdpUrl")?.ToString();
        if (string.IsNullOrEmpty(cdpUrl))
        {
            return new Response
            {
                RequestId = request.RequestId,
                Url = request.Url,
                StatusCode = 400,
                Status = RequestStatus.Failed,
                Error = "CDP URL 未配置",
                ErrorType = ErrorType.AgentError
            };
        }

        IPlaywright? playwright = null;
        IBrowser? browser = null;

        try
        {
            playwright = await Playwright.CreateAsync();
            browser = await playwright.Chromium.ConnectOverCDPAsync(cdpUrl);

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            if (request.Headers.Count > 0)
            {
                await page.SetExtraHTTPHeadersAsync(request.Headers);
            }

            var pageResponse = await page.GotoAsync(request.Url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 60000
            });

            var content = await page.ContentAsync();

            return new Response
            {
                RequestId = request.RequestId,
                Url = request.Url,
                StatusCode = pageResponse?.Status ?? 0,
                Content = System.Text.Encoding.UTF8.GetBytes(content),
                ContentType = "text/html",
                Status = RequestStatus.Success,
                Duration = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CDP 下载失败：{Url}", request.Url);
            return new Response
            {
                RequestId = request.RequestId,
                Url = request.Url,
                StatusCode = 0,
                Status = RequestStatus.Failed,
                Error = ex.Message,
                ErrorType = ErrorType.NetworkError
            };
        }
        finally
        {
            browser?.DisposeAsync().GetAwaiter().GetResult();
            playwright?.Dispose();
        }
    }
}
