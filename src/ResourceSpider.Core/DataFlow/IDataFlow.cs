using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.DataFlow;

/// <summary>
/// 响应处理委托，用于数据流管道中的中间件模式
/// </summary>
/// <param name="context">数据流上下文</param>
public delegate Task ResponseDelegate(DataFlowContext context);

/// <summary>
/// 数据流接口，定义数据流处理管道中各阶段的通用行为
/// </summary>
public interface IDataFlow : IDisposable
{
    /// <summary>
    /// 初始化数据流，在管道启动前执行
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// 处理数据流上下文，执行当前阶段逻辑后调用下一个管道阶段
    /// </summary>
    /// <param name="context">数据流上下文</param>
    /// <param name="next">下一个管道阶段的委托</param>
    Task HandleAsync(DataFlowContext context, ResponseDelegate next);
}
