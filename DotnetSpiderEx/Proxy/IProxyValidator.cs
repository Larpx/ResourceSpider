namespace Larpx.ResourceSpider.DotnetSpiderEx.Proxy
{
    public interface IProxyValidator
    {
        Task<bool> IsAvailable(Uri proxy);
    }
}
