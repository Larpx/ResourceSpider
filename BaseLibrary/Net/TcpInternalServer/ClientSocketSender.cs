using System;

namespace Larpx.ResourceSpider.BaseLibrary.Net.TcpInternalServer
{
    /// <summary>
    /// TCP 内部服务客户端套接字数据发送
    /// </summary>
    public sealed class ClientSocketSender : TcpServer.ClientSocketSender
    {
#if !NOJIT
        /// <summary>
        /// TCP 服务客户端套接字数据发送
        /// </summary>
        internal ClientSocketSender() : base() { }
#endif
        /// <summary>
        /// TCP 服务客户端套接字数据发送
        /// </summary>
        /// <param name="socket">TCP 服务客户端套接字</param>
        internal ClientSocketSender(ClientSocket socket)
            : base(socket)
        {
            Threading.ThreadPool.TinyBackground.FastStart(this, Threading.Thread.CallType.TcpInternalClientSocketSenderBuildOutput);
            //BuildOutputMainWaitHandle.Set(0);
            //BuildOutputOtherWaitHandle.Set(0);
            //SendLock = new object();
            //Threading.ThreadPool.TinyBackground.FastStart((Action)BuildOutputMain, Threading.Thread.CallType.Action);
            //Threading.ThreadPool.TinyBackground.FastStart((Action)BuildOutputOther, Threading.Thread.CallType.Action);
        }
    }
}
