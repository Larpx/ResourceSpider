using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

/// <summary>
/// 数据提取规则模型，定义单个字段的提取和转换逻辑
/// </summary>
public class ExtractionRule
{
    /// <summary>
    /// 目标字段名称
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// 表达式类型
    /// </summary>
    public ExpressionType ExpressionType { get; set; }

    /// <summary>
    /// 提取表达式
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// 是否为必填字段
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// 字段默认值
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// 转换规则列表，对提取结果进行后处理
    /// </summary>
    public List<TransformRule>? TransformRules { get; set; }

    /// <summary>
    /// 是否提取为数组（多个匹配结果）
    /// </summary>
    public bool IsArray { get; set; }
}

/// <summary>
/// 转换规则模型，定义对提取数据的转换操作
/// </summary>
public class TransformRule
{
    /// <summary>
    /// 转换类型，如 trim、replace、regexreplace 等
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 匹配模式，用于 replace 和 regexreplace 类型
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// 替换值，用于 replace 和 regexreplace 类型
    /// </summary>
    public string? Replacement { get; set; }
}
