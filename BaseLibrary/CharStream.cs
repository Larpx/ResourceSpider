using System;
using System.Runtime.CompilerServices;

namespace Larpx.ResourceSpider.BaseLibrary
{
    /// <summary>
    /// 内存字符流
    /// </summary>
    public sealed unsafe partial class CharStream : UnmanagedStreamBase
    {
        /// <summary>
        /// 数据
        /// </summary>
        public char* Char
        {
            get { return Data.Char; }
        }

        /// <summary>
        /// 当前写入位置
        /// </summary>
        public char* CurrentChar
        {
            get { return (char*)(Data.Byte + ByteSize); }
        }

        /// <summary>
        /// 当前数据长度
        /// </summary>
        public int Length
        {
            get { return ByteSize >> 1; }
        }

        /// <summary>
        /// 内存数据流
        /// </summary>
        /// <param name="length">容器初始尺寸</param>
        public CharStream(int length = UnmanagedPool.TinySize >> 1) : base(length << 1) { }

        /// <summary>
        /// 非托管内存数据流
        /// </summary>
        /// <param name="data">无需释放的数据</param>
        /// <param name="length">容器初始尺寸</param>
        internal CharStream(char* data, int length) : base((byte*)data, length << 1) { }

        /// <summary>
        /// 预增数据流长度
        /// </summary>
        /// <param name="size">增加长度</param>
        internal char* GetPrepSizeCurrent(int size)
        {
            prepSize(size << 1);
            return (char*)(Data.Byte + ByteSize);
        }

        /// <summary>
        /// 预增数据流长度
        /// </summary>
        /// <param name="length">增加长度</param>
        internal void PrepLength(int length)
        {
            prepSize(length << 1);
        }

        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="value">数据</param>
        internal void UnsafeWrite(char value)
        {
            *(char*)CurrentData = value;
            ByteSize += sizeof(char);
        }

        /// <summary>
        /// 写字符串
        /// </summary>
        /// <param name="value">字符串</param>
        public void WriteNotNull(string value)
        {
            int length = value.Length << 1;
            prepSize(length);
            Extension.StringExtension.CopyNotNull(value, Data.Byte + ByteSize);
            ByteSize += length;
        }

        /// <summary>
        /// 写字符串
        /// </summary>
        /// <param name="value">字符串</param>
        internal void UnsafeWrite(string value)
        {
            Extension.StringExtension.CopyNotNull(value, Data.Byte + ByteSize);
            ByteSize += value.Length << 1;
        }

        /// <summary>
        /// 写字符串
        /// </summary>
        /// <param name="value">字符串</param>
        internal void SimpleWriteNotNull(string value)
        {
            fixed (char* valueFixed = value)
            {
                int length = value.Length << 1;
                prepSize(length + 6);
                Extension.StringExtension.SimpleCopyNotNull64(valueFixed, (char*)(Data.Byte + ByteSize), value.Length);
                ByteSize += length;
            }
        }

        /// <summary>
        /// 写字符串(无需预增数据流)
        /// </summary>
        /// <param name="value">字符串,长度必须>0</param>
        internal void UnsafeSimpleWrite(string value)
        {
            Extension.StringExtension.SimpleCopyNotNull(value, Data.Byte + ByteSize);
            ByteSize += value.Length << 1;
        }

        /// <summary>
        /// 写字符串
        /// </summary>
        /// <param name="start">字符串起始位置,不能为null</param>
        /// <param name="count">写入字符数，必须>0</param>
        internal void UnsafeSimpleWriteNotNull(char* start, int count)
        {
            Extension.StringExtension.SimpleCopyNotNull64(start, (char*)(Data.Byte + ByteSize), count);
            ByteSize += count << 1;
        }

        /// <summary>
        /// 输出 double 值
        /// </summary>
        /// <param name="value"></param>
        internal void WriteJsonInfinity(float value)
        {
            if (!float.IsNaN(value))
            {
                if (!float.IsInfinity(value))
                    SimpleWriteNotNull(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                else if (float.IsPositiveInfinity(value))
                    WritePositiveInfinity();
                else
                    WriteNegativeInfinity();
            }
            else
                WriteJsonNaN();
        }

        /// <summary>
        /// 输出 double 值
        /// </summary>
        /// <param name="value"></param>
        internal void WriteJsonInfinity(double value)
        {
            if (!double.IsNaN(value))
            {
                if (!double.IsInfinity(value))
                {
                    if (value <= 1.7976931348623150E+308)
                    {
                        if (value >= -1.7976931348623150E+308)
                            SimpleWriteNotNull(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        else
                            writeDoubleMinValue(value);
                    }
                    else
                        writeDoubleMaxValue(value);
                }
                else if (double.IsPositiveInfinity(value))
                    WritePositiveInfinity();
                else
                    WriteNegativeInfinity();
            }
            else WriteJsonNaN();
        }

        /// <summary>
        /// 输出非数字值
        /// </summary>
        public void WriteJsonNaN()
        {
            *(long*)GetPrepSizeCurrent(4) = 'N' + ('a' << 16) + ((long)'N' << 32);
            ByteSize += 3 * sizeof(char);
        }

        /// <summary>
        /// 输出正无穷
        /// </summary>
        internal void WritePositiveInfinity()
        {
            writeInfinity((byte*)GetPrepSizeCurrent(8));
            ByteSize += 8 * sizeof(char);
        }

        /// <summary>
        /// 输出负无穷
        /// </summary>
        internal void WriteNegativeInfinity()
        {
            byte* write = (byte*)GetPrepSizeCurrent(9);
            *(char*)write = '-';
            writeInfinity(write + sizeof(char));
            ByteSize += 9 * sizeof(char);
        }

        private static void writeInfinity(byte* write)
        {//Infinity
            *(long*)write = 'I' + ('n' << 16) + ((long)'f' << 32) + ((long)'i' << 48);
            *(long*)(write + sizeof(long)) = 'n' + ('i' << 16) + ((long)'t' << 32) + ((long)'y' << 48);
        }

        /// <summary>
        /// 输出 double 最大值
        /// </summary>
        /// <param name="value"></param>
        private void writeDoubleMaxValue(double value)
        {
            byte* write = (byte*)GetPrepSizeCurrent(24);
            //1.79 7693 1348 6231 57E+ 308
            *(long*)write = '1' + ('.' << 16) + ((long)'7' << 32) + ((long)'9' << 48);
            *(long*)(write + sizeof(long)) = '7' + ('6' << 16) + ((long)'9' << 32) + ((long)'3' << 48);
            *(long*)(write + sizeof(long) * 2) = '1' + ('3' << 16) + ((long)'4' << 32) + ((long)'8' << 48);
            *(long*)(write + sizeof(long) * 3) = '6' + ('2' << 16) + ((long)'3' << 32) + ((long)'1' << 48);
            if (value >= 1.7976931348623157E+308)
                *(long*)(write + sizeof(long) * 4) = '5' + ('7' << 16) + ((long)'E' << 32) + ((long)'+' << 48);
            else if (value >= 1.7976931348623154E+308)
            {
                if (value >= 1.7976931348623155E+308)
                    *(long*)(write + sizeof(long) * 4) = '5' + ('6' << 16) + ((long)'E' << 32) + ((long)'+' << 48);
                else
                    *(long*)(write + sizeof(long) * 4) = '5' + ('4' << 16) + ((long)'E' << 32) + ((long)'+' << 48);
            }
            else
                *(long*)(write + sizeof(long) * 4) = '5' + ('2' << 16) + ((long)'E' << 32) + ((long)'+' << 48);
            *(long*)(write + sizeof(long) * 5) = '3' + ('0' << 16) + ((long)'8' << 32);
            ByteSize += 23 * sizeof(char);
        }

        /// <summary>
        /// 输出 double 最大值
        /// </summary>
        /// <param name="value"></param>
        private void writeDoubleMinValue(double value)
        {
            byte* write = (byte*)GetPrepSizeCurrent(24);
            //-1.7 9769 3134 8623 157E +308
            *(long*)write = '-' + ('1' << 16) + ((long)'.' << 32) + ((long)'7' << 48);
            *(long*)(write + sizeof(long)) = '9' + ('7' << 16) + ((long)'6' << 32) + ((long)'9' << 48);
            *(long*)(write + sizeof(long) * 2) = '3' + ('1' << 16) + ((long)'3' << 32) + ((long)'4' << 48);
            *(long*)(write + sizeof(long) * 3) = '8' + ('6' << 16) + ((long)'2' << 32) + ((long)'3' << 48);
            if (value <= -1.7976931348623157E+308)
                *(long*)(write + sizeof(long) * 4) = '1' + ('5' << 16) + ((long)'7' << 32) + ((long)'E' << 48);
            else if (value <= -1.7976931348623154E+308)
            {
                if (value <= -1.7976931348623155E+308)
                    *(long*)(write + sizeof(long) * 4) = '1' + ('5' << 16) + ((long)'6' << 32) + ((long)'E' << 48);
                else
                    *(long*)(write + sizeof(long) * 4) = '1' + ('5' << 16) + ((long)'4' << 32) + ((long)'E' << 48);
            }
            else
                *(long*)(write + sizeof(long) * 4) = '1' + ('5' << 16) + ((long)'2' << 32) + ((long)'E' << 48);
            *(long*)(write + sizeof(long) * 5) = '+' + ('3' << 16) + ((long)'0' << 32) + ((long)'8' << 48);
            ByteSize += 24 * sizeof(char);
        }
    }

    /// <summary>
    /// 内存字符流
    /// </summary>
    public sealed unsafe partial class CharStream
    {
        /// <summary>
        /// 数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>

        public unsafe void WriteJson(bool value)
        {
            Write(value ? '1' : '0');
        }

        /// <summary>
        /// 数字转换成字符串
        /// </summary>
        /// <param name="value">数字值</param>
        public unsafe void WriteJson(decimal value)
        {
            SimpleWriteNotNull(value.ToString());
        }

        /// <summary>
        /// 输出 JSON 字符串，不处理转义符
        /// </summary>
        /// <param name="value">字符串</param>
        public void CopyJsonNotNull(string value)
        {
            PrepLength(value.Length + 2);
            UnsafeWrite('"');
            UnsafeWrite(value);
            UnsafeWrite('"');
        }

        /// <summary>
        /// 写入 JSON 字符串
        /// </summary>
        /// <param name="value">不能为 null</param>
        public void WriteJson(string value)
        {
            fixed (char* valueFixed = value)
                WriteJson(valueFixed, value.Length, ' ');
        }

        /// <summary>
        /// 写入 JSON 字符串
        /// </summary>
        /// <param name="value"></param>
        public void WriteJson(SubString value)
        {
            fixed (char* valueFixed = value.String)
                WriteJson(valueFixed + value.Start, value.Length, ' ');
        }

        /// <summary>
        /// 模拟javascript解码函数unescape
        /// </summary>
        /// <param name="value">原字符串,长度必须大于 0</param>
        public void JavascriptUnescape(SubArray<byte> value)
        {
            fixed (byte* valueFixed = value.Array)
            {
                PrepLength(value.Length + 2);
                byte* start = valueFixed + value.Start, end = start + value.Length;
                while (start != end && *start != '%')
                {
                    UnsafeWrite(*start == 0 ? ' ' : (char)*start);
                    ++start;
                }
                if (start != end)
                    javascriptUnescape(start, end);
            }
        }

        /// <summary>
        /// 模拟javascript解码函数unescape
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void javascriptUnescape(byte* start, byte* end)
        {
        NEXT:
            if (*++start == 'u')
            {
                uint code = (uint)(*++start - '0'), number = (uint)(*++start - '0');
                if (code > 9)
                    code = ((code - ('A' - '0')) & 0xffdfU) + 10;
                if (number > 9)
                    number = ((number - ('A' - '0')) & 0xffdfU) + 10;
                code <<= 12;
                code += (number << 8);
                if ((number = (uint)(*++start - '0')) > 9)
                    number = ((number - ('A' - '0')) & 0xffdfU) + 10;
                code += (number << 4);
                number = (uint)(*++start - '0');
                code += (number > 9 ? (((number - ('A' - '0')) & 0xffdfU) + 10) : number);
                UnsafeWrite(code == 0 ? ' ' : (char)code);
            }
            else
            {
                uint code = (uint)(*start - '0'), number = (uint)(*++start - '0');
                if (code > 9)
                    code = ((code - ('A' - '0')) & 0xffdfU) + 10;
                code = (number > 9 ? (((number - ('A' - '0')) & 0xffdfU) + 10) : number) + (code << 4);
                UnsafeWrite(code == 0 ? ' ' : (char)code);
            }
            while (++start < end)
            {
                if (*start == '%')
                    goto NEXT;
                UnsafeWrite(*start == 0 ? ' ' : (char)*start);
            }
        }
    }
}
