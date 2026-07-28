using System.Security.Cryptography;
using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Utils;

/// <summary>
/// 请求指纹生成器，基于请求的 Method、URL 和 Body 生成唯一指纹
/// 用于请求去重判断
/// </summary>
public class RequestFingerprinter
{
    /// <summary>
    /// 根据请求的 Method、URL 和 Body 生成 SHA256 指纹
    /// </summary>
    /// <param name="request">HTTP 请求对象</param>
    /// <returns>小写十六进制格式的指纹字符串</returns>
    public static string GenerateFingerprint(Request request)
    {
        var input = $"{request.Method}:{request.Url}:{SerializeBody(request.Body)}";
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// 序列化请求体为十六进制字符串
    /// </summary>
    /// <param name="body">请求体字节数组</param>
    /// <returns>十六进制字符串，空请求体返回空字符串</returns>
    private static string SerializeBody(byte[]? body)
    {
        if (body == null || body.Length == 0) return string.Empty;
        return Convert.ToHexString(body);
    }
}
