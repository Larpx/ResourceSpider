using System;
using Net.TcpServer;

namespace Larpx.ResourceSpider.BaseLibrary.Net.TcpStaticServer
{
    /// <summary>
    /// TCP 调用函数配置
    /// </summary>
    public sealed class SerializeBoxMethodAttribute : MethodAttribute
    {
        /// <summary>
        /// 参数设置
        /// </summary>
        internal override ParameterFlags GetParameterFlags
        {
            get { return ParameterFlags.SerializeBox; }
        }
    }
}
