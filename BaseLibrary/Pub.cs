using System;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Larpx.ResourceSpider.BaseLibrary
{
    /// <summary>
    /// 常用公共定义
    /// </summary>
    public static partial class Pub
    {
        /// <summary>
        /// LGD
        /// </summary>
        internal const int PuzzleValue = 0x10035113;
        /// <summary>
        /// 默认自增标识
        /// </summary>
        private static int identity32;
        /// <summary>
        /// 默认自增标识
        /// </summary>
        internal static int Identity32
        {
            get { return Interlocked.Increment(ref identity32); }
        }
        /// <summary>
        /// 清除缓存数据
        /// </summary>
        internal static Action<int> ClearCaches;
        /// <summary>
        /// 清除缓存数据
        /// </summary>
        /// <param name="count">保留缓存数据数量</param>
        
        private static void clearCache(int count)
        {
            AutoCSer.Metadata.MemberIndexGroup.ClearCache();
            AutoCSer.Metadata.AttributeMethod.ClearCache();
            AutoCSer.Metadata.TypeAttribute.ClearCache();
            if (ClearCaches != null) ClearCaches(count);
        }
        /// <summary>
        /// 清除缓存数据
        /// </summary>
        /// <param name="count">保留缓存数据数量</param>
        
        private static void clearUnmanagedCache(int count)
        {
            UnmanagedPool.ClearCache(count);
        }
        /// <summary>
        /// 清除缓存数据
        /// </summary>
        /// <param name="count">保留缓存数据数量</param>
        
        public static void ClearCache(int count = 0)
        {
            clearCache(count);
            GC.Collect();
            clearUnmanagedCache(count);
        }
        /// <summary>
        /// 清除缓存数据
        /// </summary>
        /// <param name="count">保留缓存数据数量</param>
        
        public static void ClearCacheNoGC(int count = 0)
        {
            clearCache(count);
            clearUnmanagedCache(count);
        }

        /// <summary>
        /// 空委托
        /// </summary>
        
        private static void emptyAction() { }
        /// <summary>
        /// 空委托
        /// </summary>
        internal static readonly Action EmptyAction = emptyAction;
    }
}
