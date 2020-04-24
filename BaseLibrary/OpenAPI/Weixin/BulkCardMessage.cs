using System;
using System.Runtime.InteropServices;

namespace Larpx.ResourceSpider.BaseLibrary.OpenAPI.Weixin
{
    /// <summary>
    /// 卡券消息
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct BulkCardMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public string card_id;
    }
}
