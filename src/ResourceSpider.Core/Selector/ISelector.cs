using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ResourceSpider.Core.Selector;

/// <summary>
/// 选择器接口，定义从文本中选取内容的通用契约
/// </summary>
public interface ISelector
{
    /// <summary>
    /// 从文本中选取第一个匹配的元素
    /// </summary>
    /// <param name="text">待选取的文本内容</param>
    /// <returns>选取的可选元素，无匹配时返回 null</returns>
    ISelectable? Select(string text);

    /// <summary>
    /// 从文本中选取所有匹配的元素
    /// </summary>
    /// <param name="text">待选取的文本内容</param>
    /// <returns>选取的可选元素集合</returns>
    IEnumerable<ISelectable> SelectList(string text);
}

/// <summary>
/// 可选元素接口，提供链式选择操作，支持 XPath、CSS、JSONPath 等多种选择方式
/// </summary>
public interface ISelectable
{
    /// <summary>
    /// 可选元素的内容类型
    /// </summary>
    SelectableType Type { get; }

    /// <summary>
    /// 使用 XPath 表达式选取子元素
    /// </summary>
    /// <param name="xpath">XPath 表达式</param>
    /// <returns>选取的可选元素</returns>
    ISelectable? XPath(string xpath);

    /// <summary>
    /// 使用 CSS 选择器选取子元素
    /// </summary>
    /// <param name="css">CSS 选择器表达式</param>
    /// <param name="attr">要提取的属性名，为空时提取文本内容</param>
    /// <returns>选取的可选元素</returns>
    ISelectable? Css(string css, string? attr = null);

    /// <summary>
    /// 提取当前元素中的所有链接
    /// </summary>
    /// <returns>链接地址集合</returns>
    IEnumerable<string> Links();

    /// <summary>
    /// 使用 JSONPath 表达式从 JSON 数据中选取元素
    /// </summary>
    /// <param name="jsonPath">JSONPath 表达式</param>
    /// <returns>选取的可选元素</returns>
    ISelectable? JsonPath(string jsonPath);

    /// <summary>
    /// 获取当前元素下的所有子节点
    /// </summary>
    /// <returns>子节点集合</returns>
    IEnumerable<ISelectable> Nodes();

    /// <summary>
    /// 使用正则表达式匹配内容
    /// </summary>
    /// <param name="pattern">正则表达式模式</param>
    /// <param name="options">正则表达式选项</param>
    /// <param name="replacement">替换模式，默认为 $0（完整匹配）</param>
    /// <returns>匹配的可选元素</returns>
    ISelectable? Regex(string pattern, RegexOptions options = RegexOptions.None, string replacement = "$0");

    /// <summary>
    /// 获取当前元素的文本值
    /// </summary>
    string Value { get; }

    /// <summary>
    /// 使用选择器选取第一个匹配的元素
    /// </summary>
    /// <param name="selector">选择器实例</param>
    /// <returns>选取的可选元素</returns>
    ISelectable? Select(ISelector selector);

    /// <summary>
    /// 使用选择器选取所有匹配的元素
    /// </summary>
    /// <param name="selector">选择器实例</param>
    /// <returns>选取的可选元素集合</returns>
    IEnumerable<ISelectable> SelectList(ISelector selector);
}
