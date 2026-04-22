using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.Interfaces;

/// <summary>
/// 存储接口，定义爬取结果的持久化契约
/// </summary>
public interface IStorage
{
    /// <summary>
    /// 异步存储数据记录集合
    /// </summary>
    /// <param name="records">要存储的数据记录集合</param>
    /// <param name="ct">取消令牌</param>
    Task StoreAsync(IEnumerable<DataRecord> records, CancellationToken ct = default);
}
