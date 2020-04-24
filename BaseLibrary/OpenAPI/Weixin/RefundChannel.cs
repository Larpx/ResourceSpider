using System;

namespace Larpx.ResourceSpider.BaseLibrary.OpenAPI.Weixin
{
    /// <summary>
    /// 退款渠道
    /// </summary>
    public enum RefundChannel : byte
    {
        /// <summary>
        /// 原路退款
        /// </summary>
        ORIGINAL,
        /// <summary>
        /// 退回到余额
        /// </summary>
        BALANCE
    }
}
