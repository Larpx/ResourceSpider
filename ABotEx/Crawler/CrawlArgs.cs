using Larpx.ResourceSpider.ABotEx.Poco;
using System;

namespace Larpx.ResourceSpider.ABotEx.Crawler
{
    public class CrawlArgs : EventArgs
    {
        public CrawlContext CrawlContext { get; set; }

        public CrawlArgs(CrawlContext crawlContext)
        {
            CrawlContext = crawlContext ?? throw new ArgumentNullException(nameof(crawlContext));
        }
    }
}
