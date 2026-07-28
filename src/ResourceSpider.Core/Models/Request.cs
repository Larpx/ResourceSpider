using Larpx.PersonalTools.ResourceSpider.Core.Enums;

namespace Larpx.PersonalTools.ResourceSpider.Core.Models;

/// <summary>
/// 请求模型，表示一个待下载的 HTTP 请求
/// </summary>
public class Request
{
    /// <summary>
    /// 请求唯一标识
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 请求 URL 地址
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// HTTP 方法，如 GET、POST
    /// </summary>
    public string Method { get; set; } = "GET";

    /// <summary>
    /// 请求头集合
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// 请求体数据
    /// </summary>
    public byte[]? Body { get; set; }

    /// <summary>
    /// 关联的任务标识
    /// </summary>
    public string? TaskId { get; set; }

    /// <summary>
    /// 请求优先级，数值越小优先级越高
    /// </summary>
    public int Priority { get; set; } = 5;

    /// <summary>
    /// 当前重试次数
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetry { get; set; } = 3;

    /// <summary>
    /// 请求当前状态
    /// </summary>
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    /// <summary>
    /// 请求创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 请求指纹，用于去重判断
    /// </summary>
    public string? Fingerprint { get; set; }

    /// <summary>
    /// 请求元数据，存储与请求相关的附加信息
    /// </summary>
    public Dictionary<string, object?> Metadata { get; set; } = new();
}
