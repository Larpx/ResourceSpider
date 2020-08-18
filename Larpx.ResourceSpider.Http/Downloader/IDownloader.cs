using System.Threading.Tasks;

namespace Larpx.ResourceSpider.Http.Downloader
{
    public interface IDownloader
    {
        Task<Response> DownloadAsync(Request request);
    }
}
