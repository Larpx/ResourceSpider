using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Larpx.PersonalTools.ResourceSpider.Core.Interfaces;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Duplicate;

/// <summary>
/// 基于 Redis 的分布式去重器实现，使用 Redis 的键存在性判断实现跨进程去重
/// 适用于分布式部署场景，去重数据持久化在 Redis 中
/// </summary>
public class RedisDuplicateRemover : IDuplicateRemover, IDisposable
{
    private readonly IDatabase _database;
    private readonly string _keyPrefix;
    private readonly ILogger<RedisDuplicateRemover>? _logger;
    private bool _disposed;

    /// <summary>
    /// 初始化 Redis 去重器
    /// </summary>
    /// <param name="redis">Redis 连接复用器</param>
    /// <param name="keyPrefix">键前缀，用于隔离不同爬虫的去重数据</param>
    /// <param name="logger">日志记录器</param>
    public RedisDuplicateRemover(
        IConnectionMultiplexer redis,
        string keyPrefix = "request:dedup",
        ILogger<RedisDuplicateRemover>? logger = null)
    {
        _database = redis.GetDatabase();
        _keyPrefix = keyPrefix;
        _logger = logger;
    }

    /// <summary>
    /// 判断指定指纹的请求是否已存在于 Redis 中
    /// </summary>
    /// <param name="fingerprint">请求指纹</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>已存在返回 true，否则返回 false</returns>
    public async Task<bool> IsDuplicateAsync(string fingerprint, CancellationToken ct = default)
    {
        var key = $"{_keyPrefix}:{fingerprint}";
        return await _database.KeyExistsAsync(key);
    }

    /// <summary>
    /// 将请求指纹存储到 Redis 中，设置 24 小时过期时间
    /// </summary>
    /// <param name="fingerprint">请求指纹</param>
    /// <param name="ct">取消令牌</param>
    public async Task AddAsync(string fingerprint, CancellationToken ct = default)
    {
        var key = $"{_keyPrefix}:{fingerprint}";
        await _database.StringSetAsync(key, "1", TimeSpan.FromHours(24));
    }

    /// <summary>
    /// 获取 Redis 中当前去重集合的请求数量
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>已记录的请求数量</returns>
    public async Task<long> GetCountAsync(CancellationToken ct = default)
    {
        var server = GetServer();
        var keys = server.Keys(pattern: $"{_keyPrefix}:*").Count();
        return keys;
    }

    /// <summary>
    /// 获取 Redis 服务器实例
    /// </summary>
    /// <returns>Redis 服务器实例</returns>
    private IServer GetServer()
    {
        var endpoints = _database.Multiplexer.GetEndPoints();
        return _database.Multiplexer.GetServer(endpoints[0]);
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
