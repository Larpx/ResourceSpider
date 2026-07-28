using Larpx.PersonalTools.ResourceSpider.Core.Enums;

namespace Larpx.PersonalTools.ResourceSpider.Core.Models;

/// <summary>
/// 任务步骤模型，定义多步骤采集任务中的单个步骤，包含请求配置、提取规则、分页配置及步骤间依赖关系
/// </summary>
public class TaskStep
{
    /// <summary>
    /// 步骤唯一标识，默认自动生成 GUID
    /// </summary>
    public string StepId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 所属任务的 ID
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 步骤执行顺序，数值越小越先执行
    /// </summary>
    public int StepOrder { get; set; }

    /// <summary>
    /// 步骤名称
    /// </summary>
    public string StepName { get; set; } = string.Empty;

    /// <summary>
    /// 步骤类型，如 DataCollection（数据采集）、Navigation（页面导航）等
    /// </summary>
    public string StepType { get; set; } = "DataCollection";

    /// <summary>
    /// 采集模式，如 HttpClient、Playwright 等
    /// </summary>
    public CollectionMode CollectionMode { get; set; } = CollectionMode.HttpClient;

    /// <summary>
    /// 指定执行此步骤的 Agent 分组 ID，为 null 时使用任务级别的分组
    /// </summary>
    public string? AgentGroupId { get; set; }

    /// <summary>
    /// 步骤请求配置，定义 HTTP 请求的 URL、方法、头部、超时等参数
    /// </summary>
    public StepRequestConfig? RequestConfig { get; set; }

    /// <summary>
    /// 数据提取规则列表，定义从响应内容中提取字段的规则
    /// </summary>
    public List<ExtractionRule> ExtractionRules { get; set; } = new();

    /// <summary>
    /// 变量映射列表，定义步骤间数据传递的映射关系
    /// </summary>
    public List<VariableMapping> VariableMappings { get; set; } = new();

    /// <summary>
    /// 分页配置，定义翻页采集的策略
    /// </summary>
    public PaginationConfig? PaginationConfig { get; set; }

    /// <summary>
    /// 输出配置，定义步骤结果的输出方式
    /// </summary>
    public OutputConfig? OutputConfig { get; set; }

    /// <summary>
    /// 步骤开始条件，满足条件时步骤才会执行
    /// </summary>
    public StepStartCondition? StartCondition { get; set; }

    /// <summary>
    /// 步骤结束条件，满足条件时步骤提前结束
    /// </summary>
    public StepEndCondition? EndCondition { get; set; }

    /// <summary>
    /// 依赖的步骤 ID 列表，所有依赖步骤完成后才会执行此步骤
    /// </summary>
    public List<string>? DependsOnStepIds { get; set; }

    /// <summary>
    /// 资源池配置，定义步骤可使用的并发资源限制
    /// </summary>
    public ResourcePoolConfig? ResourcePoolConfig { get; set; }

    /// <summary>
    /// 步骤执行状态
    /// </summary>
    public StepState State { get; set; } = StepState.Waiting;

    /// <summary>
    /// 步骤扩展配置（JSON 格式字符串）
    /// </summary>
    public string? StepConfig { get; set; }

    /// <summary>
    /// 步骤超时时间（毫秒），0 表示不限制
    /// </summary>
    public int Timeout { get; set; } = 0;

    /// <summary>
    /// 步骤重试策略
    /// </summary>
    public StepRetryPolicy? RetryPolicy { get; set; }

    /// <summary>
    /// 断点续传页码，记录上次中断时的页码，恢复时从此页继续采集
    /// 为 null 或 0 表示从头开始
    /// </summary>
    public int? ResumeFromPage { get; set; }

    /// <summary>
    /// 步骤创建时间（UTC）
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
