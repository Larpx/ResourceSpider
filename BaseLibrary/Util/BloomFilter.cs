using System;
using System.Collections;

namespace Larpx.ResourceSpider.BaseLibrary.Util
{
    public interface IBloomFilter<T>
    {
        void Add(T item);
        bool Contains(T item);
    }

    /// <summary>
    /// Bloom filter.
    /// </summary>
    /// <typeparam name="T">集合类型</typeparam>
    public class BloomFilter<T> : IBloomFilter<T>
    {
        private readonly int _hashFunctionCount;
        private readonly BitArray _hashBits;
        private readonly HashFunction _getHashSecondary;

        /// <summary>
        /// 创建一个新的Bloom过滤器，指定错误率为 1/容量，根据期望的容量和错误率为底层数据结构使用最优大小，以及最优哈希函数数。
        /// 如果类型T是字符串或int，将为您提供二级散列函数。否则将引发异常。如果不使用这些类型，请使用支持自定义散列函数的重载。
        /// </summary>
        /// <param name="capacity">要添加到筛选器的预期项数。可以添加超过此数量的项，但错误率将超过预期值。</param>
        public BloomFilter(int capacity)
            : this(capacity, null)
        {
        }

        /// <summary>
        /// 创建一个新的Bloom过滤器，指定错误率为 1/容量，根据期望的容量和错误率为底层数据结构使用最优大小，以及最优哈希函数数。
        /// 如果类型T是字符串或int，将为您提供二级散列函数。否则将引发异常。如果不使用这些类型，请使用支持自定义散列函数的重载。
        /// </summary>
        /// <param name="capacity">要添加到筛选器的预期项数。可以添加超过此数量的项，但错误率将超过预期值。</param>
        /// <param name="errorRate">可接受的假阳性率（如0.01F=1%）</param>
        public BloomFilter(int capacity, float errorRate)
            : this(capacity, errorRate, null)
        {
        }

        /// <summary>
        /// 创建一个新的Bloom筛选器，指定错误率为1/capacity，使用基于所需容量和错误率的基础数据结构的最佳大小以及哈希函数的最佳数目。
        /// </summary>
        /// <param name="capacity">要添加到筛选器的预期项数。可以添加超过此数量的项，但错误率将超过预期值。</param>
        /// <param name="hashFunction">散列输入值的函数。不要使用GetHashCode（）。如果为空，并且T是string或int，则会为您提供一个哈希函数。</param>
        public BloomFilter(int capacity, HashFunction hashFunction)
            : this(capacity, BestErrorRate(capacity), hashFunction)
        {
        }

        /// <summary>
        /// 创建一个新的Bloom过滤器，指定错误率为 1/容量，根据期望的容量和错误率为底层数据结构使用最优大小，以及最优哈希函数数。
        /// 如果类型T是字符串或int，将为您提供二级散列函数。否则将引发异常。如果不使用这些类型，请使用支持自定义散列函数的重载。
        /// </summary>
        /// <param name="capacity">要添加到筛选器的预期项数。可以添加超过此数量的项，但错误率将超过预期值。</param>
        /// <param name="errorRate">可接受的假阳性率（如0.01F=1%）</param>
        /// <param name="hashFunction">散列输入值的函数。不要使用GetHashCode（）。如果为空，并且T是string或int，则会为您提供一个哈希函数。</param>
        public BloomFilter(int capacity, float errorRate, HashFunction hashFunction)
            : this(capacity, errorRate, hashFunction, BestM(capacity, errorRate), BestK(capacity, errorRate))
        {
        }

        /// <summary>
        /// 创建一个新的Bloom过滤器
        /// </summary>
        /// <param name="capacity">要添加到筛选器的预期项数。可以添加超过此数量的项，但错误率将超过预期值。</param>
        /// <param name="errorRate">可接受的假阳性率（如0.01F=1%）</param>
        /// <param name="hashFunction">散列输入值的函数。不要使用GetHashCode（）。如果为空，并且T是string或int，则会为您提供一个哈希函数。</param>
        /// <param name="m">位数组中的元素数。</param>
        /// <param name="k">要使用的哈希函数数。</param>
        public BloomFilter(int capacity, float errorRate, HashFunction hashFunction, int m, int k)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException("capacity", capacity, "capacity must be > 0");
            }

            if (errorRate >= 1 || errorRate <= 0)
            {
                throw new ArgumentOutOfRangeException("errorRate", errorRate, string.Format("errorRate must be between 0 and 1, exclusive. Was {0}", errorRate));
            }

            if (m < 1)
            {
                throw new ArgumentOutOfRangeException(string.Format("The provided capacity and errorRate values would result in an array of length > int.MaxValue. Please reduce either of these values. Capacity: {0}, Error rate: {1}", capacity, errorRate));
            }

            if (hashFunction == null)
            {
                if (typeof(T) == typeof(string))
                {
                    this._getHashSecondary = HashString;
                }
                else if (typeof(T) == typeof(int))
                {
                    this._getHashSecondary = HashInt32;
                }
                else
                {
                    throw new ArgumentNullException("hashFunction", "Please provide a hash function for your type T, when T is not a string or int.");
                }
            }
            else
            {
                this._getHashSecondary = hashFunction;
            }

            this._hashFunctionCount = k;
            this._hashBits = new BitArray(m);
        }

        /// <summary>
        ///可用于散列输入的函数。
        /// </summary>
        /// <param name="input">The values to be hashed.</param>
        /// <returns>The resulting hash code.</returns>
        public delegate int HashFunction(T input);

        /// <summary>
        /// The ratio of false to true bits in the filter. E.g., 1 true bit in a 10 bit filter means a truthiness of 0.1.
        /// </summary>
        public double Truthiness
        {
            get
            {
                return (double)this.TrueBits() / this._hashBits.Count;
            }
        }

        /// <summary>
        /// Adds a new item to the filter. It cannot be removed.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(T item)
        {
            // start flipping bits for each hash of item
            var primaryHash = item.GetHashCode();
            var secondaryHash = this._getHashSecondary(item);
            for (var i = 0; i < this._hashFunctionCount; i++)
            {
                var hash = this.ComputeHash(primaryHash, secondaryHash, i);
                this._hashBits[hash] = true;
            }
        }

        /// <summary>
        /// Checks for the existance of the item in the filter for a given probability.
        /// </summary>
        /// <param name="item"> The item. </param>
        /// <returns> The <see cref="bool"/>. </returns>
        public bool Contains(T item)
        {
            var primaryHash = item.GetHashCode();
            var secondaryHash = this._getHashSecondary(item);
            for (var i = 0; i < this._hashFunctionCount; i++)
            {
                var hash = this.ComputeHash(primaryHash, secondaryHash, i);
                if (this._hashBits[hash] == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The best k.
        /// </summary>
        /// <param name="capacity"> The capacity. </param>
        /// <param name="errorRate"> The error rate. </param>
        /// <returns> The <see cref="int"/>. </returns>
        private static int BestK(int capacity, float errorRate)
        {
            return (int)Math.Round(Math.Log(2.0) * BestM(capacity, errorRate) / capacity);
        }

        /// <summary>
        /// The best m.
        /// </summary>
        /// <param name="capacity"> The capacity. </param>
        /// <param name="errorRate"> The error rate. </param>
        /// <returns> The <see cref="int"/>. </returns>
        private static int BestM(int capacity, float errorRate)
        {
            return (int)Math.Ceiling(capacity * Math.Log(errorRate, (1.0 / Math.Pow(2, Math.Log(2.0)))));
        }

        /// <summary>
        /// The best error rate.
        /// </summary>
        /// <param name="capacity"> The capacity. </param>
        /// <returns> The <see cref="float"/>. </returns>
        private static float BestErrorRate(int capacity)
        {
            var c = (float)(1.0 / capacity);
            if (c != 0)
            {
                return c;
            }

            // default
            // http://www.cs.princeton.edu/courses/archive/spring02/cs493/lec7.pdf
            return (float)Math.Pow(0.6185, int.MaxValue / capacity);
        }

        /// <summary>
        /// Hashes a 32-bit signed int using Thomas Wang's method v3.1 (http://www.concentric.net/~Ttwang/tech/inthash.htm).
        /// Runtime is suggested to be 11 cycles. 
        /// </summary>
        /// <param name="input">The integer to hash.</param>
        /// <returns>The hashed result.</returns>
        private static int HashInt32(T input)
        {
            var x = input as uint?;
            unchecked
            {
                x = ~x + (x << 15); // x = (x << 15) - x- 1, as (~x) + y is equivalent to y - x - 1 in two's complement representation
                x = x ^ (x >> 12);
                x = x + (x << 2);
                x = x ^ (x >> 4);
                x = x * 2057; // x = (x + (x << 3)) + (x<< 11);
                x = x ^ (x >> 16);
                return (int)x;
            }
        }

        /// <summary>
        /// Hashes a string using Bob Jenkin's "One At A Time" method from Dr. Dobbs (http://burtleburtle.net/bob/hash/doobs.html).
        /// Runtime is suggested to be 9x+9, where x = input.Length. 
        /// </summary>
        /// <param name="input">The string to hash.</param>
        /// <returns>The hashed result.</returns>
        private static int HashString(T input)
        {
            var s = input as string;
            var hash = 0;

            for (var i = 0; i < s.Length; i++)
            {
                hash += s[i];
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }

            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return hash;
        }

        /// <summary>
        /// The true bits.
        /// </summary>
        /// <returns> The <see cref="int"/>. </returns>
        private int TrueBits()
        {
            var output = 0;
            foreach (bool bit in this._hashBits)
            {
                if (bit == true)
                {
                    output++;
                }
            }

            return output;
        }

        /// <summary>
        /// Performs Dillinger and Manolios double hashing. 
        /// </summary>
        /// <param name="primaryHash"> The primary hash. </param>
        /// <param name="secondaryHash"> The secondary hash. </param>
        /// <param name="i"> The i. </param>
        /// <returns> The <see cref="int"/>. </returns>
        private int ComputeHash(int primaryHash, int secondaryHash, int i)
        {
            var resultingHash = (primaryHash + (i * secondaryHash)) % this._hashBits.Count;
            return Math.Abs((int)resultingHash);
        }
    }
}
