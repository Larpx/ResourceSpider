using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ResourceSpider.Core.DataFlow;

namespace ResourceSpider.Infrastructure.DataFlow;

public abstract class DataFlowBase : IDataFlow
{
    protected ILogger Logger { get; private set; } = null!;

    public void SetLogger(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public abstract Task InitializeAsync();
    public abstract Task HandleAsync(DataFlowContext context, ResponseDelegate next);

    protected virtual bool IsNullOrEmpty(DataFlowContext context) => context.IsEmpty;

    public virtual void Dispose() { }
}
