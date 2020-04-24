using System;
using System.Runtime.InteropServices;

namespace Larpx.ResourceSpider.BaseLibrary.OpenAPI.Weixin
{
    /// <summary>
    /// 上传图文消息素材
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    internal struct UploadArticle
    {
        /// <summary>
        /// 图文消息素材
        /// </summary>
        public Article[] articles;
    }
}
