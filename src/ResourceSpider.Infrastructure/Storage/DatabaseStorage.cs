using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Storage;

public class DatabaseStorage : IStorage
{
    private readonly Func<IEnumerable<DataRecord>, CancellationToken, Task> _storeFunc;

    public DatabaseStorage(Func<IEnumerable<DataRecord>, CancellationToken, Task> storeFunc)
    {
        _storeFunc = storeFunc;
    }

    public Task HandleAsync(DataContext context, CancellationToken ct = default)
    {
        if (context?.DataRecords.Any() == true)
        {
            return StoreAsync(context.DataRecords, ct);
        }
        return Task.CompletedTask;
    }

    public async Task StoreAsync(IEnumerable<DataRecord> records, CancellationToken ct = default)
    {
        await _storeFunc(records, ct);
    }
}
