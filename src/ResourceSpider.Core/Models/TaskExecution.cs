using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

/// <summary>
/// 任务执行记录模型，记录每次任务执行的详细信息
/// </summary>
public class TaskExecution
{
    /// <summary>
    /// 执行记录唯一标识
    /// </summary>
    public string ExecutionId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 关联的任务标识
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 执行此任务的代理节点标识
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// 执行状态
    /// </summary>
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;

    /// <summary>
    /// 执行时的配置快照，记录任务执行时的配置状态
    /// </summary>
    public Dictionary<string, object?>? ConfigSnapshot { get; set; }

    /// <summary>
    /// 执行开始时间
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// 执行完成时间
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 总爬取页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 成功请求数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败请求数
    /// </summary>
    public int FailCount { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
