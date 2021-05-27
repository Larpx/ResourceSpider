using System;
using System.Threading;

namespace Larpx.ResourceSpider.BaseLibrary.Util
{
    /// <summary>
    ///将使用真正线程来处理并发的ThreadManager实现。
    /// </summary>
    public class ManualThreadManager : ThreadManager
    {
        public ManualThreadManager(int maxThreads)
            : base(maxThreads)
        {
        }

        protected override void RunActionOnDedicatedThread(Action action)
        {
            new Thread(() => RunAction(action)).Start();
        }
    }
}
