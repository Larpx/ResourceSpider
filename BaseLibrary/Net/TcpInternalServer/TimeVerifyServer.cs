using System;

namespace Larpx.ResourceSpider.BaseLibrary.Net.TcpInternalServer
{
    /// <summary>
    /// 时间验证服务
    /// </summary>
    public abstract class TimeVerifyServer : TcpServer.TimeVerifyServer<Server, ServerSocket, ServerSocketSender>
    {
    }
}
