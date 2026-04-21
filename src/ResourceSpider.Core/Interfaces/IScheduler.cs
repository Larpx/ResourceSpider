using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.Interfaces;

public interface IScheduler
{
    Task InitializeAsync(string spiderId = null, CancellationToken ct = default);

    Task<int> EnqueueAsync(IEnumerable<Request> requests, CancellationToken ct = default);

    Task<IEnumerable<Request>> DequeueAsync(int count, CancellationToken ct = default);

    Task<bool> IsDuplicateAsync(Request request, CancellationToken ct = default);

    [Obsolete("Use EnqueueAsync instead")]
    Task AddRequestAsync(Request request, CancellationToken ct = default)
        => EnqueueAsync(new[] { request }, ct).ContinueWith(_ => { });

    [Obsolete("Use DequeueAsync instead")]
    Task<IEnumerable<Request>> GetRequestsAsync(int count, CancellationToken ct = default)
        => DequeueAsync(count, ct);
}
