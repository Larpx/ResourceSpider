using System.Net;

namespace Larpx.ResourceSpider.DotnetSpiderEx.Proxy
{
    public interface IProxyService
    {
        Task<Uri> GetAsync(int seconds);
        Uri Get();
        Task ReturnAsync(Uri proxy, HttpStatusCode statusCode);
        Task<int> AddAsync(IEnumerable<Uri> proxies);
    }
}
