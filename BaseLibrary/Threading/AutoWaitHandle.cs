using System;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Larpx.ResourceSpider.BaseLibrary.Threading
{
    /// <summary>
    /// 一次性等待锁
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    internal struct AutoWaitHandle
    {
        /// <summary>
        /// 同步等待锁
        /// </summary>
        private object waitLock;
        /// <summary>
        /// 是否等待中
        /// </summary>
        private volatile int isWait;
        /// <summary>
        /// 保留
        /// </summary>
        internal int Reserved;
        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="isWait">是否等待中</param>
        internal void Set(int isWait)
        {
            waitLock = new object();
            this.isWait = isWait;
        }
        /// <summary>
        /// 等待结束
        /// </summary>
        
        public void Wait()
        {
            Monitor.Enter(waitLock);
            if (isWait == 0)
            {
                isWait = 1;
                Monitor.Wait(waitLock);
            }
            isWait = 0;
            Monitor.Exit(waitLock);
        }
        /// <summary>
        /// 结束等待
        /// </summary>
        
        public void Set()
        {
            Monitor.Enter(waitLock);
            if (isWait == 0) isWait = 1;
            else Monitor.Pulse(waitLock);
            Monitor.Exit(waitLock);
        }
        /// <summary>
        /// 如果初始化则等待结束
        /// </summary>
        
        public void TryWait()
        {
            if (waitLock != null) Wait();
        }
    }
}
