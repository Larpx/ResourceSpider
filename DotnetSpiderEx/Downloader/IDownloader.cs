using Larpx.ResourceSpider.DotnetSpiderEx.Http;

namespace Larpx.ResourceSpider.DotnetSpiderEx.Downloader
{
    public interface IDownloader
    {
        Task<Response> DownloadAsync(Request request);

        string Name { get; }
    }
}
