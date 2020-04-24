using System;
using System.Runtime.InteropServices;

namespace Larpx.ResourceSpider.BaseLibrary.OpenAPI.Weixin
{
    /// <summary>
    /// 二维码详细信息
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    internal struct QrCodeAction
    {
        /// <summary>
        /// 场景值ID
        /// </summary>
        public SceneId scene;
    }
}
