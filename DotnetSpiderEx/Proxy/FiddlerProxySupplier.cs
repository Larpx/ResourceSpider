using Microsoft.Extensions.Options;

namespace Larpx.ResourceSpider.DotnetSpiderEx.Proxy
{
    public class FiddlerProxySupplier : IProxySupplier
    {
        private Uri[] _proxies;

        public FiddlerProxySupplier(IOptions<ProxyOptions> options)
        {
            _proxies = new Uri[] { new(options.Value.ProxyTestUrl) };
        }

        public Task<IEnumerable<Uri>> GetProxiesAsync()
        {
            if (_proxies.Length <= 0)
            {
                return Task.FromResult(Enumerable.Empty<Uri>());
            }

            var result = _proxies.Clone() as IEnumerable<Uri>;
            _proxies = new Uri[0];
            return Task.FromResult(result);
        }
    }
}
