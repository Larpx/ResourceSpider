namespace ResourceSpider.Core.Interfaces;

public enum DownloadType
{
    HttpClient,
    Playwright,
    Custom
}

public interface IDownloaderFactory
{
    IDownloader CreateDownloader(DownloadType type);
}
