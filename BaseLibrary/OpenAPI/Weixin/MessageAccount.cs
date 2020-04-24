using System;
using System.Runtime.InteropServices;

namespace Larpx.ResourceSpider.BaseLibrary.OpenAPI.Weixin
{
    /// <summary>
    /// 指定客服帐号
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct MessageAccount
    {
        /// <summary>
        /// 
        /// </summary>
        public string kf_account;
    }
}
