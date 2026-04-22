using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.Interfaces;

/// <summary>
/// 下载器接口，定义页面内容下载的通用契约
/// </summary>
public interface IDownloader
{
    /// <summary>
    /// 异步下载指定请求的页面内容
    /// </summary>
    /// <param name="request">下载请求对象</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>下载响应对象</returns>
    Task<Response> DownloadAsync(Request request, CancellationToken ct = default);
}
