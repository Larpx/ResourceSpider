using Larpx.ResourceSpider.DotnetSpiderEx.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using ByteArrayContent = Larpx.ResourceSpider.DotnetSpiderEx.Http.ByteArrayContent;

namespace Larpx.ResourceSpider.DotnetSpiderEx.Downloader
{
    /// <summary>
    /// 空下载器，只请求页面结果，不下载数据
    /// </summary>
    public class EmptyDownloader : IDownloader
    {
        private int _downloadCount;

        protected ILogger Logger { get; }

        public EmptyDownloader(ILogger<EmptyDownloader> logger)
        {
            Logger = logger;
        }

        public Task<Response> DownloadAsync(Request request)
        {
            Interlocked.Increment(ref _downloadCount);
            if ((_downloadCount % 100) == 0)
            {
                Logger.LogInformation($"download {_downloadCount} already");
            }

            var response = new Response
            {
                RequestHash = request.Hash,
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes(""))
            };
            return Task.FromResult(response);
        }

        public string Name => Downloaders.Empty;
    }
}
