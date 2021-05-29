using System;

namespace Larpx.ResourceSpider.ABotEx.Poco
{
    /// <summary>
    /// 采集结果
    /// </summary>
    public class CrawlResult
    {
        /// <summary>
        /// 采集结果
        /// </summary>
        public CrawlResult()
        {
        }

        /// <summary>
        /// 采集的根URL
        /// </summary>
        public Uri RootUri { get; set; }

        /// <summary>
        /// 采集耗时
        /// </summary>
        public TimeSpan Elapsed { get; set; }

        /// <summary>
        /// 采集过程中是否发生导致其过早结束的错误
        /// </summary>
        public bool ErrorOccurred 
        {
            get
            {
                return (ErrorException != null);
            }
        }

        /// <summary>
        /// 导致爬网过早结束的异常
        /// </summary>
        public Exception ErrorException { get; set; }

        /// <summary>
        /// 爬网的上下文
        /// </summary>
        public CrawlContext CrawlContext { get; set; }
    }
}
