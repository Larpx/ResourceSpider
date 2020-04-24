using System;
using System.Runtime.InteropServices;

namespace Larpx.ResourceSpider.BaseLibrary.OpenAPI.Weixin
{
    /// <summary>
    /// 累计用户数据
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct UserCumulate
    {
        /// <summary>
        /// 数据的日期
        /// </summary>
        public string ref_date;
        /// <summary>
        /// 总用户量
        /// </summary>
        public long cumulate_user;
    }
}
