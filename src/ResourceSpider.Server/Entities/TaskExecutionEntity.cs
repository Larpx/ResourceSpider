using SqlSugar;

namespace ResourceSpider.Server.Entities;

/// <summary>
/// 任务执行记录实体，映射数据库 task_executions 表
/// 记录每次任务执行的完整信息，包括执行状态、统计数据和时间信息
/// </summary>
[SugarTable("task_executions")]
public class TaskExecutionEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 执行记录唯一标识符，业务主键
    /// </summary>
    [SugarColumn(Length = 64)]
    public string ExecutionId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的任务 ID，标识该执行记录属于哪个任务
    /// </summary>
    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 执行该任务的代理节点 ID
    /// </summary>
    [SugarColumn(Length = 64)]
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// 执行状态：0-待执行，1-执行中，2-已完成，3-已失败，4-已取消
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 执行时的任务配置快照 JSON，记录执行开始时的任务配置，防止配置变更影响执行
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? ConfigSnapshot { get; set; }

    /// <summary>
    /// 执行开始时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// 执行完成时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 总页面数，本次执行需要处理的总页面数量
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 成功页面数，成功处理的页面数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败页面数，处理失败的页面数量
    /// </summary>
    public int FailCount { get; set; }

    /// <summary>
    /// 错误信息，执行失败时的错误描述
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 执行记录创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
