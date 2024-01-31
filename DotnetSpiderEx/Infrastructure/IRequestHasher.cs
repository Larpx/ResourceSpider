using Larpx.ResourceSpider.DotnetSpiderEx.Http;

namespace Larpx.ResourceSpider.DotnetSpiderEx.Infrastructure
{
    /// <summary>
    /// 请求的哈希器计算接口
    /// </summary>
    public interface IRequestHasher
    {
        /// <summary>
        /// 编译Hash
        /// </summary>
        /// <returns></returns>
        void ComputeHash(Request request);
    }
}
