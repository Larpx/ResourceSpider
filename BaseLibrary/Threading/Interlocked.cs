using System;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Larpx.ResourceSpider.BaseLibrary.Threading
{
    /// <summary>
    /// 原子操作扩张
    /// </summary>
    public unsafe static partial class Interlocked
    {
        /// <summary>
        /// 循环等待(适应于等待时间极短的情况)
        /// </summary>
        /// <param name="value">目标值</param>
        
        public static void CompareExchangeYield(ref int value)
        {
            while (System.Threading.Interlocked.CompareExchange(ref value, 1, 0) != 0) ThreadYield.Yield(ThreadYield.Type.Unknown);
        }
    }
}
