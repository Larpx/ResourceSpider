using System;
using System.Runtime.CompilerServices;

namespace Larpx.ResourceSpider.BaseLibrary.Net.TcpInternalServer
{
    /// <summary>
    /// 获取远程表达式服务端节点标识同步调用
    /// </summary>
    internal sealed class GetRemoteExpressionServerCall : Net.TcpStaticServer.ServerCall<GetRemoteExpressionServerCall, RemoteExpression.ClientNode>
    {
        /// <summary>
        /// 调用处理
        /// </summary>
        public override void RunTask()
        {
            if (Sender.IsSocket)
            {
                Net.TcpServer.ReturnValue<RemoteExpression.ReturnValue.Output> value = new Net.TcpServer.ReturnValue<RemoteExpression.ReturnValue.Output>();
                try
                {
                    value.Value.Return = inputParameter.GetReturnValue();
                    value.Type = Net.TcpServer.ReturnType.Success;
                }
                catch (Exception error)
                {
                    value.Type = Net.TcpServer.ReturnType.ServerException;
                    Sender.AddLog(error);
                }
                Sender.Push(CommandIndex, Sender.IsBuildOutputThread ? RemoteExpression.ReturnValue.Output.OutputThreadInfo : RemoteExpression.ReturnValue.Output.OutputInfo, ref value);
            }
            push(this);
        }
    }
}
