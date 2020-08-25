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

        public PuppeteerDownloader()
        {
            Init();
        }

        /// <summary>
        /// 初始化Puppeteer
        /// </summary>
        private static void Init()
        {
            try
            {
                Platform oPlatform = Platform.Unknown;
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

                CommonHelper.PingIp(DefaultDownloadHost);

                new BrowserFetcher(new BrowserFetcherOptions()
                {
                    Host = "",
                    Platform = oPlatform


                }).DownloadAsync(ChromiumRevision).Wait();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public PuppeteerDownloader(ILogger logger, PPPoEService pppoeService)
        {
            _logger = logger;
            _pppoeService = pppoeService;
        }

        public Task<Response> DownloadAsync(Request request)
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
