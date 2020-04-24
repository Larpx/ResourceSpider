using System;
using Larpx.ResourceSpider.BaseLibrary.Extension;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Larpx.ResourceSpider.BaseLibrary
{
    /// <summary>
    /// 单向动态数组
    /// </summary>
    /// <typeparam name="valueType">数据类型</typeparam>
    public partial class ListArray<valueType> : DynamicArray<valueType>, IEnumerable<valueType>, IEnumerable
    {
        /// <summary>
        /// 数据数量
        /// </summary>
        internal int Length;
        /// <summary>
        /// 单向动态数据
        /// </summary>
        public ListArray() { Array = NullValue<valueType>.Array; }
        /// <summary>
        /// 设置或获取值
        /// </summary>
        /// <param name="index">位置</param>
        /// <returns>数据值</returns>
        public valueType this[int index]
        {
            get
            {
                if ((uint)index < (uint)Length) return Array[index];
                throw new IndexOutOfRangeException("index[" + index.toString() + "] >= Length[" + Length.toString() + "]");
            }
            set
            {
                if ((uint)index < (uint)Length) Array[index] = value;
                else throw new IndexOutOfRangeException("index[" + index.toString() + "] >= Length[" + Length.toString() + "]");
            }
        }
        /// <summary>
        /// 单向动态数据
        /// </summary>
        /// <param name="values">数据数组</param>
        /// <param name="isUnsafe">true表示使用原数组,false表示需要复制数组</param>
        internal ListArray(valueType[] values, bool isUnsafe = false)
        {
            if ((Length = values.length()) == 0) Array = NullValue<valueType>.Array;
            else if (isUnsafe) Array = values;
            else System.Array.Copy(values, 0, Array = GetNewArray(Length), 0, Length);
        }
        /// <summary>
        /// 单向动态数据
        /// </summary>
        /// <param name="array"></param>
        internal ListArray(LeftArray<valueType> array)
        {
            Array = array.Array ?? NullValue<valueType>.Array;
            Length = array.Length;
        }
        /// <summary>
        /// 枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        
        IEnumerator<valueType> IEnumerable<valueType>.GetEnumerator()
        {
            if (Length != 0) return new Enumerator<valueType>.Array(Array, 0, Length);
            return Enumerator<valueType>.Empty;
        }
        /// <summary>
        /// 枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (Length != 0) return new Enumerator<valueType>.Array(Array, 0, Length);
            return Enumerator<valueType>.Empty;
        }
        /// <summary>
        /// 增加数据长度
        /// </summary>
        /// <param name="length">数据长度</param>
        
        private void addToLength(int length)
        {
            if (length > Array.Length) Array = Array.copyNew(length, Length);
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据</param>
        public void Add(valueType value)
        {
            if (Length != 0)
            {
                if (Length == Array.Length) Array = Array.copyNew(Array.Length << 1, Length);
            }
            else if (Array.Length == 0) Array = new valueType[sizeof(int)];
            Array[Length] = value;
            ++Length;
        }
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">数量</param>
        public override void Add(valueType[] values, int index, int count)
        {
            FormatRange range = new FormatRange(values.length(), index, count);
            if ((count = range.GetCount) != 0)
            {
                int newLength = Length + count;
                addToLength(newLength);
                System.Array.Copy(values, range.SkipCount, Array, Length, count);
                Length = newLength;
            }
        }
        /// <summary>
        /// 获取匹配数据位置
        /// </summary>
        /// <param name="value">匹配数据</param>
        /// <returns>匹配位置,失败为-1</returns>
        
        public int IndexOf(valueType value)
        {
            return Length != 0 ? System.Array.IndexOf<valueType>(Array, value, 0, Length) : -1;
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>是否存在移除数据</returns>
        
        public bool Remove(valueType value)
        {
            int index = IndexOf(value);
            if (index >= 0)
            {
                removeAt(index);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="index">数据位置</param>
        /// <returns>被移除数据</returns>
        
        private void removeAt(int index)
        {
            Extension.ArrayExtension.MoveNotNull(Array, index + 1, index, --Length - index);
            Array[Length] = default(valueType);
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="index">数据位置</param>
        /// <returns>被移除数据</returns>
        
        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)Length) throw new IndexOutOfRangeException("index[" + index.toString() + "] >= length[" + Length.toString() + "]");
            removeAt(index);
        }
        /// <summary>
        /// 移除数据并使用最后一个数据移动到当前位置
        /// </summary>
        /// <param name="index">数据位置</param>
        
        public void RemoveAtEnd(int index)
        {
            if ((uint)index >= (uint)Length) throw new IndexOutOfRangeException("index[" + index.toString() + "] >= length[" + Length.toString() + "]");
            Array[index] = Array[--Length];
            Array[Length] = default(valueType);
        }
        /// <summary>
        /// 转换数组
        /// </summary>
        /// <returns>数组</returns>
        
        private valueType[] getArray()
        {
            valueType[] values = new valueType[Length];
            System.Array.Copy(Array, 0, values, 0, Length);
            return values;
        }
        /// <summary>
        /// 转换数组
        /// </summary>
        /// <returns>数组</returns>
        
        public valueType[] ToArray()
        {
            return Length == 0 ? NullValue<valueType>.Array : (Length == Array.Length ? Array : getArray());
        }
    }

    /// <summary>
    /// 单向动态数组
    /// </summary>
    /// <typeparam name="valueType">数据类型</typeparam>
    public partial class ListArray<valueType> : IList<valueType>
    {
        /// <summary>
        /// 数据数量
        /// </summary>
        public int Count
        {
            get { return Length; }
        }
        /// <summary>
        /// 清除所有数据
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl((System.Runtime.CompilerServices.MethodImplOptions)fastCSharp.Pub.MethodImplOptionsAggressiveInlining)]
        public void Clear()
        {
            if (Array.Length != 0)
            {
                if (IsClearArray) System.Array.Clear(Array, 0, Array.Length);
                Length = 0;
            }
        }
        /// <summary>
        /// 判断是否存在数据
        /// </summary>
        /// <param name="value">匹配数据</param>
        /// <returns>是否存在数据</returns>
        [System.Runtime.CompilerServices.MethodImpl((System.Runtime.CompilerServices.MethodImplOptions)fastCSharp.Pub.MethodImplOptionsAggressiveInlining)]
        public bool Contains(valueType value)
        {
            return IndexOf(value) != -1;
        }
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="value">数据</param>
        public void Insert(int index, valueType value)
        {
            if ((uint)index >= (uint)Length) throw new IndexOutOfRangeException("index[" + index.toString() + "] >= length[" + Length.toString() + "]");
            if (index == Length) Add(value);
            else
            {
                if (Length != Array.Length)
                {
                    fastCSharp.Extension.ArrayExtension.MoveNotNull(Array, index, index + 1, Length - index);
                    Array[index] = value;
                    ++Length;
                }
                else
                {
                    valueType[] values = GetNewArray(Array.Length << 1);
                    System.Array.Copy(Array, 0, values, 0, index);
                    values[index] = value;
                    System.Array.Copy(Array, index, values, index + 1, Length++ - index);
                    Array = values;
                }
            }
        }
        /// <summary>
        /// 复制数据
        /// </summary>
        /// <param name="values">目标数据</param>
        /// <param name="index">目标位置</param>
        [System.Runtime.CompilerServices.MethodImpl((System.Runtime.CompilerServices.MethodImplOptions)fastCSharp.Pub.MethodImplOptionsAggressiveInlining)]
        public void CopyTo(valueType[] values, int index)
        {
            if (index < 0) throw new IndexOutOfRangeException("index[" + index.toString() + "] < 0");
            if (Length + index > values.Length) throw new IndexOutOfRangeException("Length" + Length.toString() + " + index" + index.toString() + " > values.Length[" + values.Length.toString() + "]");
            System.Array.Copy(Array, 0, values, index, Length);
        }
    }
}
