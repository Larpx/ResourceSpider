namespace Larpx.PersonalTools.ResourceSpider.Core.Enums;

/// <summary>
/// 表达式类型枚举，定义支持的选择器语法
/// </summary>
public enum ExpressionType
{
    /// <summary>
    /// XPath 表达式，用于 XML/HTML 文档节点选择
    /// </summary>
    XPath,

    /// <summary>
    /// CSS 选择器，用于 HTML 文档元素选择
    /// </summary>
    CssSelector,

    /// <summary>
    /// JSONPath 表达式，用于 JSON 数据字段提取
    /// </summary>
    JsonPath,

    /// <summary>
    /// 正则表达式，用于文本模式匹配
    /// </summary>
    Regex,

    /// <summary>
    /// 环境变量，用于获取运行时系统信息
    /// </summary>
    Environment
}

/// <summary>
/// 表达式状态枚举，描述表达式的可用性状态
/// </summary>
public enum ExpressionStatus
{
    /// <summary>
    /// 活跃状态，表达式可正常使用
    /// </summary>
    Active = 1,

    /// <summary>
    /// 无效状态，表达式验证失败不可使用
    /// </summary>
    Invalid = 2,

    /// <summary>
    /// 已弃用状态，表达式已被新版本替代
    /// </summary>
    Deprecated = 3,

    /// <summary>
    /// 测试中状态，表达式正在验证阶段
    /// </summary>
    Testing = 4
}
