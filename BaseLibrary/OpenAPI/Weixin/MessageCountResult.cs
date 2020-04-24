using System;

namespace Larpx.ResourceSpider.BaseLibrary.OpenAPI.Weixin
{
    /// <summary>
    /// 消息发送概况数据
    /// </summary>
    internal sealed class MessageCountResult : Return
    {
#pragma warning disable
        /// <summary>
        /// 消息发送概况数据
        /// </summary>
        public MessageCount[] list;
#pragma warning restore
    }
}
