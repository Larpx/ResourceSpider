namespace Larpx.ResourceSpider.DotnetSpiderEx.Http
{
    public interface IHttpContent : IDisposable, ICloneable
    {
        ContentHeaders Headers { get; }
    }
}
