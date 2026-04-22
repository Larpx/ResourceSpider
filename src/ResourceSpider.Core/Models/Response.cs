using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

/// <summary>
/// 响应模型，表示 HTTP 请求的响应数据
/// </summary>
public class Response
{
    /// <summary>
    /// 关联的请求标识
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// 响应 URL 地址
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// HTTP 状态码
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// 响应头集合
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// 响应体原始字节数据
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 响应内容类型
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// 响应内容长度（字节）
    /// </summary>
    public long ContentLength { get; set; }

    /// <summary>
    /// 请求耗时（毫秒）
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// 请求处理状态
    /// </summary>
    public RequestStatus Status { get; set; }

    /// <summary>
    /// 错误信息，请求失败时记录错误详情
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 错误类型分类
    /// </summary>
    public ErrorType? ErrorType { get; set; }

    /// <summary>
    /// 响应接收时间
    /// </summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 响应体文本内容，将字节数据按 UTF-8 编码解码
    /// </summary>
    public string? TextContent => Content.Length > 0
        ? System.Text.Encoding.UTF8.GetString(Content)
        : null;
}
