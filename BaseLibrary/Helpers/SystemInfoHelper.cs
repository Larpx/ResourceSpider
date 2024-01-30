using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Larpx.ResourceSpider.BaseLibrary.Helpers
{
    public static class SystemInfoHelper
    {
        public static readonly int TotalMemory;

        static SystemInfoHelper()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var mStatus = new MemoryStatus();
                GlobalMemoryStatus(ref mStatus);
                TotalMemory = (int)(Convert.ToInt64(mStatus.DwTotalPhys) / 1024 / 1024);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var lines = File.ReadAllLines("/proc/meminfo");
                var infoDict = lines
                    .Select(line => line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Take(2).ToList())
                    .ToDictionary(items => items[0], items => long.Parse(items[1]));
                TotalMemory = (int)(infoDict["MemTotal:"] / 1024);
            }
        }

        /// <summary> 
        /// 判断当前系统是否为64位系统
        /// </summary> 
        /// <returns>64位返回true，32位false</returns> 
        public static bool Is64bit()
        {
            try
            {
                return IntPtr.Size == 8;
            }
            catch
            {
                return false;
            }
        }

        private struct MemoryStatus
        {
            public uint DwLength { get; set; }
            public uint DwMemoryLoad { get; set; }
            public ulong DwTotalPhys { get; set; } //总的物理内存大小
            public ulong DwAvailPhys { get; set; } //可用的物理内存大小
            public ulong DwTotalPageFile { get; set; }
            public ulong DwAvailPageFile { get; set; } //可用的页面文件大小
            public ulong DwTotalVirtual { get; set; } //返回调用进程的用户模式部分的全部可用虚拟地址空间
            public ulong DwAvailVirtual { get; set; } // 返回调用进程的用户模式部分的实际自由可用的虚拟地址空间
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatus(ref MemoryStatus lpBuffer);
    }
}
