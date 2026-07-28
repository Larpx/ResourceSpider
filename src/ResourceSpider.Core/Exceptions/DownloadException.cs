namespace Larpx.PersonalTools.ResourceSpider.Core.Exceptions;

/// <summary>
/// 下载异常，在请求下载过程中发生错误时抛出
/// </summary>
public class DownloadException : SpiderException
{
    /// <summary>
    /// 发生下载异常的 URL 地址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 初始化下载异常
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="url">发生异常的 URL 地址</param>
    public DownloadException(string message, string? url = null)
        : base(message)
    {
        Url = url;
    }

    /// <summary>
    /// 初始化下载异常，包含内部异常
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="url">发生异常的 URL 地址</param>
    /// <param name="innerException">导致当前异常的内部异常</param>
    public DownloadException(string message, string? url, Exception innerException)
        : base(message, innerException)
    {
        Url = url;
    }
}
