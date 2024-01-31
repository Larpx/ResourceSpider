namespace Larpx.ResourceSpider.DotnetSpiderEx.Proxy
{
    public interface IProxySupplier
    {
        Task<IEnumerable<Uri>> GetProxiesAsync();
    }
}
