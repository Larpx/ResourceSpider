using System.Collections.Concurrent;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Scheduler;

public class BreadthFirstScheduler : IScheduler
{
    private readonly ConcurrentQueue<Request> _queue = new();
    private readonly IDuplicateRemover _duplicateRemover;

    public BreadthFirstScheduler(IDuplicateRemover duplicateRemover)
    {
        _duplicateRemover = duplicateRemover;
    }

    public async Task AddRequestAsync(Request request, CancellationToken ct = default)
    {
        var isDuplicate = await _duplicateRemover.IsDuplicateAsync(
            request.Fingerprint ?? request.RequestId, ct);
        
        if (!isDuplicate)
        {
            await _duplicateRemover.AddAsync(
                request.Fingerprint ?? request.RequestId, ct);
            _queue.Enqueue(request);
        }
    }

    public Task<IEnumerable<Request>> GetRequestsAsync(int count, CancellationToken ct = default)
    {
        var requests = new List<Request>();
        for (int i = 0; i < count && _queue.TryDequeue(out var request); i++)
        {
            requests.Add(request);
        }
        return Task.FromResult<IEnumerable<Request>>(requests);
    }

    public async Task<bool> IsDuplicateAsync(Request request, CancellationToken ct = default)
    {
        return await _duplicateRemover.IsDuplicateAsync(
            request.Fingerprint ?? request.RequestId, ct);
    }
}
