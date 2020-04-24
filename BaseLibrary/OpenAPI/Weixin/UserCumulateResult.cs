using System;

namespace Larpx.ResourceSpider.BaseLibrary.OpenAPI.Weixin
{
    /// <summary>
    /// 累计用户数据
    /// </summary>
    internal sealed class UserCumulateResult : Return
    {
#pragma warning disable
        /// <summary>
        /// 累计用户数据
        /// </summary>
        public UserCumulate[] list;
#pragma warning restore
    }
}
