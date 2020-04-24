using System;
using System.Net.Sockets;
using Larpx.ResourceSpider.BaseLibrary.Extension;
using System.Runtime.CompilerServices;

namespace Larpx.ResourceSpider.BaseLibrary.Net
{
    /// <summary>
    /// 套接字链表
    /// </summary>
    internal sealed class SocketLink : Threading.Link<SocketLink>
    {
        /// <summary>
        /// 套接字
        /// </summary>
        internal Socket Socket;
        /// <summary>
        /// 释放套接字
        /// </summary>
        
        internal void DisposeSocket()
        {
#if !DotNetStandard
            Socket.Dispose();
#endif
            Socket = null;
        }
        /// <summary>
        /// 释放套接字
        /// </summary>
        /// <returns></returns>
        
        internal SocketLink Cancel()
        {
#if !DotNetStandard
            if (Socket != null) Socket.Dispose();
#endif
            return LinkNext;
        }
        /// <summary>
        /// 创建 TCP 服务端套接字
        /// </summary>
        /// <param name="server"></param>
        /// <param name="serverSocket"></param>
        /// <returns></returns>
        
        internal SocketLink Start(Net.TcpOpenServer.Server server, ref Net.TcpOpenServer.ServerSocket serverSocket)
        {
            serverSocket = new TcpOpenServer.ServerSocket(server, ref Socket);
            serverSocket.Start();
            serverSocket = null;
            return LinkNext;
        }
    }
}
