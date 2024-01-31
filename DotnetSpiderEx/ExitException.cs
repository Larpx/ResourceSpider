namespace Larpx.ResourceSpider.DotnetSpiderEx
{
    public sealed class ExitException : SpiderException
    {
        public ExitException(string msg) : base(msg)
        {
        }
    }
}
