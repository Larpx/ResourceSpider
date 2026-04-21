using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using ResourceSpider.Core;
using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Downloader;

/// <summary>
/// Playwright 浏览器下载器配置选项
/// </summary>
public class PlaywrightOptions
{
    /// <summary>
    /// 浏览器类型（Chromium、Firefox、WebKit）
    /// </summary>
    public string BrowserType { get; set; } = Constants.Defaults.DefaultBrowserType;

    /// <summary>
    /// 是否使用无头模式
    /// </summary>
    public bool Headless { get; set; } = true;

    /// <summary>
    /// 视口宽度（像素）
    /// </summary>
    public int ViewportWidth { get; set; } = Constants.Defaults.DefaultViewportWidth;

    /// <summary>
    /// 视口高度（像素）
    /// </summary>
    public int ViewportHeight { get; set; } = Constants.Defaults.DefaultViewportHeight;

    /// <summary>
    /// 页面超时时间（毫秒）
    /// </summary>
    public int Timeout { get; set; } = Constants.Defaults.DefaultPlaywrightTimeout;

    /// <summary>
    /// 等待页面加载完成的状态（NetworkIdle、DOMContentLoaded、Load）
    /// </summary>
    public string WaitUntil { get; set; } = Constants.Defaults.DefaultWaitUntil;

    /// <summary>
    /// 最大浏览器实例数
    /// </summary>
    public int MaxInstances { get; set; } = Constants.Defaults.DefaultMaxInstances;

    /// <summary>
    /// 浏览器实例最大存活时间（分钟）
    /// </summary>
    public int MaxLifetimeMinutes { get; set; } = Constants.Defaults.DefaultMaxLifetimeMinutes;
}

/// <summary>
/// 基于 Playwright 的浏览器下载器，支持 JavaScript 渲染的页面采集
/// 使用信号量控制并发浏览器实例数，确保资源合理使用
/// </summary>
public class PlaywrightDownloader : IDownloader, IAsyncDisposable
{
    private readonly PlaywrightOptions _options;
    private readonly ILogger<PlaywrightDownloader> _logger;
    private readonly SemaphoreSlim _semaphore;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private bool _disposed;

    public PlaywrightDownloader(
        IOptions<PlaywrightOptions> options,
        ILogger<PlaywrightDownloader> logger)
    {
        _options = options.Value;
        _logger = logger;
        _semaphore = new SemaphoreSlim(_options.MaxInstances, _options.MaxInstances);
    }

    /// <summary>
    /// 获取或创建浏览器实例（懒加载，首次使用时初始化）
    /// </summary>
    private async Task<IBrowser> GetBrowserAsync()
    {
        if (_browser != null && _browser.IsConnected)
            return _browser;

        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        _browser = _options.BrowserType.ToLowerInvariant() switch
        {
            "firefox" => await _playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = _options.Headless
            }),
            "webkit" => await _playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = _options.Headless
            }),
            _ => await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = _options.Headless
            })
        };

        return _browser;
    }

    /// <summary>
    /// 使用 Playwright 浏览器下载页面内容，支持 JavaScript 渲染
    /// </summary>
    public async Task<Response> DownloadAsync(Request request, CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            return await DownloadInternalAsync(request, ct);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 内部下载实现：创建浏览器上下文 → 打开页面 → 导航到 URL → 提取内容
    /// </summary>
    private async Task<Response> DownloadInternalAsync(Request request, CancellationToken ct)
    {
        var response = new Response
        {
            RequestId = request.RequestId,
            Url = request.Url
        };

        IBrowser? browser = null;
        IBrowserContext? context = null;

        try
        {
            browser = await GetBrowserAsync();
            context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize
                {
                    Width = _options.ViewportWidth,
                    Height = _options.ViewportHeight
                }
            });

            var page = await context.NewPageAsync();
            page.SetDefaultTimeout(_options.Timeout);

            var waitUntil = _options.WaitUntil.ToLowerInvariant() switch
            {
                "domcontentloaded" => WaitUntilState.DOMContentLoaded,
                "load" => WaitUntilState.Load,
                _ => WaitUntilState.NetworkIdle
            };

            var startTime = DateTime.UtcNow;
            var pageResponse = await page.GotoAsync(request.Url, new PageGotoOptions
            {
                WaitUntil = waitUntil
            });

            response.Duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            response.StatusCode = pageResponse?.Status ?? 0;
            response.Content = System.Text.Encoding.UTF8.GetBytes(await page.ContentAsync());
            response.ContentLength = response.Content.Length;
            response.ContentType = "text/html";
            response.Status = pageResponse is { Ok: true }
                ? RequestStatus.Success
                : RequestStatus.Failed;
        }
        catch (Exception ex)
        {
            response.Status = RequestStatus.Failed;
            response.Error = ex.Message;
            response.ErrorType = ErrorType.NetworkError;
            _logger.LogError(ex, "Playwright 下载 {Url} 失败", request.Url);
        }
        finally
        {
            if (context != null)
            {
                await context.CloseAsync();
            }
        }

        return response;
    }

    /// <summary>
    /// 异步释放资源，关闭浏览器和 Playwright 实例
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        if (_browser != null)
        {
            await _browser.CloseAsync();
        }
        _playwright?.Dispose();
        _semaphore.Dispose();
        _disposed = true;
    }
}
