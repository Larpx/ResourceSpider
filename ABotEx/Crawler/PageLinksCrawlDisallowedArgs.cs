using Larpx.ResourceSpider.ABotEx.Poco;
using System;

namespace Larpx.ResourceSpider.ABotEx.Crawler
{
    public class PageLinksCrawlDisallowedArgs : PageCrawlCompletedArgs
    {
        public string DisallowedReason { get; }

        public PageLinksCrawlDisallowedArgs(CrawlContext crawlContext, CrawledPage crawledPage, string disallowedReason)
            : base(crawlContext, crawledPage)
        {
            if (string.IsNullOrWhiteSpace(disallowedReason))
                throw new ArgumentNullException(nameof(disallowedReason));

            DisallowedReason = disallowedReason;
        }
    }
}
