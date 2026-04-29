using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

/// <summary>
/// 爬虫任务模型，定义一个完整的采集任务，包含请求配置、调度策略、重试策略、表达式配置及执行步骤
/// </summary>
public class SpiderTask
{
    /// <summary>
    /// 任务唯一标识，默认自动生成 GUID
    /// </summary>
    public string TaskId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 任务名称
    /// </summary>
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型，如单页采集、多页采集等
    /// </summary>
    public TaskType TaskType { get; set; } = TaskType.SinglePage;

    /// <summary>
    /// 任务优先级，数值越高优先级越高，默认为 5
    /// </summary>
    public int Priority { get; set; } = 5;

    /// <summary>
    /// 任务状态，如待执行、执行中、已完成等
    /// </summary>
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;

    /// <summary>
    /// 步骤请求配置，定义 HTTP 请求的 URL、方法、头部、超时等参数
    /// </summary>
    public StepRequestConfig? RequestConfig { get; set; }

    /// <summary>
    /// 任务调度配置，定义 Cron 表达式、间隔时间或一次性执行策略
    /// </summary>
    public TaskScheduleConfig? ScheduleConfig { get; set; }

    /// <summary>
    /// 重试策略，定义最大重试次数、重试间隔、可重试的 HTTP 状态码等
    /// </summary>
    public StepRetryPolicy? RetryPolicy { get; set; }

    /// <summary>
    /// 反爬策略配置，定义请求延迟、User-Agent 轮换、代理轮换等反爬措施
    /// </summary>
    public AntiCrawlConfig? AntiCrawlConfig { get; set; }

    /// <summary>
    /// 全局配置，定义并发控制、超时、去重等全局参数
    /// </summary>
    public TaskGlobalConfig? GlobalConfig { get; set; }

    /// <summary>
    /// 配置版本号，用于配置版本管理和回滚
    /// </summary>
    public int ConfigVersion { get; set; } = 1;

    /// <summary>
    /// 任务标签列表，用于分类和筛选
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// 指定执行的 Agent 分组 ID，为 null 时由系统自动分配
    /// </summary>
    public string? AgentGroupId { get; set; }

    /// <summary>
    /// 被分配执行此任务的 Agent ID
    /// </summary>
    public string? AssignedAgentId { get; set; }

    /// <summary>
    /// 关联的提取表达式配置 ID
    /// </summary>
    public string? ExpressionId { get; set; }

    /// <summary>
    /// 提取表达式配置，定义数据提取规则和字段映射
    /// </summary>
    public ExpressionConfig? ExpressionConfig { get; set; }

    /// <summary>
    /// 任务执行进度百分比（0-100）
    /// </summary>
    public decimal Progress { get; set; }

    /// <summary>
    /// 总请求数
    /// </summary>
    public int TotalRequests { get; set; }

    /// <summary>
    /// 已完成的请求数
    /// </summary>
    public int CompletedRequests { get; set; }

    /// <summary>
    /// 失败的请求数
    /// </summary>
    public int FailedRequests { get; set; }

    /// <summary>
    /// 任务开始执行时间（UTC）
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 任务结束执行时间（UTC）
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 任务创建者
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 任务创建时间（UTC）
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 任务最后更新时间（UTC）
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 任务步骤列表，用于多步骤采集任务
    /// </summary>
    public List<TaskStep>? Steps { get; set; }
}
