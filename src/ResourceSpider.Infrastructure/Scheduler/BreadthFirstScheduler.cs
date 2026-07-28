using System.Collections.Concurrent;
using Larpx.PersonalTools.ResourceSpider.Core.Interfaces;
using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Scheduler;

/// <summary>
/// 广度优先调度器实现，使用并发队列按先进先出顺序调度请求
/// 适用于需要逐层遍历页面的爬取场景
/// </summary>
public class BreadthFirstScheduler : IScheduler
{
    private readonly ConcurrentQueue<Request> _queue = new();
    private readonly IDuplicateRemover _duplicateRemover;
    private string _spiderId = string.Empty;

    /// <summary>
    /// 初始化广度优先调度器
    /// </summary>
    /// <param name="duplicateRemover">去重器实例</param>
    public BreadthFirstScheduler(IDuplicateRemover duplicateRemover)
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
    /// 将请求批量加入调度队列，自动进行去重判断
    /// </summary>
    /// <param name="requests">要入队的请求集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>成功入队的请求数量</returns>
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
                _queue.Enqueue(request);
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 从调度队列中按先进先出顺序批量取出请求
    /// </summary>
    /// <param name="count">要取出的请求数量</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>取出的请求集合</returns>
    public Task<IEnumerable<Request>> DequeueAsync(int count, CancellationToken ct = default)
    {
        var requests = new List<Request>();
        for (int i = 0; i < count && _queue.TryDequeue(out var request); i++)
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
