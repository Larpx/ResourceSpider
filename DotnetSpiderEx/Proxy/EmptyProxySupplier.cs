namespace Larpx.ResourceSpider.DotnetSpiderEx.Proxy
{
    public class EmptyProxySupplier : IProxySupplier
    {
        public Task<IEnumerable<Uri>> GetProxiesAsync()
        {
            return Task.FromResult(Enumerable.Empty<Uri>());
        }
    }
}
