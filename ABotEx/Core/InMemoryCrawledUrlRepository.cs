using System;
using System.Collections.Concurrent;

namespace Larpx.ResourceSpider.ABotEx.Core
{
    /// <summary>
    /// 内存中已经采集的URL库接口
    /// </summary>
    public interface ICrawledUrlRepository : IDisposable
    {
        /// <summary>
        /// 在集合中是否存在
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        bool Contains(Uri uri);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        bool AddIfNew(Uri uri);
    }

    /// <summary>
    /// 内存中已经采集的URL库
    /// </summary>
    public class InMemoryCrawledUrlRepository : ICrawledUrlRepository
    {
        ConcurrentDictionary<string, byte> _urlRepository = new ConcurrentDictionary<string, byte>();

        public bool Contains(Uri uri)
        {
            return _urlRepository.ContainsKey(uri.AbsoluteUri);
        }

        public bool AddIfNew(Uri uri)
        {
            return _urlRepository.TryAdd(uri.AbsoluteUri, 0);
        }

        public virtual void Dispose()
        {
            _urlRepository = null;
        }
    }
}
