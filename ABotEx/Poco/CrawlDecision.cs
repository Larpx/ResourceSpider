
namespace Larpx.ResourceSpider.ABotEx.Poco
{
    /// <summary>
    /// 采集决定
    /// </summary>
    public class CrawlDecision
    {
        /// <summary>
        /// 采集决定
        /// </summary>
        public CrawlDecision()
        {
            Reason = "";
        }

        /// <summary>
        ///是否允许采集决定
        /// </summary>
        public bool Allow { get; set; }

        /// <summary>
        /// 不允许采集的原因
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// 是否应停止爬网。将清除所有计划的页面，但将允许当前正在爬网的所有线程完成。
        /// </summary>
        public bool ShouldStopCrawl { get; set; }

        /// <summary>
        /// 爬行是否应该“硬停止”。将清除所有计划的页面并取消当前正在爬行的任何线程。
        /// </summary>
        public bool ShouldHardStopCrawl { get; set; }
    }
}
