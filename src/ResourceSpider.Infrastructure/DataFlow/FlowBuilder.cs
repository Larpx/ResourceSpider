using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ResourceSpider.Core.DataFlow;

namespace ResourceSpider.Infrastructure.DataFlow;

public class FlowBuilder
{
    private readonly List<Func<ResponseDelegate, ResponseDelegate>> _components = [];
    private readonly List<Func<IServiceProvider, IDataFlow>> _flowFactories = [];

    public async Task<(string, ResponseDelegate)> BuildAsync()
    {
        var info = await InitializeAsync();
        var requestDelegate = (ResponseDelegate)(context =>
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<FlowBuilder>>();
            logger.LogDebug("Start data flow");
            return Task.CompletedTask;
        });
        for (var index = _components.Count - 1; index >= 0; --index)
        {
            var middleware = _components[index];
            requestDelegate = middleware(requestDelegate);
        }
        return (info, requestDelegate);
    }

    public void AddFlow<T>() where T : IDataFlow
    {
        _flowFactories.Add(sp => (IDataFlow)ActivatorUtilities.CreateInstance<T>(sp));
        _components.Add(@delegate => CreateMiddleware(() => (IDataFlow)ActivatorUtilities.CreateInstance<T>(
            default!), @delegate));
    }

    public void AddFlow(Func<IDataFlow> factory)
    {
        _flowFactories.Add(_ => factory());
        _components.Add(@delegate => CreateMiddleware(factory, @delegate));
    }

    private ResponseDelegate CreateMiddleware(Func<IDataFlow> factory, ResponseDelegate next)
    {
        return async context =>
        {
            var middleware = factory();
            if (middleware is DataFlowBase baseFlow)
            {
                var logger = context.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(middleware.GetType());
                baseFlow.SetLogger(logger);
            }
            await middleware.InitializeAsync();
            await middleware.HandleAsync(context, next);
        };
    }

    private Task<string> InitializeAsync()
    {
        if (_flowFactories.Count == 0) return Task.FromResult("Empty data flow chain");
        var pre = "DataFlow full chain: ";
        var flowInfo = new StringBuilder(pre);
        foreach (var factory in _flowFactories)
        {
            var dataFlow = factory(null!);
            if (dataFlow == null) continue;
            if (flowInfo.Length == pre.Length) flowInfo.Append(dataFlow.GetType().Name);
            else flowInfo.Append(" -> ").Append(dataFlow.GetType().Name);
        }
        return Task.FromResult(flowInfo.ToString());
    }
}
