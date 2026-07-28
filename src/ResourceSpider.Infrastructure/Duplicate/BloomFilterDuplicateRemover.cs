using System.Collections;
using Larpx.PersonalTools.ResourceSpider.Core.Interfaces;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Duplicate;

/// <summary>
/// 基于布隆过滤器的去重器实现，使用位数组和多重哈希函数实现空间高效的去重判断
/// 存在一定的误判率（假阳性），但不会漏判（假阴性），适用于大规模 URL 去重场景
/// </summary>
public class BloomFilterDuplicateRemover : IDuplicateRemover, IDisposable
{
    private readonly BitArray _bitArray;
    private readonly int _hashCount;
    private readonly object _lock = new();
    private long _count;
    private bool _disposed;

    /// <summary>
    /// 初始化布隆过滤器去重器
    /// </summary>
    /// <param name="expectedItems">预期元素数量</param>
    /// <param name="falsePositiveRate">期望的误判率，默认 0.01（1%）</param>
    public BloomFilterDuplicateRemover(int expectedItems, double falsePositiveRate = 0.01)
    {
        var bitSize = OptimalBitSize(expectedItems, falsePositiveRate);
        _hashCount = OptimalHashCount(bitSize, expectedItems);
        _bitArray = new BitArray(bitSize);
    }

    /// <summary>
    /// 计算最优位数组大小
    /// </summary>
    /// <param name="n">预期元素数量</param>
    /// <param name="p">期望误判率</param>
    /// <returns>位数组的最优大小</returns>
    private static int OptimalBitSize(int n, double p)
    {
        return (int)Math.Ceiling(-(n * Math.Log(p)) / Math.Pow(Math.Log(2), 2));
    }

    /// <summary>
    /// 计算最优哈希函数数量
    /// </summary>
    /// <param name="m">位数组大小</param>
    /// <param name="n">预期元素数量</param>
    /// <returns>最优哈希函数数量</returns>
    private static int OptimalHashCount(int m, int n)
    {
        return Math.Max(1, (int)Math.Round((double)m / n * Math.Log(2)));
    }

    /// <summary>
    /// 判断指定指纹是否可能已存在（存在假阳性可能）
    /// </summary>
    /// <param name="fingerprint">请求指纹</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>可能存在返回 true，确定不存在返回 false</returns>
    public Task<bool> IsDuplicateAsync(string fingerprint, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var hashValues = ComputeHashes(fingerprint);
            bool exists = true;
            foreach (var hash in hashValues)
            {
                if (!_bitArray[hash % _bitArray.Length])
                {
                    exists = false;
                    break;
                }
            }
            return Task.FromResult(exists);
        }
    }

    /// <summary>
    /// 将指纹添加到布隆过滤器中
    /// </summary>
    /// <param name="fingerprint">请求指纹</param>
    /// <param name="ct">取消令牌</param>
    public Task AddAsync(string fingerprint, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var hashValues = ComputeHashes(fingerprint);
            foreach (var hash in hashValues)
            {
                _bitArray[hash % _bitArray.Length] = true;
            }
            _count++;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取已添加的元素数量（近似值）
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>已添加的元素数量</returns>
    public Task<long> GetCountAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_count);
        }
    }

    /// <summary>
    /// 计算指纹的多重哈希值，使用 SHA256 和不同盐值生成多个哈希
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>哈希值数组</returns>
    private int[] ComputeHashes(string input)
    {
        var result = new int[_hashCount];
        for (int i = 0; i < _hashCount; i++)
        {
            var hash = System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes($"{input}{i}"));
            result[i] = BitConverter.ToInt32(hash, 0);
        }
        return result;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
