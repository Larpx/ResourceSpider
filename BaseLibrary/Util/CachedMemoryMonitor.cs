using Serilog;
using System;
using System.Timers;

namespace Larpx.ResourceSpider.BaseLibrary.Util
{
    public class CachedMemoryMonitor : IMemoryMonitor, IDisposable
    {
        IMemoryMonitor _memoryMonitor;
        Timer _usageRefreshTimer;
        int _cachedCurrentUsageInMb;

        public CachedMemoryMonitor(IMemoryMonitor memoryMonitor, int cacheExpirationInSeconds)
        {
            if (memoryMonitor == null)
                throw new ArgumentNullException("memoryMonitor");

            if (cacheExpirationInSeconds < 1)
                cacheExpirationInSeconds = 5;

            _memoryMonitor = memoryMonitor;

            UpdateCurrentUsageValue();

            _usageRefreshTimer = new Timer(cacheExpirationInSeconds * 1000);
            _usageRefreshTimer.Elapsed += (sender, e) => UpdateCurrentUsageValue();
            _usageRefreshTimer.Start();
        }

        protected virtual void UpdateCurrentUsageValue()
        {
            var oldUsage = _cachedCurrentUsageInMb;
            _cachedCurrentUsageInMb = _memoryMonitor.GetCurrentUsageInMb();
            Log.Debug("更新缓存的内存使用率从 [{0}mb] 到 [{1}mb]", oldUsage, _cachedCurrentUsageInMb);
        }

        public virtual int GetCurrentUsageInMb()
        {
            return _cachedCurrentUsageInMb;
        }

        public void Dispose()
        {
            _usageRefreshTimer.Stop();
            _usageRefreshTimer.Dispose();
        }
    }
}
