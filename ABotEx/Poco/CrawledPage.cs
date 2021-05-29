using System;
using System.Collections.Generic;
using System.Net.Http;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Serilog;

namespace Larpx.ResourceSpider.ABotEx.Poco
{
    /// <summary>
    /// 采集的页面
    /// </summary>
    public class CrawledPage : PageToCrawl
    {
        HtmlParser _angleSharpHtmlParser;

        readonly Lazy<IHtmlDocument> _angleSharpHtmlDocument;

        public CrawledPage(Uri uri)
            : base(uri)
        {
            _angleSharpHtmlDocument = new Lazy<IHtmlDocument>(InitializeAngleSharpHtmlParser);

            Content = new PageContent();
        }

        /// <summary>
        /// 延迟加载的IHtmlDocument(https://github.com/AngleSharp/AngleSharp)
        /// 可用于检索/修改已爬网页面上的html元素的。
        /// </summary>
        public virtual IHtmlDocument AngleSharpHtmlDocument => _angleSharpHtmlDocument.Value;

        /// <summary>
        ///发送到服务器的Web请求。
        /// </summary>
        public HttpRequestMessage HttpRequestMessage { get; set; }

        /// <summary>
        /// 来自服务器的Web响应。
        /// </summary>
        public HttpResponseMessage HttpResponseMessage { get; set; }

        /// <summary>
        /// 请求期间发生的请求异常
        /// </summary>
        public HttpRequestException HttpRequestException { get; set; }

        /// <summary>
        /// 用于向服务器发出请求的HttpClientHandler
        /// </summary>
        public HttpClientHandler HttpClientHandler { get; set; }

        public override string ToString()
        {
            if (HttpResponseMessage == null)
                return Uri.AbsoluteUri;

            return $"{Uri.AbsoluteUri}[{Convert.ToInt32(HttpResponseMessage.StatusCode)}]";
        }

        /// <summary>
        /// 从页面解析的链接。
        /// 仅当“ShouldCrawlPageLinks”规则返回true或IsForcedLinkParsingEnabled配置值设置为true时，
        /// 此值才由WebCrawler.SchedulePageLinks()方法设置。
        /// </summary>
        public IEnumerable<HyperLink> ParsedLinks { get; set; }

        /// <summary>
        /// 页面请求的内容
        /// </summary>
        public PageContent Content { get; set; }

        /// <summary>
        /// http请求开始的日期时间
        /// </summary>
        public DateTime RequestStarted { get; set; }

        /// <summary>
        /// http请求完成的日期时间
        /// </summary>
        public DateTime RequestCompleted { get; set; }

        /// <summary>
        /// 页面内容下载开始的日期时间，
        /// 如果CrawlDecisionMaker不允许下载内容或内联委托ShouldDownloadPageContent，则此值可能为空
        /// </summary>
        public DateTime? DownloadContentStarted { get; set; }

        /// <summary>
        /// 页面内容下载完成的日期时间。
        /// 如果CrawlDecisionMaker不允许下载内容或内联委托ShouldDownloadPageContent，则此值可能为空
        /// </summary>
        public DateTime? DownloadContentCompleted { get; set; }

        /// <summary>
        /// 此页面被重定向到的页面
        /// </summary>
        public PageToCrawl RedirectedTo { get; set; }

        /// <summary>
        /// 从RequestStarted到RequestCompleted所用的时间（毫秒）
        /// </summary>
        public double Elapsed => (RequestCompleted - RequestStarted).TotalMilliseconds;


        private IHtmlDocument InitializeAngleSharpHtmlParser()
        {
            if (_angleSharpHtmlParser == null)
                _angleSharpHtmlParser = new HtmlParser();

            IHtmlDocument document;
            try
            {
                document = _angleSharpHtmlParser.ParseDocument(Content.Text);
            }
            catch (Exception e)
            {
                document = _angleSharpHtmlParser.ParseDocument("");

                Log.Error("加载Url [{0}] 的AngularSharp对象时出错 {@Exception}", Uri, e);
            }

            return document;
        }
    }
}
