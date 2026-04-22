using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Storage;

/// <summary>
/// 数据库存储实现，通过外部委托函数将数据记录存储到数据库
/// 适用于需要自定义数据库存储逻辑的场景
/// </summary>
public class DatabaseStorage : IStorage
{
    /// <summary>
    /// 数据存储委托函数，由调用方提供具体的数据库写入逻辑
    /// </summary>
    private readonly Func<IEnumerable<DataRecord>, CancellationToken, Task> _storeFunc;

    /// <summary>
    /// 初始化数据库存储实例
    /// </summary>
    /// <param name="storeFunc">数据存储委托函数，接收数据记录集合和取消令牌</param>
    public DatabaseStorage(Func<IEnumerable<DataRecord>, CancellationToken, Task> storeFunc)
    {
        _storeFunc = storeFunc;
    }

    /// <summary>
    /// 处理数据上下文，如果包含数据记录则执行存储操作
    /// </summary>
    /// <param name="context">数据上下文，包含待存储的数据记录</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    public Task HandleAsync(DataContext context, CancellationToken ct = default)
    {
        if (context?.DataRecords.Any() == true)
        {
            return StoreAsync(context.DataRecords, ct);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 将数据记录集合存储到数据库
    /// </summary>
    /// <param name="records">待存储的数据记录集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    public async Task StoreAsync(IEnumerable<DataRecord> records, CancellationToken ct = default)
    {
        await _storeFunc(records, ct);
    }
}
