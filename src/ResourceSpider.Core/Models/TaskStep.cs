using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

/// <summary>
/// 任务步骤模型，定义多阶段任务中单个步骤的配置
/// </summary>
public class TaskStep
{
    /// <summary>
    /// 步骤唯一标识
    /// </summary>
    public string StepId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 关联的任务标识
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
    /// 数据采集模式
    /// </summary>
    public CollectionMode CollectionMode { get; set; } = CollectionMode.HttpClient;

    /// <summary>
    /// 分配的代理组标识
    /// </summary>
    public string? AgentGroupId { get; set; }

    /// <summary>
    /// 请求配置，包含 URL、方法、请求头等
    /// </summary>
    public Dictionary<string, object?> RequestConfig { get; set; } = new();

    /// <summary>
    /// 数据提取规则列表
    /// </summary>
    public List<ExtractionRule> ExtractionRules { get; set; } = new();

    /// <summary>
    /// 变量映射列表，定义步骤间的数据传递
    /// </summary>
    public List<VariableMapping> VariableMappings { get; set; } = new();

    /// <summary>
    /// 分页配置
    /// </summary>
    public PaginationConfig? PaginationConfig { get; set; }

    /// <summary>
    /// 输出配置
    /// </summary>
    public OutputConfig? OutputConfig { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
