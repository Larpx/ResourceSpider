using Larpx.ResourceSpider.DotnetSpiderEx.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using ByteArrayContent = Larpx.ResourceSpider.DotnetSpiderEx.Http.ByteArrayContent;

namespace Larpx.ResourceSpider.DotnetSpiderEx.Downloader
{
    /// <summary>
    /// PuppeteerÏÂÔØÆ÷
    /// </summary>
    public class PuppeteerDownloader : IDownloader
    {
        private ILogger<PuppeteerDownloader> _logger;

        public PuppeteerDownloader(ILogger<PuppeteerDownloader> logger)
        {
            _logger = logger;
        }

        public Task<Response> DownloadAsync(Request request)
        {
            var response = new Response
            {
                RequestHash = request.Hash,
                StatusCode = HttpStatusCode.Gone,
                Content = new ByteArrayContent(
                    Encoding.UTF8.GetBytes("Not impl"))
            };
            return Task.FromResult(response);
        }

        public string Name => Downloaders.Puppeteer;
    }
}
