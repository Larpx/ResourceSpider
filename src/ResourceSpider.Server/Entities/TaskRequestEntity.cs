using SqlSugar;

namespace ResourceSpider.Server.Entities;

[SugarTable("task_requests")]
public class TaskRequestEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    [SugarColumn(Length = 128)]
    public string RequestId { get; set; } = string.Empty;

    [SugarColumn(Length = 2048)]
    public string Url { get; set; } = string.Empty;

    [SugarColumn(Length = 16)]
    public string Method { get; set; } = "GET";

    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? Headers { get; set; }

    [SugarColumn(IsNullable = true)]
    public string? Body { get; set; }

    public int Status { get; set; }

    public int RetryCount { get; set; }

    public int MaxRetry { get; set; } = 3;

    [SugarColumn(IsNullable = true)]
    public string? Result { get; set; }

    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? Error { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? ErrorType { get; set; }

    [SugarColumn(Length = 32, IsNullable = true)]
    public string? ErrorCode { get; set; }

    public int? Duration { get; set; }

    [SugarColumn(Length = 64, IsNullable = true)]
    public string? AssignedAgentId { get; set; }

    public int Recovered { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? RecoveredAt { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
