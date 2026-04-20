using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.Interfaces;

public interface IScheduler
{
    Task AddRequestAsync(Request request, CancellationToken ct = default);
    
    Task<IEnumerable<Request>> GetRequestsAsync(int count, CancellationToken ct = default);
    
    Task<bool> IsDuplicateAsync(Request request, CancellationToken ct = default);
}
