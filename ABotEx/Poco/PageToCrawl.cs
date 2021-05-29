using System;
using System.Dynamic;

namespace Larpx.ResourceSpider.ABotEx.Poco
{
    /// <summary>
    /// 页面采集类
    /// </summary>
    public class PageToCrawl
    {
        /// <summary>
        /// 页面采集类，序列化用
        /// </summary>
        public PageToCrawl()
        {
        }

        /// <summary>
        /// 页面采集类，初始化用
        /// </summary>
        /// <param name="uri"></param>
        public PageToCrawl(Uri uri)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            PageBag = new ExpandoObject();
        }

        /// <summary>
        /// 页面地址
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// 页面父级URL
        /// </summary>
        public Uri ParentUri { get; set; }

        /// <summary>
        /// http请求是否必须重试多次。这可能是由于节流或安全。
        /// </summary>
        public bool IsRetry { get; set; }

        /// <summary>
        /// 服务器发送到重试前等待的时间（以秒为单位）。
        /// </summary>
        public double? RetryAfter { get; set; }

        /// <summary>
        /// 重试HTTP请求的次数。
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// 上次发出http请求的日期时间。除非启用重试，否则将为空。
        /// </summary>
        public DateTime? LastRequest { get; set; }

        /// <summary>
        /// 该页面是否为采集的根uri
        /// </summary>
        public bool IsRoot { get; set; }

        /// <summary>
        /// 该页是否位于采集页面的根uri的内部
        /// </summary>
        public bool IsInternal { get; set; }

        /// <summary>
        /// 从树根深处爬行。如果这个页面是主页，这个值将是0，如果这个页面是在主页上找到的，这个值将是1，依此类推。
        /// </summary>
        public int CrawlDepth { get; set; }

        /// <summary>
        /// 可以存储任何类型的值。用于从事件订阅服务器代码向CrawledPage动态添加自定义值
        /// </summary>
        public dynamic PageBag { get; set; }

        /// <summary>
        /// 从中重定向此页的uri。如果为null，则它不是重定向链的一部分
        /// </summary>
        public CrawledPage RedirectedFrom { get; set; }

        /// <summary>
        /// 重定向链中的位置。第一个重定向是位置1，下一个重定向是位置2，依此类推。
        /// </summary>
        public int RedirectPosition { get; set; }

        public override string ToString()
        {
            return Uri.AbsoluteUri;
        }
    }
}
