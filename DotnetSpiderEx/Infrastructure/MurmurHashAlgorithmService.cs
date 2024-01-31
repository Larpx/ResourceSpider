using Murmur;
using System.Security.Cryptography;

namespace Larpx.ResourceSpider.DotnetSpiderEx.Infrastructure
{
    public class MurmurHashAlgorithmService : HashAlgorithmService
    {
        private readonly HashAlgorithm _hashAlgorithm;

        public MurmurHashAlgorithmService()
        {
            _hashAlgorithm = MurmurHash.Create32();
        }

        protected override HashAlgorithm GetHashAlgorithm()
        {
            return _hashAlgorithm;
        }
    }
}
