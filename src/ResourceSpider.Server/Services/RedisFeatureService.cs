using StackExchange.Redis;

namespace Larpx.PersonalTools.ResourceSpider.Server.Services;

/// <summary>
/// Redis 功能开关服务接口，用于描述 Redis 模块的启用状态与运行信息。
/// </summary>
public interface IRedisFeatureService
{
    /// <summary>
    /// 获取 Redis 模块是否启用。
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// 获取系统是否已配置 Redis 连接信息。
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// 获取 Redis 当前连接状态。
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 获取任务内容缓存的过期时间（秒）。
    /// </summary>
    int TaskContentTtlSeconds { get; }

    /// <summary>
    /// 动态设置 Redis 模块启用状态。
    /// </summary>
    /// <param name="enabled">true 表示启用，false 表示停用。</param>
    void SetEnabled(bool enabled);
}

/// <summary>
/// Redis 功能开关服务实现。
/// 负责统一维护 Redis 的可用性、连接状态和缓存相关运行参数。
/// </summary>
public sealed class RedisFeatureService : IRedisFeatureService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly int _taskContentTtlSeconds;
    private volatile bool _enabled;

    /// <summary>
    /// 初始化 Redis 功能开关服务。
    /// </summary>
    /// <param name="enabled">是否启用 Redis 模块。</param>
    /// <param name="configured">是否已配置 Redis 连接。</param>
    /// <param name="taskContentTtlSeconds">任务内容缓存 TTL（秒）。</param>
    /// <param name="redis">Redis 连接复用器，可为空。</param>
    public RedisFeatureService(
        bool enabled,
        bool configured,
        int taskContentTtlSeconds,
        IConnectionMultiplexer? redis)
    {
        _enabled = enabled;
        IsConfigured = configured;
        _taskContentTtlSeconds = Math.Clamp(taskContentTtlSeconds, 30, 3600);
        _redis = redis;
    }

    /// <inheritdoc />
    public bool IsEnabled => _enabled;

    /// <inheritdoc />
    public bool IsConfigured { get; }

    /// <inheritdoc />
    public bool IsConnected => _redis?.IsConnected == true;

    /// <inheritdoc />
    public int TaskContentTtlSeconds => _taskContentTtlSeconds;

    /// <inheritdoc />
    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
    }
}
