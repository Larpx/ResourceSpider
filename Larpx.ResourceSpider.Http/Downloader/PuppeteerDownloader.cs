using Larpx.ResourceSpider.BaseLibrary.Helpers;
using Larpx.ResourceSpider.Http.Service;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System;
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

        public PuppeteerDownloader(ILogger<PuppeteerDownloader> logger, PPPoEService pppoeService)
        {
            _logger = logger;
            _pppoeService = pppoeService;
            Init();
        }

        /// <summary>
        /// 初始化Puppeteer
        /// </summary>
        private static void Init()
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
                if (Helpers.CommonHelper.PingIp(DefaultDownloadHost))
                    sHost = DefaultDownloadHost;
                else if (Helpers.CommonHelper.PingIp(BackupDownloadHost))
                    sHost = BackupDownloadHost;

                new BrowserFetcher(new BrowserFetcherOptions()
                {
                    Host = sHost,
                    Platform = oPlatform
                }).DownloadAsync(ChromiumRevision).Wait();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Response> DownloadResponseAsync(Request request)
        {
            try
            {
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
                        await firstPage.SetCookieAsync(new CookieParam());
                        await firstPage.GoToAsync(request.RequestUri.ToString());
                        var htmlString = await firstPage.GetContentAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Task<string> DownloadStringAsync(Request request)
        {
            throw new NotImplementedException();
        }
    }
}
