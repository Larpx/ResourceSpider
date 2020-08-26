using Larpx.ResourceSpider.BaseLibrary.Helpers;
using Larpx.ResourceSpider.BaseLibrary.Helpers.Web;
using Larpx.ResourceSpider.Http.Content;
using Larpx.ResourceSpider.Http.Service;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Larpx.ResourceSpider.Http.Downloader
{
    public class PuppeteerDownloader : IDownloader
    {
        private readonly ILogger _logger;
        private readonly PPPoEService _pppoeService;
        private const int ChromiumRevision = BrowserFetcher.DefaultRevision;

        private const string BackupDownloadHost = @"https://mirrors.huaweicloud.com/";
        private const string DefaultDownloadHost = "https://storage.googleapis.com";

        /// <summary>
        /// 请求响应头
        /// </summary>
        public Response Response { get; set; } = new Response();

        /// <summary>
        /// 请求的Cookies信息
        /// </summary>
        public List<CookieItem> CookieArr { get; set; } = new List<CookieItem>();

        /// <summary>
        /// 使用Puppeteer获取网站信息
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="pppoeService"></param>
        public PuppeteerDownloader(ILogger<PuppeteerDownloader> logger, PPPoEService pppoeService)
        {
            _logger = logger;
            _pppoeService = pppoeService;
            Init();
        }

        /// <summary>
        /// 初始化Puppeteer
        /// </summary>
        private void Init()
        {
            try
            {
                string sHost = "";
                Platform oPlatform = Platform.Unknown;

                //测试平台
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    oPlatform = Platform.Linux;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (SystemInfoHelper.Is64bit())
                        oPlatform = Platform.Win64;
                    else
                        oPlatform = Platform.Win32;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    oPlatform = Platform.MacOS;
                }

                //测试下载地址
                if (NetHelper.PingIp(DefaultDownloadHost))
                    sHost = DefaultDownloadHost;
                else if (NetHelper.PingIp(BackupDownloadHost))
                    sHost = BackupDownloadHost;

                new BrowserFetcher(new BrowserFetcherOptions()
                {
                    Host = sHost,
                    Platform = oPlatform
                }).DownloadAsync(ChromiumRevision).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError($"系统初始化失败，错误信息：{ex.Message}");
                throw ex;
            }
        }

        /// <summary>
        /// 请求目标网站并返回Response对象
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Response> DownloadResponseAsync(Request request)
        {
            try
            {
                //计时器
                var stopwatch = new Stopwatch();
                //Starting headless browser
                using (Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    IgnoreHTTPSErrors = true,
                    IgnoreDefaultArgs = true
                }))
                {
                    var pages = await browser.PagesAsync();
                    using (var firstPage = pages.Length > 0 ? pages[0] : await browser.NewPageAsync())
                    {
                        //设置Cookies信息

                        await firstPage.SetCookieAsync(CookieArr.ToArray());
                        //执行请求
                        stopwatch.Start();
                        var oPuppeteerResponse = await firstPage.GoToAsync(request.RequestUri.ToString());
                        stopwatch.Stop();

                        //处理返回的响应信息
                        Response = new Response
                        {
                            ElapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds,
                            StatusCode = oPuppeteerResponse.Status
                        };
                        foreach (var header in oPuppeteerResponse.Headers)
                        {
                            Response.Headers.Add(header.Key, new HashSet<string>() { header.Value });
                        }
                        Response.RequestHash = request.Hash;
                        Response.Content = new ResponseContent
                        {
                            Data = await oPuppeteerResponse.BufferAsync()
                        };
                        foreach (var header in oPuppeteerResponse.Headers)
                        {
                            Response.Content.Headers.Add(header.Key, new HashSet<string>() { header.Value });
                        }

                        return Response;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{request.RequestUri} 下载失败，错误信息：{ex.Message}");
                throw ex;
            }
        }

        public async Task<string> DownloadStringAsync(Request request)
        {
            try
            {
                //计时器
                var stopwatch = new Stopwatch();
                //Starting headless browser
                using (Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    IgnoreHTTPSErrors = true,
                    IgnoreDefaultArgs = true
                }))
                {
                    var pages = await browser.PagesAsync();
                    using (var firstPage = pages.Length > 0 ? pages[0] : await browser.NewPageAsync())
                    {
                        //设置Cookies信息

                        await firstPage.SetCookieAsync(CookieArr.ToArray());
                        //执行请求
                        stopwatch.Start();
                        var oPuppeteerResponse = await firstPage.GoToAsync(request.RequestUri.ToString());
                        stopwatch.Stop();

                        //处理返回的响应信息
                        Response = new Response
                        {
                            ElapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds,
                            StatusCode = oPuppeteerResponse.Status
                        };
                        foreach (var header in oPuppeteerResponse.Headers)
                        {
                            Response.Headers.Add(header.Key, new HashSet<string>() { header.Value });
                        }
                        Response.RequestHash = request.Hash;
                        Response.Content = new ResponseContent
                        {
                            Data = await oPuppeteerResponse.BufferAsync()
                        };
                        foreach (var header in oPuppeteerResponse.Headers)
                        {
                            Response.Content.Headers.Add(header.Key, new HashSet<string>() { header.Value });
                        }

                        return firstPage.GetContentAsync().Result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{request.RequestUri} 下载失败，错误信息：{ex.Message}");
                throw ex;
            }
        }
    }
}
