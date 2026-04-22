using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

/// <summary>
/// 爬虫任务模型，定义一个完整的爬取任务及其配置
/// </summary>
public class SpiderTask
{
    /// <summary>
    /// 任务唯一标识
    /// </summary>
    public string TaskId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 任务名称
    /// </summary>
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型
    /// </summary>
    public TaskType TaskType { get; set; } = TaskType.SinglePage;

    /// <summary>
    /// 任务优先级，数值越小优先级越高
    /// </summary>
    public int Priority { get; set; } = 5;

    /// <summary>
    /// 任务当前状态
    /// </summary>
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;

    /// <summary>
    /// 请求配置，包含 URL、方法、请求头等
    /// </summary>
    public Dictionary<string, object?> RequestConfig { get; set; } = new();

    /// <summary>
    /// 调度配置，如定时执行、并发控制等
    /// </summary>
    public Dictionary<string, object?>? ScheduleConfig { get; set; }

    /// <summary>
    /// 重试策略配置
    /// </summary>
    public Dictionary<string, object?>? RetryPolicy { get; set; }

    /// <summary>
    /// 反爬虫策略配置
    /// </summary>
    public Dictionary<string, object?>? AntiCrawlConfig { get; set; }

    /// <summary>
    /// 全局配置
    /// </summary>
    public Dictionary<string, object?>? GlobalConfig { get; set; }

    /// <summary>
    /// 配置版本号
    /// </summary>
    public int ConfigVersion { get; set; } = 1;

    /// <summary>
    /// 任务标签列表
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// 分配的代理组标识
    /// </summary>
    public string? AgentGroupId { get; set; }

    /// <summary>
    /// 指定执行的代理节点标识
    /// </summary>
    public string? AssignedAgentId { get; set; }

    /// <summary>
    /// 关联的表达式标识
    /// </summary>
    public string? ExpressionId { get; set; }

    /// <summary>
    /// 关联的表达式配置
    /// </summary>
    public ExpressionConfig? ExpressionConfig { get; set; }

    /// <summary>
    /// 任务执行进度百分比
    /// </summary>
    public decimal Progress { get; set; }

    /// <summary>
    /// 总请求数
    /// </summary>
    public int TotalRequests { get; set; }

    /// <summary>
    /// 已完成请求数
    /// </summary>
    public int CompletedRequests { get; set; }

    /// <summary>
    /// 失败请求数
    /// </summary>
    public int FailedRequests { get; set; }

    /// <summary>
    /// 任务开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 任务结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 任务创建者标识
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 任务步骤列表，用于多阶段任务
    /// </summary>
    public List<TaskStep>? Steps { get; set; }
}
