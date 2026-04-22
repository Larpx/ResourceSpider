namespace ResourceSpider.Core.Models;

/// <summary>
/// 爬取结果模型，存储单次请求提取的结构化数据
/// </summary>
public class CrawlResult
{
    /// <summary>
    /// 结果唯一标识
    /// </summary>
    public string ResultId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 关联的执行记录标识
    /// </summary>
    public string ExecutionId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的任务标识
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的步骤标识
    /// </summary>
    public string? StepId { get; set; }

    /// <summary>
    /// 提取的结构化数据
    /// </summary>
    public Dictionary<string, object?> ExtractedData { get; set; } = new();

    /// <summary>
    /// 数据来源 URL
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
