using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ResourceSpider.Core.Interfaces;

namespace ResourceSpider.Infrastructure.Duplicate;

public class RedisDuplicateRemover : IDuplicateRemover, IDisposable
{
    private readonly IDatabase _database;
    private readonly string _keyPrefix;
    private readonly ILogger<RedisDuplicateRemover>? _logger;
    private bool _disposed;

    public RedisDuplicateRemover(
        IConnectionMultiplexer redis, 
        string keyPrefix = "request:dedup",
        ILogger<RedisDuplicateRemover>? logger = null)
    {
        _database = redis.GetDatabase();
        _keyPrefix = keyPrefix;
        _logger = logger;
    }

    public async Task<bool> IsDuplicateAsync(string fingerprint, CancellationToken ct = default)
    {
        var key = $"{_keyPrefix}:{fingerprint}";
        return await _database.KeyExistsAsync(key);
    }

    public async Task AddAsync(string fingerprint, CancellationToken ct = default)
    {
        var key = $"{_keyPrefix}:{fingerprint}";
        await _database.StringSetAsync(key, "1", TimeSpan.FromHours(24));
    }

    public async Task<long> GetCountAsync(CancellationToken ct = default)
    {
        var server = GetServer();
        var keys = server.Keys(pattern: $"{_keyPrefix}:*").Count();
        return keys;
    }

    private IServer GetServer()
    {
        var endpoints = _database.Multiplexer.GetEndPoints();
        return _database.Multiplexer.GetServer(endpoints[0]);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
