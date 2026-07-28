using Microsoft.Extensions.DependencyInjection;
using Larpx.PersonalTools.ResourceSpider.Core.Interfaces;
using Larpx.PersonalTools.ResourceSpider.Infrastructure.Downloader;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Downloader;

/// <summary>
/// 默认下载器工厂实现，根据下载类型从依赖注入容器中获取对应的下载器实例
/// </summary>
public class DefaultDownloaderFactory : IDownloaderFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 初始化下载器工厂
    /// </summary>
    /// <param name="serviceProvider">服务提供者，用于获取注册的下载器实例</param>
    public DefaultDownloaderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 根据下载类型创建对应的下载器实例
    /// </summary>
    /// <param name="type">下载器类型</param>
    /// <returns>下载器实例</returns>
    /// <exception cref="ArgumentException">当传入不支持的下载类型时抛出</exception>
    public IDownloader CreateDownloader(DownloadType type)
    {
        return type switch
        {
            DownloadType.HttpClient => _serviceProvider.GetRequiredService<HttpClientDownloader>(),
            DownloadType.Playwright => _serviceProvider.GetRequiredService<PlaywrightDownloader>(),
            DownloadType.Cdp => _serviceProvider.GetRequiredService<CdpDownloader>(),
            _ => throw new ArgumentException($"Unsupported download type: {type}")
        };
    }
}
