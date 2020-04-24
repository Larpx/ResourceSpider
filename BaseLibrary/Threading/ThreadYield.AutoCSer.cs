using System;
using AutoCSer.Extension;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Larpx.ResourceSpider.BaseLibrary.Threading
{
    /// <summary>
    /// 线程操作
    /// </summary>
    public unsafe partial class ThreadYield
    {
        /// <summary>
        /// 线程休眠
        /// </summary>
        
        public static void Yield()
        {
            ++yieldCounts.ULong[(int)Type.Unknown];
            YieldOnly();
        }
    }
}
