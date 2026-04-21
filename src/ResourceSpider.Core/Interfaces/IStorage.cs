using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.Interfaces;

public interface IStorage
{
    Task StoreAsync(IEnumerable<DataRecord> records, CancellationToken ct = default);
}
