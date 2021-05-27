using Larpx.ResourceSpider.ABotEx.Poco;
using System.Linq;
using System.Net;

namespace Larpx.ResourceSpider.ABotEx.Core
{
    /// <summary>
    /// 采集策略
    /// 确定应该抓取哪些页面，是否应该下载原始内容，
    /// 以及是否应该抓取页面上的链接
    /// </summary>
    public interface ICrawlDecisionMaker
    {
        /// <summary>
        /// 决定页面是否应该爬行
        /// </summary>
        CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext);

        /// <summary>
        /// 决定页面的链接是否应该爬行
        /// </summary>
        CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext);

        /// <summary>
        /// 决定是否应该下载页面的内容
        /// </summary>
        CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage, CrawlContext crawlContext);

        /// <summary>
        /// 决定是否重新抓取页面
        /// </summary>
        CrawlDecision ShouldRecrawlPage(CrawledPage crawledPage, CrawlContext crawlContext);
    }

    /// <summary>
    /// 采集决策实现
    /// </summary>
    public class CrawlDecisionMaker : ICrawlDecisionMaker
    {
        public virtual CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            if (pageToCrawl == null)
                return new CrawlDecision { Allow = false, Reason = "要采集的页面为空" };

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "要采集的上下文为空" };

            if (pageToCrawl.RedirectedFrom != null && pageToCrawl.RedirectPosition > crawlContext.CrawlConfiguration.HttpRequestMaxAutoRedirects)
                return new CrawlDecision { Allow = false, Reason = string.Format("已达到HttpRequestMaxAutoRedirects限定的最大值 [{0}]", crawlContext.CrawlConfiguration.HttpRequestMaxAutoRedirects) };

            if (pageToCrawl.CrawlDepth > crawlContext.CrawlConfiguration.MaxCrawlDepth)
                return new CrawlDecision { Allow = false, Reason = "采集深度超过最大值" };

            if (!pageToCrawl.Uri.Scheme.StartsWith("http"))
                return new CrawlDecision { Allow = false, Reason = "采集地址不是以Http开头" };

            //TODO Do we want to ignore redirect chains (ie.. do not treat them as separate page crawls)?
            if (!pageToCrawl.IsRetry &&
                crawlContext.CrawlConfiguration.MaxPagesToCrawl > 0 &&
                crawlContext.CrawledCount + crawlContext.Scheduler.Count + 1 > crawlContext.CrawlConfiguration.MaxPagesToCrawl)
            {
                return new CrawlDecision { Allow = false, Reason = string.Format("已到达MaxPagesToCrawl限定的最大值 [{0}]", crawlContext.CrawlConfiguration.MaxPagesToCrawl) };
            }

            var pagesCrawledInThisDomain = 0;
            if (!pageToCrawl.IsRetry &&
                crawlContext.CrawlConfiguration.MaxPagesToCrawlPerDomain > 0 &&
                crawlContext.CrawlCountByDomain.TryGetValue(pageToCrawl.Uri.Authority, out pagesCrawledInThisDomain) &&
                pagesCrawledInThisDomain > 0)
            {
                if (pagesCrawledInThisDomain >= crawlContext.CrawlConfiguration.MaxPagesToCrawlPerDomain)
                    return new CrawlDecision { Allow = false, Reason = string.Format("域名 [{1}] 已到达MaxPagesToCrawlPerDomain限定的最大值 [{0}]", crawlContext.CrawlConfiguration.MaxPagesToCrawlPerDomain, pageToCrawl.Uri.Authority) };
            }

            if (!crawlContext.CrawlConfiguration.IsExternalPageCrawlingEnabled && !pageToCrawl.IsInternal)
                return new CrawlDecision { Allow = false, Reason = "链接是外部的" };

            return new CrawlDecision { Allow = true };
        }

        public virtual CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (crawledPage == null)
                return new CrawlDecision { Allow = false, Reason = "要采集的页面为空" };

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "要采集页的上下文为空" };

            if (string.IsNullOrWhiteSpace(crawledPage.Content.Text))
                return new CrawlDecision { Allow = false, Reason = "采集的页面无内容" };

            if (!crawlContext.CrawlConfiguration.IsExternalPageLinksCrawlingEnabled && !crawledPage.IsInternal)
                return new CrawlDecision { Allow = false, Reason = "链接是外部的" };

            if (crawledPage.CrawlDepth >= crawlContext.CrawlConfiguration.MaxCrawlDepth)
                return new CrawlDecision { Allow = false, Reason = "采集深度超过最大值" };

            return new CrawlDecision { Allow = true };
        }

        public virtual CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (crawledPage == null)
                return new CrawlDecision { Allow = false, Reason = "要采集的页面为空" };

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "要采集页的上下文为空" };

            if (crawledPage.HttpResponseMessage == null)
                return new CrawlDecision { Allow = false, Reason = "HttpWebResponse无返回内容" };

            if (crawledPage.HttpResponseMessage.StatusCode != HttpStatusCode.OK)
                return new CrawlDecision { Allow = false, Reason = "HttpCode不是200" };

            var pageContentType = crawledPage.HttpResponseMessage.Content.Headers.ContentType.ToString().ToLower().Trim();
            var isDownloadable = false;
            var cleanDownloadableContentTypes = crawlContext.CrawlConfiguration.DownloadableContentTypes
                .Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();

            foreach (var downloadableContentType in cleanDownloadableContentTypes)
            {
                if (pageContentType.Contains(downloadableContentType.ToLower().Trim()))
                {
                    isDownloadable = true;
                    break;
                }
            }
            if (!isDownloadable)
                return new CrawlDecision { Allow = false, Reason = "内容类型不是以下任何一种: " + string.Join(",", cleanDownloadableContentTypes) };

            if (crawlContext.CrawlConfiguration.MaxPageSizeInBytes > 0 && crawledPage.HttpResponseMessage.Content.Headers.ContentLength > crawlContext.CrawlConfiguration.MaxPageSizeInBytes)
                return new CrawlDecision { Allow = false, Reason = string.Format("页面大小为 [{0}] 字节，高于最大允许的 [{1}] 字节", crawledPage.HttpResponseMessage.Content.Headers.ContentLength, crawlContext.CrawlConfiguration.MaxPageSizeInBytes) };

            return new CrawlDecision { Allow = true };
        }

        public virtual CrawlDecision ShouldRecrawlPage(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (crawledPage == null)
                return new CrawlDecision { Allow = false, Reason = "要采集的页面为空" };

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "要采集页的上下文为空" };

            if (crawledPage.HttpRequestException == null)
                return new CrawlDecision { Allow = false, Reason = "未发生WebException" };

            if (crawlContext.CrawlConfiguration.MaxRetryCount < 1)
                return new CrawlDecision { Allow = false, Reason = "MaxRetryCount小于1" };

            if (crawledPage.RetryCount >= crawlContext.CrawlConfiguration.MaxRetryCount)
                return new CrawlDecision { Allow = false, Reason = "已达到MaxRetryCount" };

            return new CrawlDecision { Allow = true };
        }
    }
}
