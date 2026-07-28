using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Core.Interfaces;

/// <summary>
/// 数据上下文，封装单次请求处理过程中的响应数据、提取记录和上下文信息
/// </summary>
public class DataContext
{
    /// <summary>
    /// 当前请求的响应数据
    /// </summary>
    public Response? Response { get; set; }

    /// <summary>
    /// 从响应中提取的数据记录列表
    /// </summary>
    public List<DataRecord> DataRecords { get; set; } = new();

    /// <summary>
    /// 上下文附加项，用于在处理阶段之间传递额外数据
    /// </summary>
    public Dictionary<string, object?> Items { get; set; } = new();

    /// <summary>
    /// 关联的任务标识
    /// </summary>
    public string? TaskId { get; set; }

    /// <summary>
    /// 关联的请求标识
    /// </summary>
    public string? RequestId { get; set; }
}
