namespace ResourceSpider.Core.Interfaces;

/// <summary>
/// 下载器类型枚举，定义支持的下载方式
/// </summary>
public enum DownloadType
{
    /// <summary>
    /// 基于 HttpClient 的轻量级下载器
    /// </summary>
    HttpClient,

    /// <summary>
    /// 基于 Playwright 的浏览器渲染下载器
    /// </summary>
    Playwright,

    /// <summary>
    /// 基于 CDP（Chrome DevTools Protocol）连接远程浏览器的下载器
    /// </summary>
    Cdp,

    /// <summary>
    /// 自定义下载器
    /// </summary>
    Custom
}

/// <summary>
/// 下载器工厂接口，根据下载类型创建对应的下载器实例
/// </summary>
public interface IDownloaderFactory
{
    /// <summary>
    /// 根据下载类型创建下载器实例
    /// </summary>
    /// <param name="type">下载器类型</param>
    /// <returns>下载器实例</returns>
    IDownloader CreateDownloader(DownloadType type);
}
