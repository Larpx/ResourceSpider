using Larpx.ResourceSpider.ABotEx.Poco;
using System;

namespace Larpx.ResourceSpider.ABotEx.Crawler
{
    public class PageCrawlCompletedArgs : CrawlArgs
    {
        public CrawledPage CrawledPage { get; private set; }

        public PageCrawlCompletedArgs(CrawlContext crawlContext, CrawledPage crawledPage)
            : base(crawlContext)
        {
            CrawledPage = crawledPage ?? throw new ArgumentNullException(nameof(crawledPage));
        }
    }
}
