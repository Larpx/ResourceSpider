using ResourceSpider.Core.Interfaces;

namespace ResourceSpider.Infrastructure.Duplicate;

public class HashSetDuplicateRemover : IDuplicateRemover
{
    private readonly HashSet<string> _fingerprints = new();
    private readonly object _lock = new();

    public Task<bool> IsDuplicateAsync(string fingerprint, CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_fingerprints.Contains(fingerprint));
        }
    }

    public Task AddAsync(string fingerprint, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _fingerprints.Add(fingerprint);
        }
        return Task.CompletedTask;
    }

    public Task<long> GetCountAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult((long)_fingerprints.Count);
        }
    }
}
