using System;

namespace Larpx.ResourceSpider.BaseLibrary.OpenAPI.Weixin
{
    /// <summary>
    /// 匹配的关键词
    /// </summary>
    public sealed class AutoReplyKeyword : AutoReplyContent
    {
        /// <summary>
        /// 匹配模式
        /// </summary>
        public AutoReplyMatchMode match_mode;
    }
}
