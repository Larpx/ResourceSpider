using SqlSugar;

namespace ResourceSpider.Server.Entities;

/// <summary>
/// 任务步骤实体，映射数据库 task_steps 表
/// 定义任务中每个步骤的配置，包括请求方式、数据提取规则和分页配置
/// 一个任务可包含多个步骤，按 StepOrder 顺序依次执行
/// </summary>
[SugarTable("task_steps")]
public class TaskStepEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 步骤唯一标识符，业务主键
    /// </summary>
    [SugarColumn(Length = 64)]
    public string StepId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的任务 ID，标识该步骤属于哪个任务
    /// </summary>
    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 步骤执行顺序，数值越小越先执行
    /// </summary>
    public int StepOrder { get; set; }

    /// <summary>
    /// 步骤名称，用于展示和识别步骤
    /// </summary>
    [SugarColumn(Length = 100)]
    public string StepName { get; set; } = string.Empty;

    /// <summary>
    /// 采集模式：HttpClient-HTTP 客户端请求，Browser-浏览器渲染采集
    /// </summary>
    [SugarColumn(Length = 64)]
    public string CollectionMode { get; set; } = "HttpClient";

    /// <summary>
    /// 指定执行该步骤的代理分组 ID，为空时使用任务级别的代理分组
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? AgentGroupId { get; set; }

    /// <summary>
    /// 请求配置 JSON，包含 URL 模板、请求方法、请求头、请求体等 HTTP 请求相关配置
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string RequestConfig { get; set; } = "{}";

    /// <summary>
    /// 数据提取规则 JSON 数组，定义如何从响应中提取结构化数据
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string ExtractionRules { get; set; } = "[]";

    /// <summary>
    /// 变量映射 JSON，定义步骤间的数据传递关系，将上游步骤的输出映射为当前步骤的输入变量
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? VariableMappings { get; set; }

    /// <summary>
    /// 分页配置 JSON，包含分页类型（页码/游标/链接）、下一页选择器等分页相关配置
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? PaginationConfig { get; set; }

    /// <summary>
    /// 输出配置 JSON，定义步骤结果的存储方式和格式
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? OutputConfig { get; set; }

    /// <summary>
    /// 步骤创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
