using Larpx.ResourceSpider.DotnetSpiderEx.Http;

namespace Larpx.ResourceSpider.DotnetSpiderEx.Downloader
{
    /// <summary>
    /// 通用下载器接口
    /// </summary>
    public interface IDownloader
    {
        Task<Response> DownloadAsync(Request request);

        /// <summary>
        /// 下载器ID
        /// </summary>
        string Name { get; }
    }
}
