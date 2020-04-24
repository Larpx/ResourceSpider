using System;

namespace Larpx.ResourceSpider.BaseLibrary.Net.TcpOpenServer.Emit
{
    /// <summary>
    /// TCP 客户端元数据
    /// </summary>
    internal sealed class ClientMetadata : Net.TcpServer.Emit.ClientMetadata
    {
        /// <summary>
        /// TCP 客户端元数据
        /// </summary>
        private ClientMetadata()
            : base(typeof(TcpOpenServer.Client), typeof(ClientSocketSender), typeof(MethodClient)
                , ((Func<ClientSocketSender>)ParameterGenericType.Client.GetSender).Method, ReturnParameterGenericType.Get
                , ParameterGenericType.Get, ParameterGenericType2.Get
                , ((Net.TcpServer.Emit.ParameterGenericType.WaitCall)ParameterGenericType.ClientSocketSender.WaitCall).Method
                , ((Action<Net.TcpServer.CommandInfo>)ParameterGenericType.ClientSocketSender.CallOnly).Method
                , ((Action<Net.TcpServer.CommandInfo, Action<Net.TcpServer.ReturnValue>>)ParameterGenericType.ClientSocketSender.Call).Method
                , ((Func<Net.TcpServer.CommandInfo, Action<Net.TcpServer.ReturnValue>, Net.TcpServer.KeepCallback>)ParameterGenericType.ClientSocketSender.CallKeep).Method
#if !DOTNET2 && !DOTNET4 && !UNITY3D
                , ((Func<Net.TcpServer.CommandInfo, Net.TcpServer.Awaiter, Net.TcpServer.ReturnType>)ParameterGenericType.ClientSocketSender.GetAwaiter).Method
#endif
                  )
        {
        }
        /// <summary>
        /// TCP 客户端元数据
        /// </summary>
        internal static readonly ClientMetadata Default = new ClientMetadata();
    }
}
