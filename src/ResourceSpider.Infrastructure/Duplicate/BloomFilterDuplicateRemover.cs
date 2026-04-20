using System.Collections;
using ResourceSpider.Core.Interfaces;

namespace ResourceSpider.Infrastructure.Duplicate;

public class BloomFilterDuplicateRemover : IDuplicateRemover, IDisposable
{
    private readonly BitArray _bitArray;
    private readonly int _hashCount;
    private readonly object _lock = new();
    private long _count;
    private bool _disposed;

    public BloomFilterDuplicateRemover(int expectedItems, double falsePositiveRate = 0.01)
    {
        var bitSize = OptimalBitSize(expectedItems, falsePositiveRate);
        _hashCount = OptimalHashCount(bitSize, expectedItems);
        _bitArray = new BitArray(bitSize);
    }

    private static int OptimalBitSize(int n, double p)
    {
        return (int)Math.Ceiling(-(n * Math.Log(p)) / Math.Pow(Math.Log(2), 2));
    }

    private static int OptimalHashCount(int m, int n)
    {
        return Math.Max(1, (int)Math.Round((double)m / n * Math.Log(2)));
    }

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

    public Task<long> GetCountAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_count);
        }
    }

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

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
