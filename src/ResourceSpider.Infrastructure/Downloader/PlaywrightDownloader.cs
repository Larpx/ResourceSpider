using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using ResourceSpider.Core.Exceptions;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Downloader;

public class PlaywrightOptions
{
    public string BrowserType { get; set; } = "Chromium";
    public bool Headless { get; set; } = true;
    public int ViewportWidth { get; set; } = 1920;
    public int ViewportHeight { get; set; } = 1080;
    public int Timeout { get; set; } = 30000;
    public string WaitUntil { get; set; } = "NetworkIdle";
    public int MaxInstances { get; set; } = 5;
    public int MaxLifetimeMinutes { get; set; } = 30;
}

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

    private async Task<IBrowser> GetBrowserAsync()
    {
        if (_browser != null && _browser.IsConnected)
            return _browser;

        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        
        _browser = _options.BrowserType.ToLower() switch
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

    private async Task<Response> DownloadInternalAsync(Request request, CancellationToken ct)
    {
        var response = new Response
        {
            RequestId = request.RequestId,
            Url = request.Url
        };

        try
        {
            var browser = await GetBrowserAsync();
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize 
                { 
                    Width = _options.ViewportWidth, 
                    Height = _options.ViewportHeight 
                }
            });

            var page = await context.NewPageAsync();
            page.SetDefaultTimeout(_options.Timeout);

            var waitUntil = _options.WaitUntil.ToLower() switch
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
            response.Status = pageResponse != null && pageResponse.Ok == true 
                ? Core.Enums.RequestStatus.Success 
                : Core.Enums.RequestStatus.Failed;

            await context.CloseAsync();
        }
        catch (Exception ex)
        {
            response.Status = Core.Enums.RequestStatus.Failed;
            response.Error = ex.Message;
            response.ErrorType = Core.Enums.ErrorType.NetworkError;
            _logger.LogError(ex, "Playwright download failed for {Url}", request.Url);
        }

        return response;
    }

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
