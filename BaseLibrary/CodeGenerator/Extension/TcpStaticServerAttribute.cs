using System;
using Metadata;
using System.Runtime.CompilerServices;

namespace Larpx.ResourceSpider.BaseLibrary.Extension
{
    /// <summary>
    /// TCP 静态服务配置相关操作
    /// </summary>
    public static class TcpStaticServerAttribute
    {
        /// <summary>
        /// 复制 TCP 服务配置成员位图
        /// </summary>
#if DOTNET2
        private static readonly MemberMap<Net.TcpStaticServer.ServerAttribute> copyMemberMap = new MemberMap<Net.TcpStaticServer.ServerAttribute>.Builder(MemberMap<Net.TcpStaticServer.ServerAttribute>.NewFull()).Clear("Name").Clear("IsAttribute").Clear("IsBaseTypeAttribute");
#else
        private static readonly MemberMap<Net.TcpStaticServer.ServerAttribute> copyMemberMap = new MemberMap<Net.TcpStaticServer.ServerAttribute>.Builder(MemberMap<Net.TcpStaticServer.ServerAttribute>.NewFull()).Clear(value => value.Name).Clear(value => value.IsAttribute).Clear(value => value.IsBaseTypeAttribute);
#endif
        /// <summary>
        /// 复制 TCP 服务配置
        /// </summary>
        /// <param name="value">TCP 服务配置</param>
        /// <param name="copyValue">TCP 服务配置</param>
        
        internal static void CopyFrom(this Net.TcpStaticServer.ServerAttribute value, Net.TcpStaticServer.ServerAttribute copyValue)
        {
            MemberCopy.Copyer<Net.TcpStaticServer.ServerAttribute>.Copy(value, copyValue, copyMemberMap);
        }

        /// <summary>
        /// 复制 TCP 服务配置成员位图
        /// </summary>
#if DOTNET2
        private static readonly MemberMap<Net.TcpStaticSimpleServer.ServerAttribute> simpleCopyMemberMap = new MemberMap<Net.TcpStaticSimpleServer.ServerAttribute>.Builder(MemberMap<Net.TcpStaticSimpleServer.ServerAttribute>.NewFull()).Clear("Name").Clear("IsAttribute").Clear("IsBaseTypeAttribute");
#else
        private static readonly MemberMap<Net.TcpStaticSimpleServer.ServerAttribute> simpleCopyMemberMap = new MemberMap<Net.TcpStaticSimpleServer.ServerAttribute>.Builder(MemberMap<Net.TcpStaticSimpleServer.ServerAttribute>.NewFull()).Clear(value => value.Name).Clear(value => value.IsAttribute).Clear(value => value.IsBaseTypeAttribute);
#endif
        /// <summary>
        /// 复制 TCP 服务配置
        /// </summary>
        /// <param name="value">TCP 服务配置</param>
        /// <param name="copyValue">TCP 服务配置</param>
        
        internal static void CopyFrom(this Net.TcpStaticSimpleServer.ServerAttribute value, Net.TcpStaticSimpleServer.ServerAttribute copyValue)
        {
            MemberCopy.Copyer<Net.TcpStaticSimpleServer.ServerAttribute>.Copy(value, copyValue, simpleCopyMemberMap);
        }
    }
}
