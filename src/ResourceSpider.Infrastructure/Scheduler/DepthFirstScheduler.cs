using System.Collections.Concurrent;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Scheduler;

/// <summary>
/// 深度优先调度器实现，使用并发栈按后进先出顺序调度请求
/// 适用于需要深入遍历页面链接的爬取场景
/// </summary>
public class DepthFirstScheduler : IScheduler
{
    private readonly ConcurrentStack<Request> _stack = new();
    private readonly IDuplicateRemover _duplicateRemover;
    private string _spiderId = string.Empty;

    /// <summary>
    /// 初始化深度优先调度器
    /// </summary>
    /// <param name="duplicateRemover">去重器实例</param>
    public DepthFirstScheduler(IDuplicateRemover duplicateRemover)
    {
        _duplicateRemover = duplicateRemover;
    }

    /// <summary>
    /// 初始化调度器
    /// </summary>
    /// <param name="spiderId">爬虫实例标识</param>
    /// <param name="ct">取消令牌</param>
    public Task InitializeAsync(string? spiderId = null, CancellationToken ct = default)
    {
        _spiderId = spiderId ?? string.Empty;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 将请求批量加入调度栈，自动进行去重判断
    /// </summary>
    /// <param name="requests">要入栈的请求集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>成功入栈的请求数量</returns>
    public async Task<int> EnqueueAsync(IEnumerable<Request> requests, CancellationToken ct = default)
    {
        var count = 0;
        foreach (var request in requests)
        {
            var isDuplicate = await _duplicateRemover.IsDuplicateAsync(
                request.Fingerprint ?? request.RequestId, ct);
            if (!isDuplicate)
            {
                await _duplicateRemover.AddAsync(
                    request.Fingerprint ?? request.RequestId, ct);
                _stack.Push(request);
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 从调度栈中按后进先出顺序批量取出请求
    /// </summary>
    /// <param name="count">要取出的请求数量</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>取出的请求集合</returns>
    public Task<IEnumerable<Request>> DequeueAsync(int count, CancellationToken ct = default)
    {
        var requests = new List<Request>();
        for (int i = 0; i < count && _stack.TryPop(out var request); i++)
        {
            requests.Add(request);
        }
        return Task.FromResult<IEnumerable<Request>>(requests);
    }

    /// <summary>
    /// 判断请求是否重复
    /// </summary>
    /// <param name="request">待检查的请求</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>如果请求已存在返回 true，否则返回 false</returns>
    public async Task<bool> IsDuplicateAsync(Request request, CancellationToken ct = default)
    {
        return await _duplicateRemover.IsDuplicateAsync(
            request.Fingerprint ?? request.RequestId, ct);
    }
}
