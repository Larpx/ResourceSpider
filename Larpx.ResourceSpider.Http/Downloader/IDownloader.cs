using System.Threading.Tasks;

namespace Larpx.ResourceSpider.Http.Downloader
{
    public interface IDownloader
    {
        Task<Response> DownloadResponseAsync(Request request);

        Task<string> DownloadStringAsync(Request request);
    }
}
