using System;

namespace Larpx.ResourceSpider.BaseLibrary.Net.TcpServer
{
    /// <summary>
    /// await 模拟返回值
    /// </summary>
    /// <typeparam name="valueType"></typeparam>
	public struct AwaiterResult<valueType>
	{
        /// <summary>
        /// 返回值
        /// </summary>
        public valueType Result;
	}
}
