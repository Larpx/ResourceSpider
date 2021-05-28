using System.Collections.Generic;

namespace Larpx.ResourceSpider.ABotEx.Poco
{
    public class CrawlConfiguration 
    {
        public CrawlConfiguration()
        {
            MaxConcurrentThreads = 10;
            UserAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";
            RobotsDotTextUserAgentString = "abot";
            MaxPagesToCrawl = 1000;
            DownloadableContentTypes = "text/html";
            ConfigurationExtensions = new Dictionary<string, string>();
            MaxRobotsDotTextCrawlDelayInSeconds = 5;
            HttpRequestMaxAutoRedirects = 7;
            IsHttpRequestAutoRedirectsEnabled = true;
            MaxCrawlDepth = 100;
            HttpServicePointConnectionLimit = 200;
            HttpRequestTimeoutInSeconds = 15;
            IsSslCertificateValidationEnabled = false;
        }

        #region crawlBehavior

        /// <summary>
        /// 用于HTTP请求的最大并发线程
        /// </summary>
        public int MaxConcurrentThreads { get; set; }

        /// <summary>
        /// 要爬网的最大页数。如果为零，则此设置无效
        /// </summary>
        public int MaxPagesToCrawl { get; set; }

        /// <summary>
        /// 每个域要爬网的最大页数
        /// 如果为零，则此设置无效
        /// </summary>
        public int MaxPagesToCrawlPerDomain { get; set; }

        /// <summary>
        /// 页面的最大大小。如果页面大小大于此值，则不会下载或处理
        /// 如果为零，则此设置无效
        /// </summary>
        public int MaxPageSizeInBytes { get; set; }

        /// <summary>
        /// UA
        /// </summary>
        public string UserAgentString { get; set; }

        /// <summary>
        /// HTTP协议版本号在HTTP请求期间使用。目前支持值“1.1”和“1.0”。
        /// </summary>
        public HttpProtocolVersion HttpProtocolVersion { get; set; }

        /// <summary>
        /// 采集超时和停止前的最大秒数。
        /// 如果为零，则此设置无效
        /// </summary>
        public int CrawlTimeoutSeconds { get; set; }

        /// <summary>
        /// 可通过管道访问的键值对字典
        /// </summary>
        public Dictionary<string, string> ConfigurationExtensions { get; set; }

        /// <summary>
        /// 是否应该多次爬行uri。这并不常见，在大多数情况下应该是False
        /// </summary>
        public bool IsUriRecrawlingEnabled { get; set; }

        /// <summary>
        /// 是否应该抓取根uri外部的页面
        /// </summary>
        public bool IsExternalPageCrawlingEnabled { get; set; }

        /// <summary>
        /// 根uri外部的页面是否应该对其链接进行采集。注意：IsExternalPageCrawlEnabled必须为true，此设置才能生效
        /// </summary>
        public bool IsExternalPageLinksCrawlingEnabled { get; set; }

        /// <summary>
        /// 命名锚或散列标是否被视为URL的一部分。
        /// 如果为假，它们将被忽略。如果为真，它们将被视为url的一部分
        /// </summary>
        public bool IsRespectUrlNamedAnchorOrHashbangEnabled { get; set; }

        /// <summary>
        /// 一个逗号分隔的字符串，
        /// 它的内容类型应该有其页面内容被下载。对于每个页面，将检查内容类型，以查看它是否包含这里定义的任何值。
        /// </summary>
        public string DownloadableContentTypes { get; set; }

        /// <summary>
        /// 获取或设置System.Net.ServicePoint允许的最大并发连接数。系统默认值为2。这意味着同一个主机只能打开两个并发的http连接。
        /// 如果为零，则此设置无效
        /// </summary>
        public int HttpServicePointConnectionLimit { get; set; }

        /// <summary>
        ///获取或设置System.Net.HttpWebRequest.GetResponse()和System.Net.HttpWebRequest.GetRequestStream()方法的超时值，以秒为单位。
        /// 如果为零，则此设置无效
        /// </summary>
        public int HttpRequestTimeoutInSeconds { get; set; }

        /// <summary>
        /// 获取或设置请求遵循的最大重定向数。
        /// 如果为零，则此设置无效
        /// </summary>
        public int HttpRequestMaxAutoRedirects { get; set; }

        /// <summary>
        ///获取或设置一个值，该值指示请求是否应遵循重定向
        /// </summary>
        public bool IsHttpRequestAutoRedirectsEnabled { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值指示gzip、Brotli和deflate将被自动接受和解压缩
        /// </summary>
        public bool IsHttpRequestAutomaticDecompressionEnabled { get; set; }

        /// <summary>
        /// 是否应设置cookie并随每个请求一起重新发送
        /// </summary>
        public bool IsSendingCookiesEnabled { get; set; }

        /// <summary>
        /// 是否验证服务器SSL证书。如果为true，将进行默认验证。
        /// 如果为false，则绕过证书验证。此设置对于抓取SSL证书无效或过期的站点非常有用
        /// </summary>
        public bool IsSslCertificateValidationEnabled { get; set; }

        /// <summary>
        /// 使用16的倍数集。如果在开始采集之前没有这么多可用内存，则抛出InsufficientMemoryException。
        /// 如果为零，则此设置无效
        /// </summary>
        /// <exception cref="http://msdn.microsoft.com/en-us/library/system.insufficientmemoryexception.aspx">InsufficientMemoryException</exception>
        public int MinAvailableMemoryRequiredInMb { get; set; }

        /// <summary>
        /// 允许进程使用的最大内存量。如果超过这个限制，爬虫将提前停止。
        /// 如果为零，则此设置无效
        /// </summary>
        public int MaxMemoryUsageInMb { get; set; }

        /// <summary>
        /// 刷新用于确定承载爬虫实例的进程正在使用的内存量的值之前的最长时间。
        /// 如果MaxMemoryUsageInMb为零，则此值无效。
        /// </summary>
        public int MaxMemoryUsageCacheTimeInSeconds { get; set; }

        /// <summary>
        /// 在根页面下爬行的最大级别。
        /// 如果为0，则会对主页进行抓取，但不会对其链接进行抓取。
        /// 如果为1，则对主页及其链接进行抓取，但对所有链接都不进行抓取。
        /// </summary>
        public int MaxCrawlDepth { get; set; }

        /// <summary>
        /// 每页爬网的最大链接。
        /// 如果值为零，则此设置无效。
        /// </summary>
        public int MaxLinksPerPage { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the crawler should parse the page's links even if a CrawlDecision (like CrawlDecisionMaker.ShouldCrawlPageLinks()) determines that those links will not be crawled.
        /// </summary>
        public bool IsForcedLinkParsingEnabled { get; set; }

        /// <summary>
        /// The max number of retries for a url if a web exception is encountered. If the value is 0, no retries will be made
        /// </summary>
        public int MaxRetryCount { get; set; }

        /// <summary>
        /// The minimum delay between a failed http request and the next retry
        /// </summary>
        public int MinRetryDelayInMilliseconds { get; set; }

        #endregion

        #region politeness

        /// <summary>
        /// Whether the crawler should retrieve and respect the robots.txt file.
        /// </summary>
        public bool IsRespectRobotsDotTextEnabled { get; set; }

        /// <summary>
        /// Whether the crawler should ignore links on pages that have a <meta name="robots" content="nofollow" /> tag
        /// </summary>
        public bool IsRespectMetaRobotsNoFollowEnabled { get; set; }

        /// <summary>
        /// Whether the crawler should ignore links on pages that have an http X-Robots-Tag header of nofollow
        /// </summary>
        public bool IsRespectHttpXRobotsTagHeaderNoFollowEnabled { get; set; }

        /// <summary>
        /// Whether the crawler should ignore links that have a <a href="whatever" rel="nofollow" />...
        /// </summary>
        public bool IsRespectAnchorRelNoFollowEnabled { get; set; }

        /// <summary>
        /// If true, will ignore the robots.txt file if it disallows crawling the root uri.
        /// </summary>
        public bool IsIgnoreRobotsDotTextIfRootDisallowedEnabled { get; set; }

        /// <summary>
        /// The user agent string to use when checking robots.txt file for specific directives.  Some examples of other crawler's user agent values are "googlebot", "slurp" etc...
        /// </summary>
        public string RobotsDotTextUserAgentString { get; set; }

        /// <summary>
        /// The number of milliseconds to wait in between http requests to the same domain.
        /// </summary>
        public int MinCrawlDelayPerDomainMilliSeconds { get; set; }

        /// <summary>
        /// The maximum numer of seconds to respect in the robots.txt "Crawl-delay: X" directive. 
        /// IsRespectRobotsDotTextEnabled must be true for this value to be used.
        /// If zero, will use whatever the robots.txt crawl delay requests no matter how high the value is.
        /// </summary>
        public int MaxRobotsDotTextCrawlDelayInSeconds { get; set; }

        #endregion

        #region Authorization

        /// <summary>
        /// Defines whether each request should be authorized via login
        /// </summary>
        public bool IsAlwaysLogin { get; set; }
        /// <summary>
        /// The user name to be used for authorization
        /// </summary>
        public string LoginUser { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LoginPassword { get; set; }

        /// <summary>
        /// 是否使用默认凭据。
        /// </summary>
        public bool UseDefaultCredentials { get; set; }

        #endregion
    }
}
