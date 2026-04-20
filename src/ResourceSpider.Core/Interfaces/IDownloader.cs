using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.Interfaces;

public interface IDownloader
{
    Task<Response> DownloadAsync(Request request, CancellationToken ct = default);
}
