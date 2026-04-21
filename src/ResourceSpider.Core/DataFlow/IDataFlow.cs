using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.DataFlow;

public delegate Task ResponseDelegate(DataFlowContext context);

public interface IDataFlow : IDisposable
{
    Task InitializeAsync();
    Task HandleAsync(DataFlowContext context, ResponseDelegate next);
}
