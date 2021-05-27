using Serilog;
using System;
using System.Runtime;

namespace Larpx.ResourceSpider.BaseLibrary.Util
{
    /// <summary>
    /// 处理内存监控/使用
    /// </summary>
    public interface IMemoryManager : IMemoryMonitor, IDisposable
    {
        /// <summary>
        /// 当前进程是否被分配/使用超过内存(以mb为单位)的参数值
        /// </summary>
        bool IsCurrentUsageAbove(int sizeInMb);

        /// <summary>
        ///是否至少存在可用内存的参数值(以mb为单位)
        /// </summary>
        bool IsSpaceAvailable(int sizeInMb);
    }

    public class MemoryManager : IMemoryManager
    {
        IMemoryMonitor _memoryMonitor;

        public MemoryManager(IMemoryMonitor memoryMonitor)
        {
            if (memoryMonitor == null)
                throw new ArgumentNullException("memoryMonitor");

            _memoryMonitor = memoryMonitor;
        }

        public virtual bool IsCurrentUsageAbove(int sizeInMb)
        {
            return GetCurrentUsageInMb() > sizeInMb;
        }

        public virtual bool IsSpaceAvailable(int sizeInMb)
        {
            if (sizeInMb < 1)
                return true;

            var isAvailable = true;

            MemoryFailPoint _memoryFailPoint = null;
            try
            {
                _memoryFailPoint = new MemoryFailPoint(sizeInMb);
            }
            catch (InsufficientMemoryException)
            {
                isAvailable = false;
            }
            catch (NotImplementedException)
            {
                Log.Warning("MemoryFailPoint不在此平台上实现。MemoryManager.IsSpaceAvailable()将返回true。");
            }
            finally
            {
                if (_memoryFailPoint != null)
                    _memoryFailPoint.Dispose();
            }

            return isAvailable;
        }

        public virtual int GetCurrentUsageInMb()
        {
            return _memoryMonitor.GetCurrentUsageInMb();
        }

        public void Dispose()
        {
            _memoryMonitor.Dispose();
        }
    }
}
