using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Core.Interfaces;

/// <summary>
/// 调度器接口，管理爬虫请求的入队、出队和去重
/// </summary>
public interface IScheduler
{
    /// <summary>
    /// 初始化调度器，准备请求队列
    /// </summary>
    /// <param name="spiderId">爬虫实例标识，用于隔离不同爬虫的队列</param>
    /// <param name="ct">取消令牌</param>
    Task InitializeAsync(string? spiderId = null, CancellationToken ct = default);

    /// <summary>
    /// 将请求批量加入调度队列
    /// </summary>
    /// <param name="requests">要入队的请求集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>成功入队的请求数量</returns>
    Task<int> EnqueueAsync(IEnumerable<Request> requests, CancellationToken ct = default);

    /// <summary>
    /// 从调度队列中批量取出请求
    /// </summary>
    /// <param name="count">要取出的请求数量</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>取出的请求集合</returns>
    Task<IEnumerable<Request>> DequeueAsync(int count, CancellationToken ct = default);

    /// <summary>
    /// 判断请求是否重复
    /// </summary>
    /// <param name="request">待检查的请求</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>如果请求已存在返回 true，否则返回 false</returns>
    Task<bool> IsDuplicateAsync(Request request, CancellationToken ct = default);

    /// <summary>
    /// 将单个请求加入调度队列（已弃用，请使用 EnqueueAsync）
    /// </summary>
    [Obsolete("Use EnqueueAsync instead")]
    Task AddRequestAsync(Request request, CancellationToken ct = default)
        => EnqueueAsync(new[] { request }, ct).ContinueWith(_ => { });

    /// <summary>
    /// 从调度队列中取出请求（已弃用，请使用 DequeueAsync）
    /// </summary>
    [Obsolete("Use DequeueAsync instead")]
    Task<IEnumerable<Request>> GetRequestsAsync(int count, CancellationToken ct = default)
        => DequeueAsync(count, ct);
}
