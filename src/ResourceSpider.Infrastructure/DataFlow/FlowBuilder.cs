using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Larpx.PersonalTools.ResourceSpider.Core.DataFlow;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.DataFlow;

/// <summary>
/// 数据流构建器，负责组装和构建数据流处理管道
/// 支持通过泛型或工厂方法添加数据流处理器，并按添加顺序构建中间件管道
/// </summary>
public class FlowBuilder
{
    private readonly List<Func<ResponseDelegate, ResponseDelegate>> _components = [];
    private readonly List<Func<IServiceProvider, IDataFlow>> _flowFactories = [];

    /// <summary>
    /// 构建数据流管道，初始化所有处理器并返回管道信息和执行委托
    /// </summary>
    /// <returns>管道描述信息和执行委托的元组</returns>
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

    /// <summary>
    /// 通过泛型添加数据流处理器到管道中
    /// </summary>
    /// <typeparam name="T">数据流处理器类型</typeparam>
    public void AddFlow<T>() where T : IDataFlow
    {
        _flowFactories.Add(sp => (IDataFlow)ActivatorUtilities.CreateInstance<T>(sp));
        _components.Add(@delegate => CreateMiddleware(() => (IDataFlow)ActivatorUtilities.CreateInstance<T>(
            default!), @delegate));
    }

    /// <summary>
    /// 通过工厂方法添加数据流处理器到管道中
    /// </summary>
    /// <param name="factory">数据流处理器工厂方法</param>
    public void AddFlow(Func<IDataFlow> factory)
    {
        _flowFactories.Add(_ => factory());
        _components.Add(@delegate => CreateMiddleware(factory, @delegate));
    }

    /// <summary>
    /// 创建中间件委托，包装数据流处理器的初始化和执行逻辑
    /// </summary>
    /// <param name="factory">数据流处理器工厂方法</param>
    /// <param name="next">下一个处理器的委托</param>
    /// <returns>中间件委托</returns>
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

    /// <summary>
    /// 初始化并生成数据流管道的描述信息
    /// </summary>
    /// <returns>管道描述字符串</returns>
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
