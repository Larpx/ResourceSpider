using SqlSugar;

namespace ResourceSpider.Server.Entities;

/// <summary>
/// 爬虫任务实体，映射数据库 tasks 表
/// 表示一个完整的爬虫任务，包含任务配置、调度策略、执行状态和进度信息
/// </summary>
[SugarTable("tasks")]
public class TaskEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 任务唯一标识符，业务主键
    /// </summary>
    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 任务名称，用于展示和识别任务
    /// </summary>
    [SugarColumn(Length = 256)]
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型，如 SinglePage（单页）、MultiPage（多页）、Crawl（爬取）等
    /// </summary>
    [SugarColumn(Length = 64)]
    public string TaskType { get; set; } = "SinglePage";

    /// <summary>
    /// 任务优先级，范围 1-10，数值越大优先级越高
    /// </summary>
    public int Priority { get; set; } = 5;

    /// <summary>
    /// 任务状态：0-待执行，1-执行中，2-已暂停，3-已完成，4-已失败，5-已取消
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 请求配置 JSON，包含 URL、请求头、超时等 HTTP 请求相关配置
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string RequestConfig { get; set; } = "{}";

    /// <summary>
    /// 调度配置 JSON，包含 Cron 表达式、执行间隔等定时调度相关配置
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? ScheduleConfig { get; set; }

    /// <summary>
    /// 重试策略 JSON，包含最大重试次数、重试间隔、退避策略等配置
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? RetryPolicy { get; set; }

    /// <summary>
    /// 反爬配置 JSON，包含 User-Agent 轮换、代理设置、请求延迟等反爬策略
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? AntiCrawlConfig { get; set; }

    /// <summary>
    /// 全局配置 JSON，包含数据存储、日志级别等全局性配置
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? GlobalConfig { get; set; }

    /// <summary>
    /// 配置版本号，每次修改任务配置时递增，用于配置版本管理
    /// </summary>
    public int ConfigVersion { get; set; } = 1;

    /// <summary>
    /// 任务标签 JSON 数组，用于任务分类和筛选
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? Tags { get; set; }

    /// <summary>
    /// 代理分组 ID，指定执行该任务的代理分组
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? AgentGroupId { get; set; }

    /// <summary>
    /// 已分配的代理 ID，记录当前正在执行该任务的代理节点
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? AssignedAgentId { get; set; }

    /// <summary>
    /// 关联的表达式 ID，指定该任务使用的数据提取表达式
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? ExpressionId { get; set; }

    /// <summary>
    /// 任务执行进度百分比，范围 0-100
    /// </summary>
    public decimal Progress { get; set; }

    /// <summary>
    /// 总请求数，任务需要处理的总请求量
    /// </summary>
    public int TotalRequests { get; set; }

    /// <summary>
    /// 已完成请求数，成功处理的请求量
    /// </summary>
    public int CompletedRequests { get; set; }

    /// <summary>
    /// 失败请求数，处理失败的请求量
    /// </summary>
    public int FailedRequests { get; set; }

    /// <summary>
    /// 任务开始执行时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 任务结束执行时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 任务创建者用户 ID
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 任务结果存储引擎（MySQL/PostgreSQL）
    /// </summary>
    [SugarColumn(Length = 32, IsNullable = true)]
    public string? ResultStorageEngine { get; set; }

    /// <summary>
    /// 任务创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 任务最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
