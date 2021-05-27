using Larpx.ResourceSpider.ABotEx.Core;
using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Threading;

namespace Larpx.ResourceSpider.ABotEx.Poco
{
    /// <summary>
    /// 采集的上下文
    /// </summary>
    public class CrawlContext
    {
        /// <summary>
        /// 采集器的上下文
        /// </summary>
        public CrawlContext()
        {
            CrawlCountByDomain = new ConcurrentDictionary<string, int>();
            CrawlBag = new ExpandoObject();
            CancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 采集的根URL
        /// </summary>
        public Uri RootUri { get; set; }

        /// <summary>
        /// 配置中指定的爬网的根。如果根URI被重定向到另一个URI，则将在rooturi中设置。
        /// </summary>
        public Uri OriginalRootUri { get; set; }

        /// <summary>
        /// 已爬网的总页数
        /// </summary>
        public int CrawledCount = 0;

        /// <summary>
        /// 请求上次不成功HTTP状态（非200）的日期时间
        /// </summary>
        public DateTime CrawlStartDate { get; set; }

        /// <summary>
        /// 线程安全的域字典以及在该域中爬行了多少页
        /// </summary>
        public ConcurrentDictionary<string, int> CrawlCountByDomain { get; set; }

        /// <summary>
        /// 用于确定爬网设置的配置值
        /// </summary>
        public CrawlConfiguration CrawlConfiguration { get; set; }

        /// <summary>
        /// 正在使用的调度器
        /// </summary>
        public IScheduler Scheduler { get; set; }

        /// <summary>
        /// 采集的数据值，动态类型
        /// </summary>
        public dynamic CrawlBag { get; set; }

        /// <summary>
        /// 是否发生了停止抓取的请求。将清除所有计划的页面，但将允许当前正在爬行完成的任何线程。
        /// </summary>
        public bool IsCrawlStopRequested { get; set; }

        /// <summary>
        ///是否已发生硬停止抓取的请求。将清除所有计划的页面并取消当前正在爬行的任何线程。
        /// </summary>
        public bool IsCrawlHardStopRequested { get; set; }

        /// <summary>
        /// 采集开始时的内存使用量(以mb为单位)
        /// </summary>
        public int MemoryUsageBeforeCrawlInMb { get; set; }

        /// <summary>
        /// 采集结束时的内存使用量(以mb为单位)
        /// </summary>
        public int MemoryUsageAfterCrawlInMb { get; set; }

        /// <summary>
        /// 用于硬停止抓取的取消令牌。将清除所有计划的页面并中止当前正在爬行的任何线程。
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}
