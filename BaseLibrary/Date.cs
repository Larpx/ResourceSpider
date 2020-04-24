using System;
using System.Threading;
using Larpx.ResourceSpider.BaseLibrary.Extension;
using System.Diagnostics;

namespace Larpx.ResourceSpider.BaseLibrary
{
    /// <summary>
    /// 日期相关操作
    /// </summary>
    public unsafe static partial class Date
    {
        /// <summary>
        /// 初始化时间 Utc
        /// </summary>
        public static readonly DateTime StartTime;

        /// <summary>
        /// 初始化时钟周期
        /// </summary>
        private static readonly long startTimestamp;

        /// <summary>
        /// 本地时钟周期
        /// </summary>
        public static readonly long LocalTimeTicks;

        /// <summary>
        /// 时区小时字符串 +HH:
        /// </summary>
        internal static readonly long ZoneHourString;

        /// <summary>
        /// 时区f分钟字符串 mm"
        /// </summary>
        internal static readonly long ZoneMinuteString;

        /// <summary>
        /// 精确到秒的时间
        /// </summary>
        internal static class NowTime
        {
            /// <summary>
            /// 精确到秒的时间
            /// </summary>
            internal static DateTime Now;

            /// <summary>
            /// 精确到秒的时间
            /// </summary>
            internal static DateTime UtcNow;

            /// <summary>
            /// 重置时间
            /// </summary>
            /// <returns></returns>
            internal static DateTime Set()
            {
                DateTime now = DateTime.Now;
                Now = now;
                UtcNow = now.localToUniversalTime();
                return now;
            }

            /// <summary>
            /// 重置时间
            /// </summary>
            /// <returns></returns>
            internal static DateTime SetUtc()
            {
                DateTime now = DateTime.Now;
                Now = now;
                UtcNow = now.localToUniversalTime();
                return UtcNow;
            }

            /// <summary>
            /// 刷新时间的定时器
            /// </summary>
            private readonly static Timer timer;

            /// <summary>
            /// 
            /// </summary>
            internal static long TimerInterval;

#if !Serialize
            /// <summary>
            /// 下一秒时钟周期
            /// </summary>
            internal static long NextSecondTicks;

            /// <summary>
            /// 当前时钟秒数计数
            /// </summary>
            internal static long CurrentSeconds;

            /// <summary>
            /// 内部项目定时器
            /// </summary>
            internal abstract class OnTime
            {
                /// <summary>
                /// 定时处理
                /// </summary>
                internal abstract void OnTimer();
            }

            /// <summary>
            /// 定时触发类型
            /// </summary>
            internal static bool IsOnTime;

            /// <summary>
            /// 定时触发 TCP 应答服务扩展
            /// </summary>
            internal static OnTime TcpSimpleServerOnTime;

            /// <summary>
            /// 定时触发 WEB 扩展
            /// </summary>
            internal static OnTime WebViewOnTime;

            /// <summary>
            /// 定时触发 Sql 扩展
            /// </summary>
            internal static OnTime SqlOnTime;

            /// <summary>
            /// 定时触发 缓存 扩展
            /// </summary>
            internal static OnTime CacheServerOnTime;
#endif

            /// <summary>
            /// 刷新时间
            /// </summary>
            /// <param name="state"></param>
            private static void refreshTime(object state)
            {
                DateTime now = DateTime.Now;
                Now = now;
                UtcNow = now.localToUniversalTime();
                timer.Change(TimerInterval = 1000L - now.Millisecond, -1);
#if !Serialize
            CHECK:
                long nextSecondTicks = NextSecondTicks;
                if (nextSecondTicks <= Now.Ticks)
                {
                    if (Interlocked.CompareExchange(ref NextSecondTicks, nextSecondTicks + TimeSpan.TicksPerSecond, nextSecondTicks) == nextSecondTicks)
                    {
                        Interlocked.Increment(ref CurrentSeconds);
                        try
                        {
                            Threading.ThreadPool.CheckExit();
                            for (Net.SocketTimeoutLink.TimerLink timeout = Net.SocketTimeoutLink.TimerLink.TimeoutEnd; timeout != null; timeout = timeout.DoubleLinkPrevious)
                                timeout.OnTimer();
                            for (Net.TcpServer.ClientCheckTimer timeout = Net.TcpServer.ClientCheckTimer.TimeoutEnd; timeout != null; timeout = timeout.DoubleLinkPrevious)
                                timeout.OnTimer();
                            for (TimeoutCount timeout = TimeoutCount.OnTimerLink.End; timeout != null; timeout = timeout.DoubleLinkPrevious) timeout.OnTimer();
                            if (IsOnTime)
                            {
                                if (SqlOnTime != null) SqlOnTime.OnTimer();
                                if (WebViewOnTime != null) WebViewOnTime.OnTimer();
                                if (CacheServerOnTime != null) CacheServerOnTime.OnTimer();
                                if (TcpSimpleServerOnTime != null) TcpSimpleServerOnTime.OnTimer();
                            }

                            Threading.TimerTask.Default.OnTimer(Now);
                            if (Date.OnTime != null) Date.OnTime();
                        }
                        catch (Exception error)
                        {
                            throw error;
                        }
                    }
                    goto CHECK;
                }
#endif
            }
            /// <summary>
            /// 激活计时器
            /// </summary>

            public static bool OnTimeFlag;

            static NowTime()
            {
                UtcNow = (Now = DateTime.Now).localToUniversalTime();
#if !Serialize
                NextSecondTicks = ((Now.Ticks / TimeSpan.TicksPerSecond) + 1) * TimeSpan.TicksPerSecond;
#endif
                timer = new Timer(refreshTime, null, TimerInterval = 1000L - Now.Millisecond, -1);
            }
        }

        /// <summary>
        /// 时间更新间隔
        /// </summary>
        internal static int NowTimerInterval
        {
            get { return (int)NowTime.TimerInterval; }
        }

        /// <summary>
        /// 精确到秒的时间
        /// </summary>
        public static DateTime Now
        {
            get { return NowTime.Now; }
        }

        /// <summary>
        /// 精确到秒的时间
        /// </summary>
        public static DateTime UtcNow
        {
            get { return NowTime.UtcNow; }
        }

#if !Serialize
        /// <summary>
        /// 自定义定时触发
        /// </summary>
        public static event Action OnTime;
#endif

        /// <summary>
        /// 时间转换字符串字节长度
        /// </summary>
        public const int MillisecondStringSize = 23;

        /// <summary>
        /// 默认日期分隔符
        /// </summary>
        internal const char DateSplitChar = '/';

        /// <summary>
        /// 时间转换成字符串(精确到毫秒)
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="charStream">字符流</param>
        internal unsafe static void ToMillisecondString(DateTime time, CharStream charStream)
        {
            toMillisecondString(time, charStream.CurrentChar);
            charStream.ByteSize += MillisecondStringSize * sizeof(char);
        }

        /// <summary>
        /// 时间转换成字符串(精确到毫秒)
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="chars">时间字符串</param>
        private unsafe static void toMillisecondString(DateTime time, char* chars)
        {
            long dayTiks = time.Ticks % TimeSpan.TicksPerDay;
            toString(time, chars, DateSplitChar);
            long seconds = dayTiks / TimeSpan.TicksPerSecond;
            *(chars + 19) = '.';
            *(chars + 10) = ' ';
            toString((int)seconds, chars + 11);
            int data0 = (int)(((ulong)(dayTiks - seconds * TimeSpan.TicksPerSecond) * Extension.Number.Div10000Mul) >> Extension.Number.Div10000Shift);
            int data1 = (data0 * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            int data2 = (data1 * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            *(chars + 22) = (char)(data0 - data1 * 10 + '0');
            *(int*)(chars + 20) = (data2 + ((data1 - data2 * 10) << 16)) + 0x300030;
        }

        /// <summary>
        /// 时间转换成日期字符串(yyyy/MM/dd)
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="chars">时间字符串</param>
        /// <param name="split">分隔符</param>
        private unsafe static void toString(DateTime time, char* chars, char split)
        {
            int data0 = time.Year, data1 = (data0 * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            *(chars + 4) = split;
            int data2 = (data1 * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            *(chars + 7) = split;
            int data3 = (data2 * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            *(int*)(chars + 2) = ((data1 - data2 * 10) + ((data0 - data1 * 10) << 16)) + 0x300030;
            *(int*)chars = (data3 + ((data2 - data3 * 10) << 16)) + 0x300030;
            data0 = time.Month;
            data2 = time.Day;
            data1 = (data0 + 6) >> 4;
            data3 = (data2 * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            *(chars + 5) = (char)(data1 + '0');
            *(chars + 6) = (char)((data0 - data1 * 10) + '0');
            *(int*)(chars + 8) = (data3 + ((data2 - data3 * 10) << 16)) + 0x300030;
        }

        /// <summary>
        /// 32位除以60转乘法的乘数
        /// </summary>
        public const ulong Div60_32Mul = ((1L << Div60_32Shift) + 59) / 60;

        /// <summary>
        /// 32位除以60转乘法的位移
        /// </summary>
        public const int Div60_32Shift = 21 + 32;

        /// <summary>
        /// 16位除以60转乘法的乘数
        /// </summary>
        public const uint Div60_16Mul = ((1U << Div60_16Shift) + 59) / 60;

        /// <summary>
        /// 16位除以60转乘法的位移
        /// </summary>
        public const int Div60_16Shift = 21;

        /// <summary>
        /// 时间转换成字符串(HH:mm:ss)
        /// </summary>
        /// <param name="second">当天的计时秒数</param>
        /// <param name="chars">时间字符串</param>
        private unsafe static void toString(int second, char* chars)
        {
            int minute = (int)(((ulong)second * Div60_32Mul) >> Div60_32Shift);
            int hour = (minute * (int)Div60_16Mul) >> Div60_16Shift;
            second -= minute * 60;
            int high = (hour * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            minute -= hour * 60;
            *chars = (char)(high + '0');
            *(chars + 1) = (char)((hour - high * 10) + '0');
            *(chars + 2) = ':';
            high = (minute * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            *(int*)(chars + 3) = (high + ((minute - high * 10) << 16)) + 0x300030;
            *(chars + 5) = ':';
            high = (second * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            *(chars + 6) = (char)(high + '0');
            *(chars + 7) = (char)((second - high * 10) + '0');
        }

        /// <summary>
        /// 时间转换成字符串(yyyy/MM/dd HH:mm:ss)
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="dateSplit">日期分隔符</param>
        /// <returns>时间字符串</returns>
        public unsafe static string toString(this DateTime time, char dateSplit = DateSplitChar)
        {
            string timeString = Extension.StringExtension.FastAllocateString(19);
            fixed (char* timeFixed = timeString)
            {
                toString(time, timeFixed, dateSplit);
                *(timeFixed + 10) = ' ';
                toString((int)((time.Ticks % TimeSpan.TicksPerDay) / TimeSpan.TicksPerSecond), timeFixed + 11);
            }
            return timeString;
        }

        /// <summary>
        /// 时间转换成字符串
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="charStream">字符流</param>
        /// <param name="dateSplit">日期分隔符</param>
        internal unsafe static void ToString(this DateTime time, CharStream charStream, char dateSplit = DateSplitChar)
        {
            char* timeFixed = charStream.GetPrepSizeCurrent(19);
            toString(time, timeFixed, dateSplit);
            *(timeFixed + 10) = ' ';
            toString((int)((time.Ticks % TimeSpan.TicksPerDay) / TimeSpan.TicksPerSecond), timeFixed + 11);
            charStream.ByteSize += 19 * sizeof(char);
        }

        /// <summary>
        /// 时间转换成字符串 yyyy-MM-ddTHH:mm:ss.XXXXXXX
        /// </summary>
        /// <param name="time"></param>
        /// <param name="timeFixed"></param>
        /// <returns>字符串长度</returns>
        internal static int ToString(this DateTime time, char* timeFixed)
        {
            toString(time, timeFixed, '-');
            *(timeFixed + 10) = 'T';
            long ticks = time.Ticks % TimeSpan.TicksPerDay, seconds = ticks / TimeSpan.TicksPerSecond;
            toString((int)seconds, timeFixed + 11);
            ticks -= seconds * TimeSpan.TicksPerSecond;
            if (ticks == 0) return 19;
            int low = (int)(uint)ticks, high = (int)(((uint)low * Number.Div10000Mul) >> Number.Div10000Shift);
            int data1 = (high * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            int data2 = (data1 * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            *(long*)(timeFixed + 19) = '.' + (data2 << 16) + ((long)(data1 - data2 * 10) << 32) + ((long)(high - data1 * 10) << 48) + 0x30003000300000L;
            if ((low -= high * 10000) == 0) return 23;
            high = (low * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            data1 = (high * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            data2 = (data1 * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            *(long*)(timeFixed + 23) = data2 + ((data1 - data2 * 10) << 16) + ((long)(high - data1 * 10) << 32) + ((long)(low - high * 10) << 48) + 0x30003000300030L;
            return 27;
        }

        /// <summary>
        /// 时间转换
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        internal static DateTime localToUniversalTime(this DateTime date)
        {
            return new DateTime(date.Ticks - LocalTimeTicks, DateTimeKind.Utc);
        }

        static Date()
        {
            if (Stopwatch.IsHighResolution)
            {
                StartTime = DateTime.UtcNow;
                startTimestamp = Stopwatch.GetTimestamp();
            }
            else
            {
                StartTime = DateTime.UtcNow;
                startTimestamp = StartTime.Ticks;
            }
#if DOTNET2
            LocalTimeTicks = StartTime.ToLocalTime().Ticks - StartTime.Ticks;
#else
            LocalTimeTicks = TimeZoneInfo.Local.BaseUtcOffset.Ticks;
#endif

            long zoneChar0, localTimeTicks;
            if (LocalTimeTicks >= 0)
            {
                zoneChar0 = '+' + ((long)':' << 48);
                localTimeTicks = LocalTimeTicks;
            }
            else
            {
                zoneChar0 = '-' + ((long)':' << 48);
                localTimeTicks = -LocalTimeTicks;
            }
            long minute = (int)(LocalTimeTicks / TimeSpan.TicksPerMinute);
            int hour = (int)(((ulong)minute * Div60_32Mul) >> Div60_32Shift), high = (hour * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            ZoneHourString = zoneChar0 + ((high + '0') << 16) + ((long)((hour - high * 10) + '0') << 32);
            minute -= hour * 60;
            high = ((int)minute * (int)Extension.Number.Div10_16Mul) >> Extension.Number.Div10_16Shift;
            ZoneMinuteString = (high + '0') + ((((int)minute - high * 10) + '0') << 16) + ((long)'"' << 32);

#if !Serialize
            weekData = new Pointer { Data = Unmanaged.GetStatic64(8 * sizeof(int) + 12 * sizeof(int), false) };
            monthData = new Pointer { Data = weekData.Byte + 8 * sizeof(int) };
            int* write = weekData.Int;
            *write = 'S' + ('u' << 8) + ('n' << 16) + (',' << 24);
            *++write = 'M' + ('o' << 8) + ('n' << 16) + (',' << 24);
            *++write = 'T' + ('u' << 8) + ('e' << 16) + (',' << 24);
            *++write = 'W' + ('e' << 8) + ('d' << 16) + (',' << 24);
            *++write = 'T' + ('h' << 8) + ('u' << 16) + (',' << 24);
            *++write = 'F' + ('r' << 8) + ('i' << 16) + (',' << 24);
            *++write = 'S' + ('a' << 8) + ('t' << 16) + (',' << 24);
            write = monthData.Int;
            *write = 'J' + ('a' << 8) + ('n' << 16) + (' ' << 24);
            *++write = 'F' + ('e' << 8) + ('b' << 16) + (' ' << 24);
            *++write = 'M' + ('a' << 8) + ('r' << 16) + (' ' << 24);
            *++write = 'A' + ('p' << 8) + ('r' << 16) + (' ' << 24);
            *++write = 'M' + ('a' << 8) + ('y' << 16) + (' ' << 24);
            *++write = 'J' + ('u' << 8) + ('n' << 16) + (' ' << 24);
            *++write = 'J' + ('u' << 8) + ('l' << 16) + (' ' << 24);
            *++write = 'A' + ('u' << 8) + ('g' << 16) + (' ' << 24);
            *++write = 'S' + ('e' << 8) + ('p' << 16) + (' ' << 24);
            *++write = 'O' + ('c' << 8) + ('t' << 16) + (' ' << 24);
            *++write = 'N' + ('o' << 8) + ('v' << 16) + (' ' << 24);
            *++write = 'D' + ('e' << 8) + ('c' << 16) + (' ' << 24);
#endif
        }
    }

    /// <summary>
    /// 日期相关操作
    /// </summary>
    public unsafe static partial class Date
    {
        /// <summary>
        /// 星期
        /// </summary>
        private static Pointer weekData;

        /// <summary>
        /// 月份
        /// </summary>
        private static Pointer monthData;

        /// <summary>
        /// 时间转字节流长度
        /// </summary>
        internal const int ToByteLength = 29;

        /// <summary>
        /// 时间转换
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        internal static DateTime toUniversalTime(this DateTime date)
        {
            return date.Kind == DateTimeKind.Utc ? date : new DateTime(date.Ticks - LocalTimeTicks, DateTimeKind.Utc);
        }

        /// <summary>
        /// 时间转字节流
        /// </summary>
        /// <param name="date">时间</param>
        /// <param name="data">写入数据起始位置</param>
        internal unsafe static void ToBytes(DateTime date, byte* data)
        {
            UniversalToBytes(date.toUniversalTime(), data);
        }

        /// <summary>
        /// 时间转字节流
        /// </summary>
        /// <param name="date">时间</param>
        /// <param name="data">写入数据起始位置</param>
        internal unsafe static void UniversalToBytes(DateTime date, byte* data)
        {
            *(int*)data = weekData.Int[(int)date.DayOfWeek];
            int value = date.Day, value10 = (value * (int)Number.Div10_16Mul) >> Number.Div10_16Shift;
            *(int*)(data + sizeof(int)) = (' ' + (value10 << 8) + ((value - value10 * 10) << 16) + (' ' << 24)) | 0x303000;
            value = date.Year;
            *(int*)(data + sizeof(int) * 2) = monthData.Int[date.Month - 1];
            value10 = (value * (int)Number.Div10_16Mul) >> Number.Div10_16Shift;
            int value100 = (value10 * (int)Number.Div10_16Mul) >> Number.Div10_16Shift;
            int value1000 = (value100 * (int)Number.Div10_16Mul) >> Number.Div10_16Shift;
            *(int*)(data + sizeof(int) * 3) = (value1000 + ((value100 - value1000 * 10) << 8) + ((value10 - value100 * 10) << 16) + ((value - value10 * 10) << 24)) | 0x30303030;

            value100 = (int)(date.Ticks % TimeSpan.TicksPerDay / TimeSpan.TicksPerSecond);
            value1000 = (int)(((ulong)value100 * Div60_32Mul) >> Div60_32Shift);
            value100 -= value1000 * 60;
            value = (value1000 * (int)Div60_16Mul) >> Div60_16Shift;
            value1000 -= value * 60;

            value10 = (value * (int)Number.Div10_16Mul) >> Number.Div10_16Shift;
            *(int*)(data + sizeof(int) * 4) = (' ' + (value10 << 8) + ((value - value10 * 10) << 16) + (':' << 24)) | 0x303000;
            value10 = (value1000 * (int)Number.Div10_16Mul) >> Number.Div10_16Shift;
            value = (value100 * (int)Number.Div10_16Mul) >> Number.Div10_16Shift;
            *(int*)(data + sizeof(int) * 5) = (value10 + ((value1000 - value10 * 10) << 8) + (':' << 16) + (value << 24)) | 0x30003030;
            *(int*)(data + sizeof(int) * 6) = ((value100 - value * 10) + '0') + (' ' << 8) + ('G' << 16) + ('M' << 24);
            *(data + sizeof(int) * 7) = (byte)'T';
        }

        /// <summary>
        /// 时间转字节流
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        internal unsafe static byte[] ToBytes(this DateTime date)
        {
            byte[] data = new byte[ToByteLength];
            fixed (byte* dataFixed = data) ToBytes(date, dataFixed);
            return data;
        }
        /// <summary>
        /// 时间转字节流
        /// </summary>
        /// <param name="date">时间</param>
        /// <returns>字节流</returns>

        public unsafe static byte[] UniversalNewBytes(this DateTime date)
        {
            byte[] data = new byte[ToByteLength];
            fixed (byte* fixedData = data) UniversalToBytes(date, fixedData);
            return data;
        }
        /// <summary>
        /// 判断时间是否相等
        /// </summary>
        /// <param name="date"></param>
        /// <param name="dataArray"></param>
        /// <returns></returns>
        internal unsafe static int UniversalByteEquals(DateTime date, SubArray<byte> dataArray)
        {
            fixed (byte* dataFixed = dataArray.Array)
            {
                byte* data = dataFixed + dataArray.Start;
                if (((*(int*)data ^ weekData.Int[(int)date.DayOfWeek]) | (*(data + sizeof(int) * 7) ^ (byte)'T')) != 0) return 1;
                int value = date.Day, value10 = (value * (int)Number.Div10_16Mul) >> Number.Div10_16Shift;
                if (*(int*)(data + sizeof(int)) != ((' ' + (value10 << 8) + ((value - value10 * 10) << 16) + (' ' << 24)) | 0x303000)) return 1;
                value = date.Year;
                if (*(int*)(data + sizeof(int) * 2) != monthData.Int[date.Month - 1]) return 1;
                value10 = (value * (int)Number.Div10_16Mul) >> Number.Div10_16Shift;
                int value100 = (value10 * (int)Number.Div10_16Mul) >> Number.Div10_16Shift;
                int value1000 = (value100 * (int)Number.Div10_16Mul) >> Number.Div10_16Shift;
                if (*(int*)(data + sizeof(int) * 3) != ((value1000 + ((value100 - value1000 * 10) << 8) + ((value10 - value100 * 10) << 16) + ((value - value10 * 10) << 24)) | 0x30303030)) return 1;


                value100 = (int)(date.Ticks % TimeSpan.TicksPerDay / TimeSpan.TicksPerSecond);
                value1000 = (int)(((ulong)value100 * Div60_32Mul) >> Div60_32Shift);
                value100 -= value1000 * 60;
                value = (value1000 * (int)Div60_16Mul) >> Div60_16Shift;
                value1000 -= value * 60;

                value10 = (value * (int)Number.Div10_16Mul) >> Number.Div10_16Shift;
                if (*(int*)(data + sizeof(int) * 4) != ((' ' + (value10 << 8) + ((value - value10 * 10) << 16) + (':' << 24)) | 0x303000)) return 1;
                value10 = (value1000 * (int)Number.Div10_16Mul) >> Number.Div10_16Shift;
                value = (value100 * (int)Number.Div10_16Mul) >> Number.Div10_16Shift;
                return (*(int*)(data + sizeof(int) * 5) ^ ((value10 + ((value1000 - value10 * 10) << 8) + (':' << 16) + (value << 24)) | 0x30003030))
                    | (*(int*)(data + sizeof(int) * 6) ^ ((value100 - value * 10) + '0') + (' ' << 8) + ('G' << 16) + ('M' << 24));
            }
        }
        /// <summary>
        /// 时间转换成日期字符串(yyyy/MM/dd)
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="split">分隔符</param>
        /// <returns></returns>

        public unsafe static string toDateString(this DateTime time, char split = '/')
        {
            string timeString = Extension.StringExtension.FastAllocateString(10);
            fixed (char* timeFixed = timeString) toString(time, timeFixed, split);
            return timeString;
        }

        /// <summary>
        /// 每毫秒时间戳
        /// </summary>
        internal static readonly long TimestampPerMillisecond = Stopwatch.IsHighResolution ? Stopwatch.Frequency / 1000 : TimeSpan.TicksPerMillisecond;
        /// <summary>
        /// 每秒 毫秒时间戳误差
        /// </summary>
        internal static readonly long MillisecondTimestampDifferencePerSecond = Stopwatch.IsHighResolution ? Stopwatch.Frequency - Stopwatch.Frequency / 1000 * 1000 : 0;

        /// <summary>
        /// 获取初始化时间差
        /// </summary>
        /// <returns></returns>
        internal static long TimestampDifference
        {
            get
            {
                return Stopwatch.GetTimestamp() - startTimestamp;
            }
        }

        /// <summary>
        /// 时钟周期转时间戳乘数
        /// </summary>
        private static readonly double ticksToTimestamp = Stopwatch.IsHighResolution ? (double)Stopwatch.Frequency / TimeSpan.TicksPerSecond : 1;

        /// <summary>
        /// 时钟周期转时间戳
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        internal static long GetTimestampByTicks(long ticks)
        {
            return Stopwatch.IsHighResolution ? (long)(ticks * ticksToTimestamp) : ticks;
        }

        /// <summary>
        /// 时间戳转毫秒数乘数
        /// </summary>
        private static readonly double timestampToMilliseconds = Stopwatch.IsHighResolution ? 1000 / (double)Stopwatch.Frequency : (1 / (double)TimeSpan.TicksPerMillisecond);

        /// <summary>
        /// 时间戳转毫秒数
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        internal static long GetMillisecondsByTimestamp(long timestamp)
        {
            if (Stopwatch.IsHighResolution) return (long)(timestamp * timestampToMilliseconds);
            return timestamp / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// 毫秒数转时间戳乘数
        /// </summary>
        private static readonly double millisecondsToTimestamp = Stopwatch.IsHighResolution ? (double)Stopwatch.Frequency / 1000 : TimeSpan.TicksPerMillisecond;

        /// <summary>
        /// 毫秒数转时间戳
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        internal static long GetTimestampByMilliseconds(long milliseconds)
        {
            if (Stopwatch.IsHighResolution) return (long)(milliseconds * millisecondsToTimestamp);
            return milliseconds * TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// 获取时钟周期差值（不处理溢出）
        /// </summary>
        /// <param name="startTimestamp"></param>
        /// <returns></returns>
        public static TimeSpan GetTimestampTimeSpan(long startTimestamp)
        {
            return new TimeSpan(GetTicksByTimestamp(Stopwatch.GetTimestamp() - startTimestamp));
        }

        /// <summary>
        /// 时间戳转时钟周期
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static long GetTicksByTimestamp(long timestamp)
        {
            return Stopwatch.IsHighResolution ? timestamp * TimeSpan.TicksPerSecond / Stopwatch.Frequency : timestamp;
        }
    }
}
