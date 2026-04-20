using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.Interfaces;

public interface IDataFlow
{
    Task HandleAsync(DataContext context, CancellationToken ct = default);
}
