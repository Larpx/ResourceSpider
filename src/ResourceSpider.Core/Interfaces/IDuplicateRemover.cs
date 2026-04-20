namespace ResourceSpider.Core.Interfaces;

public interface IDuplicateRemover
{
    Task<bool> IsDuplicateAsync(string fingerprint, CancellationToken ct = default);
    
    Task AddAsync(string fingerprint, CancellationToken ct = default);
    
    Task<long> GetCountAsync(CancellationToken ct = default);
}
