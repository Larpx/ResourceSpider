using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.Interfaces;

public interface IProxyPool
{
    Task<Proxy?> GetProxyAsync(CancellationToken ct = default);
    
    Task AddProxyAsync(Proxy proxy, CancellationToken ct = default);
    
    Task RemoveProxyAsync(string proxyId, CancellationToken ct = default);
    
    Task MarkSuccessAsync(string proxyId, CancellationToken ct = default);
    
    Task MarkFailureAsync(string proxyId, CancellationToken ct = default);
    
    Task<IEnumerable<Proxy>> GetAllProxiesAsync(CancellationToken ct = default);
}
