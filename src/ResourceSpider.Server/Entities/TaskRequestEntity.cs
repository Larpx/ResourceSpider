using SqlSugar;

namespace ResourceSpider.Server.Entities;

/// <summary>
/// 任务请求实体，映射数据库 task_requests 表
/// 记录任务中每个 HTTP 请求的详细信息，包括请求配置、执行状态和错误信息
/// 用于请求级别的状态跟踪和重试管理
/// </summary>
[SugarTable("task_requests")]
public class TaskRequestEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 关联的任务 ID，标识该请求属于哪个任务
    /// </summary>
    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 请求唯一标识符，业务主键
    /// </summary>
    [SugarColumn(Length = 128)]
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// 请求目标 URL
    /// </summary>
    [SugarColumn(Length = 2048)]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// HTTP 请求方法：GET、POST、PUT、DELETE 等
    /// </summary>
    [SugarColumn(Length = 16)]
    public string Method { get; set; } = "GET";

    /// <summary>
    /// 请求头 JSON，包含 Content-Type、User-Agent 等 HTTP 请求头
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? Headers { get; set; }

    /// <summary>
    /// 请求体内容，POST/PUT 请求时使用
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Body { get; set; }

    /// <summary>
    /// 请求状态：0-待处理，1-处理中，2-已完成，3-已失败，4-已超时
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 已重试次数
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// 最大重试次数，默认 3 次
    /// </summary>
    public int MaxRetry { get; set; } = 3;

    /// <summary>
    /// 请求响应结果内容
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Result { get; set; }

    /// <summary>
    /// 错误信息描述
    /// </summary>
    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? Error { get; set; }

    /// <summary>
    /// 错误类型分类，如 NetworkError、TimeoutError、ParseError 等
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? ErrorType { get; set; }

    /// <summary>
    /// 错误代码，如 HTTP 状态码或自定义错误码
    /// </summary>
    [SugarColumn(Length = 32, IsNullable = true)]
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 请求耗时（毫秒）
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// 被分配执行该请求的代理节点 ID
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? AssignedAgentId { get; set; }

    /// <summary>
    /// 是否已恢复：0-未恢复，1-已恢复
    /// </summary>
    public int Recovered { get; set; }

    /// <summary>
    /// 请求恢复时间，记录失败请求被重新处理的时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? RecoveredAt { get; set; }

    /// <summary>
    /// 请求处理完成时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// 请求记录创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 请求记录最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
