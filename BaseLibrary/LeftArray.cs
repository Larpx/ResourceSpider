using System;
using System.Collections.Generic;
using Larpx.ResourceSpider.BaseLibrary.Extension;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Larpx.ResourceSpider.BaseLibrary
{
    /// <summary>
    /// 数组子串
    /// </summary>
    /// <typeparam name="valueType"></typeparam>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    public partial struct LeftArray<valueType> : IList<valueType>
    {
        /// <summary>
        /// 默认数组长度
        /// </summary>
        private const int defalutArraySize = sizeof(int);
        /// <summary>
        /// 原数组
        /// </summary>
        internal valueType[] Array;
        /// <summary>
        /// 长度
        /// </summary>
        internal int Length;
        /// <summary>
        /// 长度
        /// </summary>
        public int Count
        {
            get { return Length; }
        }
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
                if ((uint)index < (uint)Length)
                {
                    Array[index] = value;
                    return;
                }
                throw new IndexOutOfRangeException("index[" + index.toString() + "] >= Length[" + Length.toString() + "]");
            }
        }
        /// <summary>
        /// 只读
        /// </summary>
        public bool IsReadOnly { get { return false; } }
        /// <summary>
        /// 数组子串
        /// </summary>
        /// <param name="size">容器大小</param>
        public LeftArray(int size)
        {
            Array = size > 0 ? new valueType[size] : null;
            Length = 0;
        }
        /// <summary>
        /// 数组子串
        /// </summary>
        /// <param name="value">数组</param>
        public LeftArray(valueType[] value)
        {
            Array = value;
            Length = value == null ? 0 : value.Length;
        }
        /// <summary>
        /// 枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        
        IEnumerator<valueType> IEnumerable<valueType>.GetEnumerator()
        {
            if (Length != 0) return new Enumerator<valueType>.Array(this);
            return Enumerator<valueType>.Empty;
        }
        /// <summary>
        /// 枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (Length != 0) return new Enumerator<valueType>.Array(this);
            return Enumerator<valueType>.Empty;
        }
        /// <summary>
        /// 置空并释放数组
        /// </summary>
        
        public void SetNull()
        {
            Array = null;
            Length = 0;
        }
        /// <summary>
        /// 置空并释放数组
        /// </summary>
        /// <returns></returns>
        
        internal valueType[] GetNull()
        {
            valueType[] array = Array;
            SetNull();
            return array;
        }
        /// <summary>
        /// 置空并释放数组
        /// </summary>
        /// <param name="array"></param>
        /// <param name="length"></param>
        
        internal void GetNull(ref valueType[] array, ref int length)
        {
            array = Array;
            length = Length;
            SetNull();
        }
        /// <summary>
        /// 数组互换
        /// </summary>
        /// <param name="value"></param>
        
        internal void Exchange(ref LeftArray<valueType> value)
        {
            LeftArray<valueType> temp = value;
            value = this;
            this = temp;
        }
        /// <summary>
        /// 重置数据
        /// </summary>
        /// <param name="value">数组,不能为null</param>
        
        internal void Set(valueType[] value)
        {
            Array = value;
            Length = value.Length;
        }
        /// <summary>
        /// 重置数据
        /// </summary>
        /// <param name="value">数组,不能为null</param>
        /// <param name="length">长度,必须合法</param>
        
        internal void Set(valueType[] value, int length)
        {
            Array = value;
            Length = length;
        }
        /// <summary>
        /// 设置数据容器长度
        /// </summary>
        /// <param name="count">数据长度</param>
        
        private void setLength(int count)
        {
            valueType[] newArray = DynamicArray<valueType>.GetNewArray(count);
            System.Array.Copy(Array, 0, newArray, 0, Length);
            Array = newArray;
        }
        /// <summary>
        /// 增加数据长度
        /// </summary>
        /// <param name="length">数据长度</param>
        
        private void addToLength(int length)
        {
            if (Array == null) Array = new valueType[length < defalutArraySize ? defalutArraySize : length];
            else if (length > Array.Length) setLength(length);
        }
        /// <summary>
        /// 预增长度
        /// </summary>
        /// <param name="length"></param>
        
        internal void PrepLength(int length)
        {
            if (Array == null) Array = new valueType[length < defalutArraySize ? defalutArraySize : length];
            else if ((length += this.Length) > Array.Length) setLength(Math.Max(length, Array.Length << 1));
        }
        /// <summary>
        /// 清除所有数据
        /// </summary>
        
        public void Clear()
        {
            if (Array != null)
            {
                if (DynamicArray<valueType>.IsClearArray) System.Array.Clear(Array, 0, Array.Length);
                Length = 0;
            }
        }
        /// <summary>
        /// 清除当前长度有效数据
        /// </summary>
        
        public void ClearOnlyLength()
        {
            if (Array != null)
            {
                if (DynamicArray<valueType>.IsClearArray) System.Array.Clear(Array, 0, Length);
                Length = 0;
            }
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据</param>
        public void Add(valueType value)
        {
            if (Array == null)
            {
                Array = new valueType[defalutArraySize];
                Array[0] = value;
                Length = 1;
            }
            else
            {
                if (Length == Array.Length)
                {
                    if (Length == 0) Array = new valueType[defalutArraySize];
                    else setLength(Length << 1);
                }
                Array[Length++] = value;
            }
        }
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        public void Add(ICollection<valueType> values)
        {
            int count = values.Count;
            if (count != 0)
            {
                addToLength(Length + count);
                foreach (valueType value in values) Array[Length++] = value;
            }
        }
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="value">数据</param>
        public void Insert(int index, valueType value)
        {
            if ((uint)index > (uint)Length) throw new IndexOutOfRangeException("index[" + index.toString() + "] > Length[" + Length.toString() + "]");
            if (index == Length)
            {
                Add(value);
                return;
            }
            if (Length == Array.Length)
            {
                valueType[] values = DynamicArray<valueType>.GetNewArray(Length << 1);
                System.Array.Copy(Array, 0, values, 0, index);
                values[index] = value;
                System.Array.Copy(Array, index, values, index + 1, Length++ - index);
                Array = values;
            }
            else
            {
                Extension.ArrayExtension.MoveNotNull(Array, index, index + 1, Length - index);
                Array[index] = value;
                ++Length;
            }
        }
        /// <summary>
        /// 判断是否存在数据
        /// </summary>
        /// <param name="value">匹配数据</param>
        /// <returns>是否存在数据</returns>
        
        public bool Contains(valueType value)
        {
            return IndexOf(value) != -1;
        }
        /// <summary>
        /// 获取匹配数据位置
        /// </summary>
        /// <param name="value">匹配数据</param>
        /// <returns>匹配位置,失败为-1</returns>
        
        public int IndexOf(valueType value)
        {
            return Length == 0 ? -1 : System.Array.IndexOf(Array, value, 0, Length);
        }
        /// <summary>
        /// 获取获取数组中的匹配位置
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>数组中的匹配位置,失败为-1</returns>
        
        private int indexOf(Func<valueType, bool> isValue)
        {
            int index = 0;
            foreach (valueType value in Array)
            {
                if (isValue(value)) return index;
                if (++index == Length) return -1;
            }
            return -1;
        }
        /// <summary>
        /// 获取获取数组中的匹配位置
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>数组中的匹配位置,失败为-1</returns>
        
        public int IndexOf(Func<valueType, bool> isValue)
        {
            return Length == 0 ? -1 : indexOf(isValue);
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
                RemoveAt(index);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="index">数据位置</param>
        /// <returns>被移除数据</returns>
        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)Length) throw new IndexOutOfRangeException("index[" + index.toString() + "] >= Length[" + Length.toString() + "]");
            Extension.ArrayExtension.MoveNotNull(Array, index + 1, index, --Length - index);
            Array[Length] = default(valueType);
        }
        /// <summary>
        /// 最后一个数据移动到被删除数据位置
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool removeAtToEnd(int index)
        {
            if (index >= 0)
            {
                if (index != --Length) Array[index] = Array[Length];
                Array[Length] = default(valueType);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 移除第一个匹配数据，然后将最后一个数据移动到被删除数据位置
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>是否存在移除数据</returns>
        
        public bool RemoveToEnd(Func<valueType, bool> isValue)
        {
            return removeAtToEnd(IndexOf(isValue));
        }
        /// <summary>
        /// 弹出最后一个数据
        /// </summary>
        /// <returns></returns>
        
        internal valueType UnsafePop()
        {
            valueType value = Array[--Length];
            Array[Length] = default(valueType);
            return value;
        }
        /// <summary>
        /// 复制数据
        /// </summary>
        /// <param name="values">目标数据</param>
        /// <param name="index">目标位置</param>
        public void CopyTo(valueType[] values, int index)
        {
            if (index < 0) throw new IndexOutOfRangeException("index[" + index.toString() + "]");
            if (Length + index > values.Length) throw new IndexOutOfRangeException("Length + index[" + (Length + index).toString() + "] > values.Length[" + values.Length.toString() + "]");
            if (Length != 0) System.Array.Copy(Array, 0, values, index, Length);
        }
        /// <summary>
        /// 转换数组
        /// </summary>
        /// <returns>数组</returns>
        
        public valueType[] ToArray()
        {
            if (Length == 0) return NullValue<valueType>.Array;
            return Length == Array.Length ? Array : getArray();
        }
        /// <summary>
        /// 转换数组
        /// </summary>
        /// <returns>数组</returns>
        
        private valueType[] getArray()
        {
            valueType[] newArray = new valueType[Length];
            System.Array.Copy(Array, 0, newArray, 0, Length);
            return newArray;
        }
        /// <summary>
        /// 转换数组
        /// </summary>
        /// <returns>数组</returns>
        
        public valueType[] GetArray()
        {
            return Length != 0 ? getArray() : NullValue<valueType>.Array;
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <returns>排序后的数组</returns>
        
        public LeftArray<valueType> Sort(Func<valueType, valueType, int> comparer)
        {
            Algorithm.QuickSort.Sort(Array, comparer, 0, Length);
            return this;
        }
    }

    /// <summary>
    /// 数组子串
    /// </summary>
    public partial struct LeftArray<valueType>
    {
        /// <summary>
        /// 原数组是否为 null
        /// </summary>
        public bool IsNull
        {
            get { return Array == null; }
        }
        /// <summary>
        /// 数组子串
        /// </summary>
        /// <param name="values">数据集合</param>
        public LeftArray(ICollection<valueType> values)
        {
            Length = 0;
            if (values == null) Array = null;
            else
            {
                int count = values.Count;
                if (count == 0) Array = NullValue<valueType>.Array;
                else
                {
                    Array = new valueType[count];
                    foreach (valueType value in values)
                    {
                        if (--count >= 0) Array[Length++] = value;
                        else Add(value);
                    }
                }
            }
        }
        /// <summary>
        /// 数组子串
        /// </summary>
        /// <param name="values">数据集合</param>
        public LeftArray(IEnumerable<valueType> values)
        {
            Array = null;
            Length = 0;
            if (values != null)
            {
                foreach (valueType value in values) Add(value);
            }
        }
        /// <summary>
        /// 数组子串
        /// </summary>
        /// <param name="array"></param>
        internal LeftArray(ListArray<valueType> array)
        {
            if (array == null)
            {
                Array = null;
                Length = 0;
            }
            else
            {
                Array = array.Array;
                Length = array.Length;
            }
        }
        /// <summary>
        /// 数组子串
        /// </summary>
        /// <param name="length">长度</param>
        /// <param name="value">数组</param>
        internal LeftArray(int length, valueType[] value)
        {
            Array = value;
            Length = length;
        }
        /// <summary>
        /// 长度设为0（注意：对于引用类型没有置 0 可能导致内存泄露）
        /// </summary>

        public void Empty()
        {
            Length = 0;
        }
        /// <summary>
        /// 返回非 null 数组  
        /// </summary>
        /// <returns></returns>
        internal LeftArray<valueType> NotNull()
        {
            if (Array == null) return new LeftArray<valueType>(NullValue<valueType>.Array);
            return this;
        }
        /// <summary>
        /// 获取最后一个值
        /// </summary>
        internal valueType UnsafeLast
        {
            get { return Array[Length - 1]; }
        }
        /// <summary>
        /// 获取最后一个值
        /// </summary>
        /// <returns>最后一个值,失败为default(valueType)</returns>

        public valueType LastOrDefault()
        {
            return Length != 0 ? Array[Length - 1] : default(valueType);
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据</param>

        internal void UnsafeAdd(valueType value)
        {
            Array[Length++] = value;
        }
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="array">数据集合</param>
        public void Add(valueType[] array)
        {
            int count = array.length();
            if (count != 0)
            {
                addToLength(Length + count);
                System.Array.Copy(array, 0, Array, Length, count);
                Length += count;
            }
        }
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        public void Add(ref LeftArray<valueType> values)
        {
            if (values.Length != 0)
            {
                addToLength(Length + values.Length);
                System.Array.Copy(values.Array, 0, Array, Length, values.Length);
                Length += values.Length;
            }
        }
        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <param name="values">数据集合</param>
        public void Add(ListArray<valueType> values)
        {
            if (values != null && values.Length != 0)
            {
                addToLength(Length + values.Length);
                System.Array.Copy(values.Array, 0, Array, Length, values.Length);
                Length += values.Length;
            }
        }

        /// <summary>
        /// 添加数据集合
        /// </summary>
        /// <typeparam name="collectionValueType">集合数据类型</typeparam>
        /// <param name="values">数据集合</param>
        /// <param name="getValue">获取数据委托</param>
        public void Add<collectionValueType>(ICollection<collectionValueType> values, Func<collectionValueType, valueType> getValue)
        {
            int count = values.Count;
            if (count != 0)
            {
                addToLength(Length + count);
                foreach (collectionValueType value in values)
                {
                    Array[Length] = getValue(value);
                    ++Length;
                }
            }
        }
        /// <summary>
        /// 弹出最后一个数据
        /// </summary>
        /// <returns></returns>

        internal valueType UnsafePopOnly()
        {
            return Array[--Length];
        }
        /// <summary>
        /// 逆转列表
        /// </summary>

        public void Reverse()
        {
            if (Length > 1) System.Array.Reverse(Array, 0, Length);
        }
        /// <summary>
        /// 转换数组
        /// </summary>
        /// <typeparam name="arrayType">数组类型</typeparam>
        /// <param name="getValue">数据获取委托</param>
        /// <returns>数组</returns>
        public arrayType[] GetArray<arrayType>(Func<valueType, arrayType> getValue)
        {
            if (Length == 0) return NullValue<arrayType>.Array;
            arrayType[] newArray = new arrayType[Length];
            int index = 0;
            do
            {
                newArray[index] = getValue(Array[index]);
            }
            while (++index != Length);
            return newArray;
        }
        /// <summary>
        /// 获取第一个匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配值,失败为 default(valueType)</returns>

        public valueType FirstOrDefault(Func<valueType, bool> isValue)
        {
            int index = indexOf(isValue);
            return index != -1 ? Array[index] : default(valueType);
        }
        /// <summary>
        /// 获取匹配值集合
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配值集合</returns>
        public unsafe valueType[] GetFindArray(Func<valueType, bool> isValue)
        {
            if (Length == 0) return NullValue<valueType>.Array;
            int length = ((Length + 63) >> 6) << 3;
            UnmanagedPool pool = UnmanagedPool.GetDefaultPool(length);
            Pointer.Size buffer = pool.GetSize64(length);
            try
            {
                Memory.ClearUnsafe(buffer.ULong, length >> 3);
                return getFindArray(isValue, new MemoryMap(buffer.Data));
            }
            finally { pool.PushOnly(ref buffer); }
        }
        /// <summary>
        /// 获取匹配值集合
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="map">匹配结果位图</param>
        /// <returns>匹配值集合</returns>
        private valueType[] getFindArray(Func<valueType, bool> isValue, MemoryMap map)
        {
            int count = 0, index = 0;
            foreach (valueType value in Array)
            {
                if (isValue(value))
                {
                    ++count;
                    map.Set(index);
                }
                if (++index == Length) break;
            }
            if (count == 0) return NullValue<valueType>.Array;
            valueType[] values = new valueType[count];
            for (index = Length; count != 0; values[--count] = Array[index])
            {
                while (map.Get(--index) == 0) ;
            }
            return values;
        }
        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <param name="toString">字符串转换器</param>
        /// <param name="join">连接串</param>
        /// <returns>字符串</returns>

        public string JoinString(string join, Func<valueType, string> toString)
        {
            return string.Join(join, GetArray(toString));
        }
        /// <summary>
        /// 设置数据长度并清除其它数据
        /// </summary>
        /// <param name="length"></param>

        private void setLengthClear(int length)
        {
            if (DynamicArray<valueType>.IsClearArray) System.Array.Clear(Array, length, Length - length);
            Length = length;
        }
        /// <summary>
        /// 移除所有后端匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        private void removeEnd(Func<valueType, bool> isValue)
        {
            int index = Length;
            do
            {
                if (isValue(Array[index - 1])) --index;
                else break;
            }
            while (index != 0);
            setLengthClear(index);
        }
        /// <summary>
        /// 移除匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        internal void Remove(Func<valueType, bool> isValue)
        {
            if (Length != 0)
            {
                removeEnd(isValue);
                if (Length != 0)
                {
                    int index = indexOf(isValue);
                    if (index != -1)
                    {
                        for (int read = index; ++read != Length;)
                        {
                            if (!isValue(Array[read])) Array[index++] = Array[read];
                        }
                        setLengthClear(index);
                    }
                }
            }
        }
        /// <summary>
        /// 移除所有后端匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        private void removeEndNot(Func<valueType, bool> isValue)
        {
            int index = Length;
            do
            {
                if (!isValue(Array[index - 1])) --index;
                else break;
            }
            while (index != 0);
            setLengthClear(index);
        }
        /// <summary>
        /// 获取获取数组中的匹配位置
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>数组中的匹配位置,失败为-1</returns>

        private int indexOfNot(Func<valueType, bool> isValue)
        {
            int index = 0;
            foreach (valueType value in Array)
            {
                if (!isValue(value)) return index;
                if (++index == Length) return -1;
            }
            return -1;
        }
        /// <summary>
        /// 移除匹配值
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        internal void RemoveNot(Func<valueType, bool> isValue)
        {
            if (Length != 0)
            {
                removeEndNot(isValue);
                if (Length != 0)
                {
                    int index = indexOfNot(isValue);
                    if (index != -1)
                    {
                        for (int read = index; ++read != Length;)
                        {
                            if (isValue(Array[read])) Array[index++] = Array[read];
                        }
                        setLengthClear(index);
                    }
                }
            }
        }
        /// <summary>
        /// 移除第一个匹配数据，然后将最后一个数据移动到被删除数据位置
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>是否存在移除数据</returns>

        public bool RemoveToEnd(valueType value)
        {
            return removeAtToEnd(IndexOf(value));
        }
        /// <summary>
        /// 移除数据范围
        /// </summary>
        /// <param name="index">起始位置</param>
        /// <param name="count">移除数量</param>

        internal void RemoveRangeOnly(int index, int count)
        {
            Extension.ArrayExtension.MoveNotNull(Array, index + count, index, (Length -= count) - index);
            //if (DynamicArray<valueType>.IsClearArray) System.Array.Clear(Array, Length, count);
        }
    }
}
