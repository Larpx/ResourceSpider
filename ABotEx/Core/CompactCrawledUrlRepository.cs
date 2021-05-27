using System;
using System.Collections.Concurrent;

namespace Larpx.ResourceSpider.ABotEx.Core
{
    /// <summary>
    /// 实现URL的数字哈希值而不是URL本身用于查找的实现。
    /// 当爬网URL列表变长时，这应该节省空间。
    /// </summary>
    public class CompactCrawledUrlRepository : ICrawledUrlRepository
    {
        private ConcurrentDictionary<long, byte> _mUrlRepository = new ConcurrentDictionary<long, byte>();

        /// <inheritDoc />
        public bool Contains(Uri uri)
        {
            return _mUrlRepository.ContainsKey(uri.GetHashCode());
        }

        /// <inheritDoc />
        public bool AddIfNew(Uri uri)
        {
            return _mUrlRepository.TryAdd(uri.GetHashCode(), 0);
        }

        /// <inheritDoc />
        public virtual void Dispose()
        {
            _mUrlRepository = null;
        }
    }
}
