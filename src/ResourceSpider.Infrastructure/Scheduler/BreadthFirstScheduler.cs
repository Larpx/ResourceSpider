using System.Collections.Concurrent;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Scheduler;

public class BreadthFirstScheduler : IScheduler
{
    private readonly ConcurrentQueue<Request> _queue = new();
    private readonly IDuplicateRemover _duplicateRemover;
    private string _spiderId;

    public BreadthFirstScheduler(IDuplicateRemover duplicateRemover)
    {
        _duplicateRemover = duplicateRemover;
    }

    public Task InitializeAsync(string spiderId = null, CancellationToken ct = default)
    {
        _spiderId = spiderId;
        return Task.CompletedTask;
    }

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

    public Task<IEnumerable<Request>> DequeueAsync(int count, CancellationToken ct = default)
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
