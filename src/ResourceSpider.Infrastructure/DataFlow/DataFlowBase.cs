using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ResourceSpider.Core.DataFlow;

namespace ResourceSpider.Infrastructure.DataFlow;

/// <summary>
/// 数据流处理基类，提供日志注入和上下文空值判断的通用实现
/// 所有数据流处理器应继承此基类
/// </summary>
public abstract class DataFlowBase : IDataFlow
{
    /// <summary>
    /// 日志记录器实例
    /// </summary>
    protected ILogger Logger { get; private set; } = null!;

    /// <summary>
    /// 设置日志记录器
    /// </summary>
    /// <param name="logger">日志记录器实例</param>
    /// <exception cref="ArgumentNullException">日志记录器为 null 时抛出</exception>
    public void SetLogger(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 初始化数据流处理器
    /// </summary>
    public abstract Task InitializeAsync();

    /// <summary>
    /// 处理数据流上下文，并调用下一个处理器
    /// </summary>
    /// <param name="context">数据流上下文</param>
    /// <param name="next">下一个处理器的委托</param>
    public abstract Task HandleAsync(DataFlowContext context, ResponseDelegate next);

    /// <summary>
    /// 判断数据流上下文是否为空
    /// </summary>
    /// <param name="context">数据流上下文</param>
    /// <returns>上下文为空返回 true，否则返回 false</returns>
    protected virtual bool IsNullOrEmpty(DataFlowContext context) => context.IsEmpty;

    /// <summary>
    /// 释放资源
    /// </summary>
    public virtual void Dispose() { }
}
