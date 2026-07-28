using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Larpx.PersonalTools.ResourceSpider.Core.Interfaces;
using Larpx.PersonalTools.ResourceSpider.Core.Models;
using ProxyModel = Larpx.PersonalTools.ResourceSpider.Core.Models.Proxy;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Proxy;

/// <summary>
/// 代理供应器接口，定义获取代理列表的方法
/// 用于从外部数据源获取可用代理
/// </summary>
public interface IProxySupplier
{
    /// <summary>
    /// 从外部数据源获取代理列表
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>代理对象集合</returns>
    Task<IEnumerable<ProxyModel>> GetProxiesAsync(CancellationToken ct = default);
}

/// <summary>
/// 代理验证器接口，定义验证代理可用性的方法
/// </summary>
public interface IProxyValidator
{
    /// <summary>
    /// 验证指定代理是否可用
    /// </summary>
    /// <param name="proxy">待验证的代理对象</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>代理可用返回 true，否则返回 false</returns>
    Task<bool> IsValidAsync(ProxyModel proxy, CancellationToken ct = default);
}

/// <summary>
/// 代理检测器配置选项
/// </summary>
public class ProxyDetectorOptions
{
    /// <summary>
    /// 用于验证代理的测试 URL
    /// </summary>
    public string TestUrl { get; set; } = "https://httpbin.org/ip";

    /// <summary>
    /// 代理验证请求超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// 并发验证代理的数量，默认为 5
    /// </summary>
    public int ConcurrentCount { get; set; } = 5;
}

/// <summary>
/// 代理检测器，负责验证代理的可用性并更新代理池状态
/// 支持单个代理检测和批量并发检测
/// </summary>
public class ProxyDetector
{
    /// <summary>
    /// 代理验证器实例
    /// </summary>
    private readonly IProxyValidator _validator;

    /// <summary>
    /// 代理池实例，用于存储验证通过的代理
    /// </summary>
    private readonly IProxyPool _proxyPool;

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<ProxyDetector> _logger;

    /// <summary>
    /// 代理检测器配置选项
    /// </summary>
    private readonly ProxyDetectorOptions _options;

    /// <summary>
    /// 初始化代理检测器
    /// </summary>
    /// <param name="validator">代理验证器</param>
    /// <param name="proxyPool">代理池</param>
    /// <param name="options">配置选项</param>
    /// <param name="logger">日志记录器</param>
    public ProxyDetector(
        IProxyValidator validator,
        IProxyPool proxyPool,
        IOptions<ProxyDetectorOptions> options,
        ILogger<ProxyDetector> logger)
    {
        _validator = validator;
        _proxyPool = proxyPool;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// 检测单个代理的可用性，验证通过则添加到代理池
    /// 验证失败则增加失败计数
    /// </summary>
    /// <param name="proxy">待检测的代理对象</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    public async Task DetectAsync(ProxyModel proxy, CancellationToken ct = default)
    {
        var isValid = await _validator.IsValidAsync(proxy, ct);
        
        proxy.LastCheckedAt = DateTime.UtcNow;
        proxy.IsAvailable = isValid;

        if (isValid)
        {
            await _proxyPool.AddProxyAsync(proxy, ct);
            _logger.LogInformation("Proxy {ProxyId} is available", proxy.ProxyId);
        }
        else
        {
            proxy.FailureCount++;
            _logger.LogWarning("Proxy {ProxyId} is unavailable", proxy.ProxyId);
        }
    }

    /// <summary>
    /// 批量并发检测代理列表的可用性
    /// 使用信号量控制并发数量，避免过多并发请求
    /// </summary>
    /// <param name="proxies">待检测的代理集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    public async Task DetectAllAsync(IEnumerable<ProxyModel> proxies, CancellationToken ct = default)
    {
        var semaphore = new SemaphoreSlim(_options.ConcurrentCount);
        var tasks = proxies.Select(async proxy =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                await DetectAsync(proxy, ct);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }
}
