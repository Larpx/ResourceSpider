using Serilog;
using System;
using System.Diagnostics;

namespace Larpx.ResourceSpider.BaseLibrary.Util
{
    public interface IMemoryMonitor : IDisposable
    {
        int GetCurrentUsageInMb();
    }

    public class GcMemoryMonitor : IMemoryMonitor
    {
        public virtual int GetCurrentUsageInMb()
        {
            var timer = Stopwatch.StartNew();
            var currentUsageInMb = Convert.ToInt32(GC.GetTotalMemory(false) / (1024 * 1024));
            timer.Stop();

            Log.Debug("GC报告当前分配了 [{0}mb] 空间,耗费 [{1}] 毫秒", currentUsageInMb, timer.ElapsedMilliseconds);

            return currentUsageInMb;
        }

        public void Dispose()
        {
            //do nothing
        }
    }
}
