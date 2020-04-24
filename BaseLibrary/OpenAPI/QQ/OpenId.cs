using System;
using System.Runtime.InteropServices;

namespace Larpx.ResourceSpider.BaseLibrary.OpenAPI.QQ
{
    /// <summary>
    /// 用户身份的标识
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    internal struct OpenId
    {
        /// <summary>
        /// 用户身份的标识
        /// </summary>
        public string openid;
        /// <summary>
        /// appid
        /// </summary>
        public string client_id;
    }
}
