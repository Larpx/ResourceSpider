using ResourceSpider.Core.Interfaces;
using ResourceSpider.Infrastructure.Downloader;

namespace ResourceSpider.Infrastructure.Downloader;

public class DefaultDownloaderFactory : IDownloaderFactory
{
    private readonly HttpClientDownloader _httpClientDownloader;
    private readonly PlaywrightDownloader _playwrightDownloader;

    public DefaultDownloaderFactory(
        HttpClientDownloader httpClientDownloader,
        PlaywrightDownloader playwrightDownloader)
    {
        _httpClientDownloader = httpClientDownloader;
        _playwrightDownloader = playwrightDownloader;
    }

    public IDownloader CreateDownloader(DownloadType type)
    {
        return type switch
        {
            DownloadType.HttpClient => _httpClientDownloader,
            DownloadType.Playwright => _playwrightDownloader,
            _ => throw new ArgumentException($"Unsupported download type: {type}")
        };
    }
}
