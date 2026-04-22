using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ProxyModel = ResourceSpider.Core.Models.Proxy;

namespace ResourceSpider.Infrastructure.Proxy;

/// <summary>
/// 代理池实现，使用并发字典管理可用代理
/// 支持代理的添加、移除、健康度评分和随机选取
/// </summary>
public class ProxyPool : IProxyPool
{
    /// <summary>
    /// 代理存储字典，键为代理 ID，值为代理对象
    /// </summary>
    private readonly ConcurrentDictionary<string, ProxyModel> _proxies = new();

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<ProxyPool> _logger;

    /// <summary>
    /// 随机数生成器，用于随机选取代理
    /// </summary>
    private readonly Random _random = new();

    /// <summary>
    /// 初始化代理池
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public ProxyPool(ILogger<ProxyPool> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 从代理池中随机获取一个可用代理
    /// 仅返回可用且健康分数大于 0.5 的代理
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>可用代理对象，无可用代理时返回 null</returns>
    public Task<ProxyModel?> GetProxyAsync(CancellationToken ct = default)
    {
        var available = _proxies.Values
            .Where(p => p.IsAvailable && p.HealthScore > 0.5)
            .ToList();

        if (!available.Any())
        {
            return Task.FromResult<ProxyModel?>(null);
        }

        var proxy = available[_random.Next(available.Count)];
        return Task.FromResult<ProxyModel?>(proxy);
    }

    /// <summary>
    /// 添加代理到代理池，如果已存在则更新
    /// </summary>
    /// <param name="proxy">代理对象</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    public Task AddProxyAsync(ProxyModel proxy, CancellationToken ct = default)
    {
        _proxies[proxy.ProxyId] = proxy;
        _logger.LogInformation("Added proxy {ProxyId}", proxy.ProxyId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 从代理池中移除指定代理
    /// </summary>
    /// <param name="proxyId">代理 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    public Task RemoveProxyAsync(string proxyId, CancellationToken ct = default)
    {
        _proxies.TryRemove(proxyId, out _);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 标记代理请求成功，增加成功计数并重新计算健康分数
    /// 健康分数低于 0.3 时标记为不可用
    /// </summary>
    /// <param name="proxyId">代理 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    public Task MarkSuccessAsync(string proxyId, CancellationToken ct = default)
    {
        if (_proxies.TryGetValue(proxyId, out var proxy))
        {
            proxy.SuccessCount++;
            proxy.HealthScore = CalculateHealthScore(proxy);
            proxy.IsAvailable = proxy.HealthScore > 0.3;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 标记代理请求失败，增加失败计数并重新计算健康分数
    /// 健康分数低于 0.3 时标记为不可用
    /// </summary>
    /// <param name="proxyId">代理 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    public Task MarkFailureAsync(string proxyId, CancellationToken ct = default)
    {
        if (_proxies.TryGetValue(proxyId, out var proxy))
        {
            proxy.FailureCount++;
            proxy.HealthScore = CalculateHealthScore(proxy);
            proxy.IsAvailable = proxy.HealthScore > 0.3;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取代理池中所有代理的列表
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>代理对象集合</returns>
    public Task<IEnumerable<ProxyModel>> GetAllProxiesAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IEnumerable<ProxyModel>>(_proxies.Values.ToList());
    }

    /// <summary>
    /// 计算代理的健康分数，基于成功次数与总次数的比率
    /// 无请求记录时默认返回 1.0（满分）
    /// </summary>
    /// <param name="proxy">代理对象</param>
    /// <returns>健康分数，范围 0.0 到 1.0</returns>
    private static double CalculateHealthScore(ProxyModel proxy)
    {
        var total = proxy.SuccessCount + proxy.FailureCount;
        if (total == 0) return 1.0;
        return (double)proxy.SuccessCount / total;
    }
}
