using System;
using System.Runtime.InteropServices;

namespace Larpx.ResourceSpider.BaseLibrary.OpenAPI.Weixin
{
    /// <summary>
    /// 图文消息
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct NewsMessage
    {
        /// <summary>
        /// 图文消息
        /// </summary>
        public ArticleMessage[] articles;
    }
}
