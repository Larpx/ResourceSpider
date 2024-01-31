namespace Larpx.ResourceSpider.DotnetSpiderEx.Infrastructure
{
    public interface IHashAlgorithmService
    {
        byte[] ComputeHash(byte[] bytes);
    }
}
