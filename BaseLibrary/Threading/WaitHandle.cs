using System;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Larpx.ResourceSpider.BaseLibrary.Threading
{
    /// <summary>
    /// 同步等待锁
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    internal struct WaitHandle
    {
        /// <summary>
        /// 同步等待锁
        /// </summary>
        private object waitLock;
        /// <summary>
        /// 是否等待中
        /// </summary>
        private int isSet;
        /// <summary>
        /// 等待数量
        /// </summary>
        private int waitCount;
        /// <summary>
        /// 初始化同步等待锁
        /// </summary>
        /// <param name="isSet">是否等待中</param>
        internal void Set(int isSet)
        {
            waitLock = new object();
            this.isSet = isSet;
        }
        /// <summary>
        /// 重置等待
        /// </summary>
        
        public void Reset()
        {
            Monitor.Enter(waitLock);
            if (isSet != 0) isSet = 0;
            Monitor.Exit(waitLock);
        }
        /// <summary>
        /// 等待结束
        /// </summary>
        
        public void Wait()
        {
            if (isSet == 0)
            {
                Monitor.Enter(waitLock);
                if (isSet == 0)
                {
                    ++waitCount;
                    do { Monitor.Wait(waitLock); } while (isSet == 0);
                    --waitCount;
                }
                Monitor.Pulse(waitLock);
                Monitor.Exit(waitLock);
            }
        }
        /// <summary>
        /// 结束等待
        /// </summary>
        
        public void Set()
        {
            Monitor.Enter(waitLock);
            if (isSet == 0)
            {
                isSet = 1;
                Monitor.Pulse(waitLock);
            }
            Monitor.Exit(waitLock);
        }
        /// <summary>
        /// 结束等待并重置
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        
        internal void PulseReset(int millisecondsTimeout = 1)
        {
            Set();
            while (waitCount != 0) System.Threading.Thread.Sleep(millisecondsTimeout);
            Reset();
        }
    }
}
