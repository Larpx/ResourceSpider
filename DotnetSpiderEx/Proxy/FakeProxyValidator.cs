namespace Larpx.ResourceSpider.DotnetSpiderEx.Proxy
{
    public class FakeProxyValidator : IProxyValidator
    {
        public Task<bool> IsAvailable(Uri proxy)
        {
            return Task.FromResult(true);
        }
    }
}
