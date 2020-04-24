using System;
using System.Runtime.CompilerServices;

namespace Larpx.ResourceSpider.BaseLibrary.Net.RemoteExpression
{
    /// <summary>
    /// 客户端远程表达式节点
    /// </summary>
    [BinarySerialize.Serialize(IsReferenceMember = false, IsMemberMap = false)]
    public struct ClientNode
    {
        /// <summary>
        /// 远程表达式命令信息
        /// </summary>
        internal static readonly TcpServer.CommandInfo CommandInfo = new TcpServer.CommandInfo { Command = TcpServer.Server.RemoteExpressionCommandIndex, InputParameterIndex = -TcpServer.Server.RemoteExpressionCommandIndex };
        /// <summary>
        /// 远程表达式节点
        /// </summary>
        internal Node Node;
        /// <summary>
        /// 客户端检测服务端映射标识
        /// </summary>
        internal ServerNodeIdChecker Checker;
        /// <summary>
        /// 客户端映射标识
        /// </summary>
        internal int ClientNodeId;
        /// <summary>
        /// 清除数据
        /// </summary>
        
        internal void SetNull()
        {
            Node = null;
            Checker = null;
        }
        /// <summary>
        /// 服务端获取返回值
        /// </summary>
        
        public ReturnValue GetReturnValue()
        {
            return Node.Get(ClientNodeId);
        }
        /// <summary>
        /// 客户端序列化
        /// </summary>
        /// <param name="serializer"></param>
        [BinarySerialize.SerializeCustom]
        
        private void serialize(BinarySerialize.Serializer serializer)
        {
            serializer.Stream.Write(ClientNodeId);
            Node.Serialize(serializer, Checker);
        }
        /// <summary>
        /// 服务端反序列化
        /// </summary>
        /// <param name="deSerializer"></param>
        [BinarySerialize.SerializeCustom]
        
        private void deSerialize(BinarySerialize.DeSerializer deSerializer)
        {
            ClientNodeId = deSerializer.ReadInt();
            Node.DeSerialize(deSerializer, out Node);
        }
        /// <summary>
        /// 客户端序列化
        /// </summary>
        /// <param name="serializer"></param>
        [Json.SerializeCustom]
        
        private void serialize(Json.Serializer serializer)
        {
            serializer.CharStream.Write('[');
            serializer.CallSerialize(ClientNodeId);
            serializer.CharStream.Write(',');
            Node.Serialize(serializer, Checker);
            serializer.CharStream.Write(']');
        }
        /// <summary>
        /// 服务端反序列化
        /// </summary>
        /// <param name="parser"></param>
        [Json.ParseCustom]
        
        private unsafe void deSerialize(Json.Parser parser)
        {
            if (*parser.Current++ == '[')
            {
                parser.CallParse(ref ClientNodeId);
                if (parser.State == Json.ParseState.Success)
                {
                    if (*parser.Current++ == ',')
                    {
                        Node.DeSerialize(parser, ref Node);
                        if (parser.State != Json.ParseState.Success || *parser.Current++ == ']') return;
                    }
                }
                else return;
            }
            parser.ParseState = Json.ParseState.Custom;
        }
    }
    /// <summary>
    /// 客户端远程表达式参数节点
    /// </summary>
    /// <typeparam name="returnType">返回值类型</typeparam>
    [BinarySerialize.Serialize(IsReferenceMember = false, IsMemberMap = false)]
    public struct ClientNode<returnType>
    {
        /// <summary>
        /// 远程表达式节点
        /// </summary>
        internal Node<returnType> Node;
        /// <summary>
        /// 客户端检测服务端映射标识
        /// </summary>
        internal ServerNodeIdChecker Checker;
        /// <summary>
        /// 服务端获取返回值
        /// </summary>
        
        public returnType GetReturnValue()
        {
            return Node.GetValue();
        }
        /// <summary>
        /// 客户端序列化
        /// </summary>
        /// <param name="serializer"></param>
        [BinarySerialize.SerializeCustom]
        
        private void serialize(BinarySerialize.Serializer serializer)
        {
            Node.Serialize(serializer, Checker);
        }
        /// <summary>
        /// 服务端反序列化
        /// </summary>
        /// <param name="deSerializer"></param>
        [BinarySerialize.SerializeCustom]
        
        private void deSerialize(BinarySerialize.DeSerializer deSerializer)
        {
            Node node;
            RemoteExpression.Node.DeSerialize(deSerializer, out node);
            Node = (Node<returnType>)node;
        }
        /// <summary>
        /// 客户端序列化
        /// </summary>
        /// <param name="serializer"></param>
        [Json.SerializeCustom]
        
        private void serialize(Json.Serializer serializer)
        {
            Node.Serialize(serializer, Checker);
        }
        /// <summary>
        /// 服务端反序列化
        /// </summary>
        /// <param name="parser"></param>
        [Json.ParseCustom]
        
        private unsafe void deSerialize(Json.Parser parser)
        {
            Node node = null;
            RemoteExpression.Node.DeSerialize(parser, ref node);
            if (parser.State != Json.ParseState.Success)
            {
                Node = (Node<returnType>)node;
                return;
            }
            parser.ParseState = Json.ParseState.Custom;
        }
    }
}
