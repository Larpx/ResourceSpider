using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Larpx.PersonalTools.ResourceSpider.Core;
using Larpx.PersonalTools.ResourceSpider.Core.Enums;
using Larpx.PersonalTools.ResourceSpider.Core.Interfaces;
using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Downloader;

public class PlaywrightOptions
{
    public string BrowserType { get; set; } = Constants.Defaults.DefaultBrowserType;

    public bool Headless { get; set; } = true;

    public int ViewportWidth { get; set; } = Constants.Defaults.DefaultViewportWidth;

    public int ViewportHeight { get; set; } = Constants.Defaults.DefaultViewportHeight;

    public int Timeout { get; set; } = Constants.Defaults.DefaultPlaywrightTimeout;

    public string WaitUntil { get; set; } = Constants.Defaults.DefaultWaitUntil;

    public int MaxInstances { get; set; } = Constants.Defaults.DefaultMaxInstances;

    public int MaxLifetimeMinutes { get; set; } = Constants.Defaults.DefaultMaxLifetimeMinutes;
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

        IBrowser? browser = null;
        IBrowserContext? context = null;

        try
        {
            browser = await GetBrowserAsync();

            var contextOptions = new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize
                {
                    Width = _options.ViewportWidth,
                    Height = _options.ViewportHeight
                }
            };

            var playwrightConfig = ExtractPlaywrightConfig(request);

            if (playwrightConfig != null)
            {
                if (!string.IsNullOrEmpty(playwrightConfig.UserAgent))
                    contextOptions.UserAgent = playwrightConfig.UserAgent;

                if (playwrightConfig.ProxyConfig != null)
                {
                    contextOptions.Proxy = new Microsoft.Playwright.Proxy
                    {
                        Server = $"http://{playwrightConfig.ProxyConfig.Host}:{playwrightConfig.ProxyConfig.Port}"
                    };
                    if (!string.IsNullOrEmpty(playwrightConfig.ProxyConfig.Username))
                        contextOptions.Proxy.Username = playwrightConfig.ProxyConfig.Username;
                    if (!string.IsNullOrEmpty(playwrightConfig.ProxyConfig.Password))
                        contextOptions.Proxy.Password = playwrightConfig.ProxyConfig.Password;
                }
            }

            context = await browser.NewContextAsync(contextOptions);

            if (request.Headers.Count > 0)
            {
                await context.SetExtraHTTPHeadersAsync(request.Headers);
            }

            var page = await context.NewPageAsync();
            page.SetDefaultTimeout(_options.Timeout);

            if (playwrightConfig != null)
            {
                await ConfigureResourceBlockingAsync(page, playwrightConfig);
            }

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

            if (playwrightConfig != null)
            {
                if (!string.IsNullOrEmpty(playwrightConfig.WaitForSelector))
                {
                    await page.WaitForSelectorAsync(playwrightConfig.WaitForSelector, new PageWaitForSelectorOptions
                    {
                        Timeout = playwrightConfig.WaitForNetworkIdleTimeout
                    });
                }

                if (playwrightConfig.Actions != null && playwrightConfig.Actions.Count > 0)
                {
                    await ExecuteBrowserActionsAsync(page, playwrightConfig.Actions);
                }

                if (playwrightConfig.Scripts != null && playwrightConfig.Scripts.Count > 0)
                {
                    foreach (var script in playwrightConfig.Scripts)
                    {
                        await page.EvaluateAsync(script);
                    }
                }
            }

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

    private static PlaywrightConfig? ExtractPlaywrightConfig(Request request)
    {
        if (request.Metadata.TryGetValue("PlaywrightConfig", out var configObj) && configObj is PlaywrightConfig config)
        {
            return config;
        }
        return null;
    }

    private static async Task ConfigureResourceBlockingAsync(IPage page, PlaywrightConfig config)
    {
        if (!config.DisableImages && !config.DisableCss && !config.DisableFonts) return;

        await page.RouteAsync("**/*", async route =>
        {
            var resourceType = route.Request.ResourceType;
            if (config.DisableImages && resourceType == "image")
            {
                await route.AbortAsync();
                return;
            }
            if (config.DisableCss && resourceType == "stylesheet")
            {
                await route.AbortAsync();
                return;
            }
            if (config.DisableFonts && resourceType == "font")
            {
                await route.AbortAsync();
                return;
            }
            await route.ContinueAsync();
        });
    }

    private static async Task ExecuteBrowserActionsAsync(IPage page, List<BrowserAction> actions)
    {
        foreach (var action in actions)
        {
            if (action.WaitAfterMs.HasValue && action.WaitAfterMs.Value > 0)
            {
                await Task.Delay(action.WaitAfterMs.Value);
            }

            switch (action.ActionType.ToLowerInvariant())
            {
                case "click":
                    if (!string.IsNullOrEmpty(action.Selector))
                        await page.ClickAsync(action.Selector);
                    break;

                case "fill":
                case "input":
                    if (!string.IsNullOrEmpty(action.Selector) && action.Value != null)
                        await page.FillAsync(action.Selector, action.Value);
                    break;

                case "select":
                    if (!string.IsNullOrEmpty(action.Selector) && action.OptionValue != null)
                        await page.SelectOptionAsync(action.Selector, action.OptionValue);
                    break;

                case "hover":
                    if (!string.IsNullOrEmpty(action.Selector))
                        await page.HoverAsync(action.Selector);
                    break;

                case "scroll":
                    var scrollPixels = action.ScrollPixels ?? 300;
                    await page.EvaluateAsync($"window.scrollBy(0, {scrollPixels})");
                    break;

                case "press":
                    if (!string.IsNullOrEmpty(action.Value))
                        await page.Keyboard.PressAsync(action.Value);
                    break;

                case "waitforselector":
                    if (!string.IsNullOrEmpty(action.Selector))
                        await page.WaitForSelectorAsync(action.Selector);
                    break;

                case "evaluate":
                    if (!string.IsNullOrEmpty(action.Script))
                        await page.EvaluateAsync(action.Script);
                    break;
            }
        }
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
