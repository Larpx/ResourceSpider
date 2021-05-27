using Larpx.ResourceSpider.ABotEx.Poco;
using System;

namespace Larpx.ResourceSpider.ABotEx.Crawler
{
    public class PageCrawlStartingArgs : CrawlArgs
    {
        public PageToCrawl PageToCrawl { get; private set; }

        public PageCrawlStartingArgs(CrawlContext crawlContext, PageToCrawl pageToCrawl)
            : base(crawlContext)
        {
            PageToCrawl = pageToCrawl ?? throw new ArgumentNullException(nameof(pageToCrawl));
        }
    }
}
