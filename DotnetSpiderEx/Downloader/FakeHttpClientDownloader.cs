using Larpx.ResourceSpider.DotnetSpiderEx.Proxy;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace Larpx.ResourceSpider.DotnetSpiderEx.Downloader
{
    public class FakeHttpClientDownloader : HttpClientDownloader
    {
        public FakeHttpClientDownloader(IHttpClientFactory httpClientFactory,
            IProxyService proxyService,
            ILogger<HttpClientDownloader> logger) : base(httpClientFactory, proxyService, logger)
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpClient httpClient,
            HttpRequestMessage httpRequestMessage)
        {
            return Task.FromResult(new HttpResponseMessage
            {
                Content = new StringContent("<html></html>", Encoding.UTF8),
                RequestMessage = httpRequestMessage,
                StatusCode = HttpStatusCode.OK,
                Version = HttpVersion.Version11
            });
        }

        public override string Name => UseProxy ? Downloaders.FakeProxyHttpClient : Downloaders.FakeHttpClient;
    }
}
