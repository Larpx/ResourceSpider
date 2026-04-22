using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.Interfaces;

/// <summary>
/// 代理池接口，管理 HTTP 代理的获取、添加、移除和健康状态维护
/// </summary>
public interface IProxyPool
{
    /// <summary>
    /// 从代理池中获取一个可用代理
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>可用代理，无可用代理时返回 null</returns>
    Task<Proxy?> GetProxyAsync(CancellationToken ct = default);

    /// <summary>
    /// 向代理池中添加一个代理
    /// </summary>
    /// <param name="proxy">要添加的代理</param>
    /// <param name="ct">取消令牌</param>
    Task AddProxyAsync(Proxy proxy, CancellationToken ct = default);

    /// <summary>
    /// 从代理池中移除指定代理
    /// </summary>
    /// <param name="proxyId">代理标识</param>
    /// <param name="ct">取消令牌</param>
    Task RemoveProxyAsync(string proxyId, CancellationToken ct = default);

    /// <summary>
    /// 标记代理使用成功，提升其健康分数
    /// </summary>
    /// <param name="proxyId">代理标识</param>
    /// <param name="ct">取消令牌</param>
    Task MarkSuccessAsync(string proxyId, CancellationToken ct = default);

    /// <summary>
    /// 标记代理使用失败，降低其健康分数
    /// </summary>
    /// <param name="proxyId">代理标识</param>
    /// <param name="ct">取消令牌</param>
    Task MarkFailureAsync(string proxyId, CancellationToken ct = default);

    /// <summary>
    /// 获取代理池中所有代理的列表
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>代理列表</returns>
    Task<IEnumerable<Proxy>> GetAllProxiesAsync(CancellationToken ct = default);
}
