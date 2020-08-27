using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace Larpx.ResourceSpider.Http.Downloader
{
    public class DownloaderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, IDownloader> _dict;

        public DownloaderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _dict = new ConcurrentDictionary<string, IDownloader>();
        }

        /// <summary>
        /// 创建下载器
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IDownloader Create(string type)
        {
            var downloader = _dict.GetOrAdd(type, t =>
            {
                var downloaderList = _serviceProvider.GetServices(typeof(IDownloader));
                if (downloaderList != null)
                {
                    foreach (var x in downloaderList)
                    {
                        if (x.GetType().Name.StartsWith(type))
                        {
                            return (IDownloader)x;
                        }
                    }
                }

                return null;
            });

            return downloader;
        }
    }
}
