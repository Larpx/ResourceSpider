using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Larpx.PersonalTools.ResourceSpider.Core.Enums;
using Larpx.PersonalTools.ResourceSpider.Core.Interfaces;
using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Downloader;

/// <summary>
/// CDP（Chrome DevTools Protocol）下载器，通过连接远程浏览器实例进行页面采集
/// 适用于需要复用已有浏览器实例的场景
/// </summary>
public class CdpDownloader : IDownloader
{
    private readonly ILogger<CdpDownloader> _logger;

    /// <summary>
    /// 初始化 CDP 下载器
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public CdpDownloader(ILogger<CdpDownloader> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 通过 CDP 协议连接远程浏览器并下载页面内容
    /// </summary>
    /// <param name="request">下载请求，需在 Metadata 中包含 CdpUrl</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>下载响应对象</returns>
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
