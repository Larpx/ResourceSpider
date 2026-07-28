namespace Larpx.PersonalTools.ResourceSpider.Core.Selector;

/// <summary>
/// 选择器类型枚举，定义支持的选择器语法
/// </summary>
public enum SelectorType
{
    /// <summary>
    /// XPath 选择器
    /// </summary>
    XPath,

    /// <summary>
    /// 正则表达式选择器
    /// </summary>
    Regex,

    /// <summary>
    /// CSS 选择器
    /// </summary>
    Css,

    /// <summary>
    /// JSONPath 选择器
    /// </summary>
    JsonPath,

    /// <summary>
    /// 环境变量选择器
    /// </summary>
    Environment
}

/// <summary>
/// 可选元素内容类型枚举
/// </summary>
public enum SelectableType
{
    /// <summary>
    /// 纯文本类型
    /// </summary>
    Text,

    /// <summary>
    /// HTML 文档类型
    /// </summary>
    Html,

    /// <summary>
    /// JSON 数据类型
    /// </summary>
    Json
}
