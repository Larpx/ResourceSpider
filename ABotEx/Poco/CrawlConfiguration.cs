using System.Collections.Generic;

namespace Larpx.ResourceSpider.ABotEx.Poco
{
    /// <summary>
    /// 采集程序配置
    /// </summary>
    public class CrawlConfiguration
    {
        /// <summary>
        /// 采集程序配置
        /// </summary>
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

        #region crawlBehavior(采集行为)

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
        /// 获取或设置一个值，该值指示爬网程序是否应分析页的链接，
        /// 即使爬网决策（如CrawlDecisionMaker.shouldScrawlPageLinks（））确定这些链接不会被爬网。
        /// </summary>
        public bool IsForcedLinkParsingEnabled { get; set; }

        /// <summary>
        /// 遇到web异常时url的最大重试次数。如果值为0，则不会重试
        /// </summary>
        public int MaxRetryCount { get; set; }

        /// <summary>
        /// 失败的http请求与下一次重试之间的最小延迟
        /// </summary>
        public int MinRetryDelayInMilliseconds { get; set; }

        #endregion

        #region politeness(礼貌行为)

        /// <summary>
        /// 是否应该检索并尊重robots.txt文件。
        /// </summary>
        public bool IsRespectRobotsDotTextEnabled { get; set; }

        /// <summary>
        /// 爬虫程序是否应该忽略带有 <meta name="robots" content="nofollow"/> 标记的页面上的链接
        /// </summary>
        public bool IsRespectMetaRobotsNoFollowEnabled { get; set; }

        /// <summary>
        /// 爬虫程序是否应该忽略具有nofollow的httpx-Robots-Tag头的页面上的链接
        /// </summary>
        public bool IsRespectHttpXRobotsTagHeaderNoFollowEnabled { get; set; }

        /// <summary>
        /// 爬虫程序是否应该忽略具有 <a href="whatever" rel="nofollow" />的链接
        /// </summary>
        public bool IsRespectAnchorRelNoFollowEnabled { get; set; }

        /// <summary>
        ///如果为true，则将忽略robots.txt文件（如果它不允许爬网根uri）。
        /// </summary>
        public bool IsIgnoreRobotsDotTextIfRootDisallowedEnabled { get; set; }

        /// <summary>
        /// 检查robots.txt文件中的特定指令时要使用的用户代理字符串。其他爬虫的用户代理值的一些例子有“googlebot”、“slurp”等。。。
        /// </summary>
        public string RobotsDotTextUserAgentString { get; set; }

        /// <summary>
        /// 对同一域的http请求之间等待的毫秒数。
        /// </summary>
        public int MinCrawlDelayPerDomainMilliSeconds { get; set; }

        /// <summary>
        /// robots.txt“Crawl delay:X”指令中需要遵守的最大秒数。
        /// IsRespectRobotsDotTextEnabled必须为true才能使用此值。
        /// 如果为零，将使用robots.txt爬网延迟请求的任何值，无论该值有多高。
        /// </summary>
        public int MaxRobotsDotTextCrawlDelayInSeconds { get; set; }

        #endregion

        #region Authorization(授权)

        /// <summary>
        /// 定义是否应通过登录来授权每个请求
        /// </summary>
        public bool IsAlwaysLogin { get; set; }

        /// <summary>
        /// 授权用户名
        /// </summary>
        public string LoginUser { get; set; }

        /// <summary>
        /// 授权用户密码
        /// </summary>
        public string LoginPassword { get; set; }

        /// <summary>
        /// 是否使用默认凭据。
        /// </summary>
        public bool UseDefaultCredentials { get; set; }

        #endregion
    }
}
