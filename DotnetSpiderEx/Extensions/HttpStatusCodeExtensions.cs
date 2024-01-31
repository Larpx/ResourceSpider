using System.Net;

namespace Larpx.ResourceSpider.DotnetSpiderEx.Extensions
{
    public static class HttpStatusCodeExtensions
    {
        public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
        {
            return statusCode >= HttpStatusCode.OK && statusCode <= (HttpStatusCode)299;
        }
    }
}