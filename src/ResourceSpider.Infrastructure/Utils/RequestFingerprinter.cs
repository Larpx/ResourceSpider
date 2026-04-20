using System.Security.Cryptography;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Utils;

public class RequestFingerprinter
{
    public static string GenerateFingerprint(Request request)
    {
        var input = $"{request.Method}:{request.Url}:{SerializeBody(request.Body)}";
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string SerializeBody(byte[]? body)
    {
        if (body == null || body.Length == 0) return string.Empty;
        return Convert.ToHexString(body);
    }
}
