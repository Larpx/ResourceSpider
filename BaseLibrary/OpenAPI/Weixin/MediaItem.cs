using System;
using System.Runtime.InteropServices;

namespace Larpx.ResourceSpider.BaseLibrary.OpenAPI.Weixin
{
    /// <summary>
    /// 永久素材
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct MediaItem
    {
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public long update_time;
        /// <summary>
        /// 
        /// </summary>
        public string media_id;
        /// <summary>
        /// 文件名称
        /// </summary>
        public string name;
        /// <summary>
        /// 图片的URL
        /// </summary>
        public string url;
    }
}
