namespace ResourceSpider.Core.Models;

/// <summary>
/// 数据记录模型，表示从页面中提取的一条结构化数据
/// </summary>
public class DataRecord
{
    /// <summary>
    /// 记录唯一标识
    /// </summary>
    public string RecordId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 关联的任务标识
    /// </summary>
    public string? TaskId { get; set; }

    /// <summary>
    /// 关联的请求标识
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// 关联的步骤标识
    /// </summary>
    public string? StepId { get; set; }

    /// <summary>
    /// 关联的表达式标识
    /// </summary>
    public string? ExpressionId { get; set; }

    /// <summary>
    /// 提取的字段数据，键为字段名，值为字段值
    /// </summary>
    public Dictionary<string, object?> Fields { get; set; } = new();

    /// <summary>
    /// 字段与表达式的映射关系，键为字段名，值为表达式
    /// </summary>
    public Dictionary<string, string> FieldExpressionMap { get; set; } = new();

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 数据来源 URL
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// 执行采集的代理节点标识
    /// </summary>
    public string? AgentId { get; set; }

    /// <summary>
    /// 数据提取时间
    /// </summary>
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;
}
