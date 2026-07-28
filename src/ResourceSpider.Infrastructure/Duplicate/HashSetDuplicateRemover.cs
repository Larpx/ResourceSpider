using Larpx.PersonalTools.ResourceSpider.Core.Interfaces;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Duplicate;

/// <summary>
/// 基于 HashSet 的内存去重器实现，使用线程安全的哈希集合存储请求指纹
/// 适用于单机场景，数据不持久化，进程重启后去重集合清空
/// </summary>
public class HashSetDuplicateRemover : IDuplicateRemover
{
    private readonly HashSet<string> _fingerprints = new();
    private readonly object _lock = new();

    /// <summary>
    /// 判断指定指纹的请求是否已存在
    /// </summary>
    /// <param name="fingerprint">请求指纹</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>已存在返回 true，否则返回 false</returns>
    public Task<bool> IsDuplicateAsync(string fingerprint, CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_fingerprints.Contains(fingerprint));
        }
    }

    /// <summary>
    /// 将请求指纹添加到去重集合中
    /// </summary>
    /// <param name="fingerprint">请求指纹</param>
    /// <param name="ct">取消令牌</param>
    public Task AddAsync(string fingerprint, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _fingerprints.Add(fingerprint);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取当前去重集合中的请求数量
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>已记录的请求数量</returns>
    public Task<long> GetCountAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult((long)_fingerprints.Count);
        }
    }
}
