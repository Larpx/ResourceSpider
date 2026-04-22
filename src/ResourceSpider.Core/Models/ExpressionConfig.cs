using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

/// <summary>
/// 表达式配置模型，定义页面数据的提取规则
/// </summary>
public class ExpressionConfig
{
    /// <summary>
    /// 表达式唯一标识
    /// </summary>
    public string ExpressionId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 表达式名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 表达式描述信息
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 选择器类型，决定使用哪种语法解析页面
    /// </summary>
    public ExpressionType SelectorType { get; set; }

    /// <summary>
    /// 容器表达式，用于定位包含多条数据的父元素
    /// </summary>
    public string ContainerExpression { get; set; } = string.Empty;

    /// <summary>
    /// 字段提取规则列表，定义每个字段的提取方式
    /// </summary>
    public List<ExpressionField> Fields { get; set; } = new();

    /// <summary>
    /// 全局值提取配置，用于提取页面级别的公共数据
    /// </summary>
    public List<GlobalValueConfig> GlobalValues { get; set; } = new();

    /// <summary>
    /// 后续请求配置，定义从当前页面提取的跟踪链接
    /// </summary>
    public List<FollowRequestConfig> FollowRequests { get; set; } = new();

    /// <summary>
    /// 表达式当前状态
    /// </summary>
    public ExpressionStatus Status { get; set; } = ExpressionStatus.Active;

    /// <summary>
    /// 使用成功次数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 使用失败次数
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// 最后验证时间
    /// </summary>
    public DateTime? LastValidatedAt { get; set; }

    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 表达式字段配置，定义单个字段的提取规则
/// </summary>
public class ExpressionField
{
    /// <summary>
    /// 字段唯一标识
    /// </summary>
    public string FieldId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 所属表达式标识
    /// </summary>
    public string ExpressionId { get; set; } = string.Empty;

    /// <summary>
    /// 字段名称
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// 字段选择器类型
    /// </summary>
    public ExpressionType SelectorType { get; set; }

    /// <summary>
    /// 提取表达式
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// 要提取的 HTML 属性名，为空时提取文本内容
    /// </summary>
    public string? AttributeName { get; set; }

    /// <summary>
    /// 是否为必填字段
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// 字段默认值，提取失败时使用
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// 格式化器名称，用于对提取结果进行后处理
    /// </summary>
    public string? Formatter { get; set; }

    /// <summary>
    /// 格式化器参数
    /// </summary>
    public string? FormatterArgs { get; set; }

    /// <summary>
    /// 字段排序序号
    /// </summary>
    public int Order { get; set; }
}

/// <summary>
/// 全局值提取配置，用于提取页面级别的公共数据（如页面标题等）
/// </summary>
public class GlobalValueConfig
{
    /// <summary>
    /// 全局值名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 选择器类型
    /// </summary>
    public ExpressionType SelectorType { get; set; }

    /// <summary>
    /// 提取表达式
    /// </summary>
    public string Expression { get; set; } = string.Empty;
}

/// <summary>
/// 后续请求配置，定义从当前页面中提取的跟踪链接规则
/// </summary>
public class FollowRequestConfig
{
    /// <summary>
    /// 选择器类型
    /// </summary>
    public ExpressionType SelectorType { get; set; }

    /// <summary>
    /// 提取链接的表达式列表
    /// </summary>
    public List<string> Expressions { get; set; } = new();

    /// <summary>
    /// URL 匹配模式列表，仅跟踪匹配的链接
    /// </summary>
    public List<string> Patterns { get; set; } = new();
}
