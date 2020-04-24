using System;

namespace Larpx.ResourceSpider.BaseLibrary.Net.TcpStaticServer
{
    /// <summary>
    /// 远程调用链中间节点配置
    /// </summary>
    public sealed partial class RemoteLinkAttribute : Metadata.IgnoreMemberAttribute
    {
        /// <summary>
        /// 是否需要可空检查
        /// </summary>
        public bool IsNull = true;
    }
}
