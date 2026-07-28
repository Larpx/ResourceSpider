using System.Collections.Generic;
using System.Text.RegularExpressions;
using Larpx.PersonalTools.ResourceSpider.Core.Selector;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Selector;

/// <summary>
/// 可选择对象抽象基类，提供 XPath、CSS、JsonPath、Regex 等选择器的统一调用接口
/// </summary>
public abstract class Selectable : ISelectable
{
    /// <summary>
    /// 获取当前节点中的所有链接
    /// </summary>
    /// <returns>链接字符串集合</returns>
    public abstract IEnumerable<string> Links();

    /// <summary>
    /// 获取可选择对象的类型
    /// </summary>
    public abstract SelectableType Type { get; }

    /// <summary>
    /// 获取子节点集合
    /// </summary>
    /// <returns>子节点可选择对象集合</returns>
    public abstract IEnumerable<ISelectable> Nodes();

    /// <summary>
    /// 获取当前节点的文本值
    /// </summary>
    public abstract string Value { get; }

    /// <summary>
    /// 使用指定选择器选取单个元素
    /// </summary>
    /// <param name="selector">选择器实例</param>
    /// <returns>匹配的可选择对象，未匹配返回 null</returns>
    public abstract ISelectable? Select(ISelector selector);

    /// <summary>
    /// 使用指定选择器选取多个元素
    /// </summary>
    /// <param name="selector">选择器实例</param>
    /// <returns>匹配的可选择对象集合</returns>
    public abstract IEnumerable<ISelectable> SelectList(ISelector selector);

    /// <summary>
    /// 使用 XPath 表达式选取单个元素
    /// </summary>
    /// <param name="xpath">XPath 表达式</param>
    /// <returns>匹配的可选择对象，未匹配返回 null</returns>
    public virtual ISelectable? XPath(string xpath) => Select(Selectors.XPath(xpath));

    /// <summary>
    /// 使用 CSS 选择器选取单个元素，可选指定属性名
    /// </summary>
    /// <param name="css">CSS 选择器表达式</param>
    /// <param name="attr">要提取的属性名，为 null 时返回元素本身</param>
    /// <returns>匹配的可选择对象，未匹配返回 null</returns>
    public ISelectable? Css(string css, string? attr = null) => Select(Selectors.Css(css, attr));

    /// <summary>
    /// 使用 JsonPath 表达式选取单个元素
    /// </summary>
    /// <param name="jsonPath">JsonPath 表达式</param>
    /// <returns>匹配的可选择对象，未匹配返回 null</returns>
    public virtual ISelectable? JsonPath(string jsonPath) => Select(Selectors.JsonPath(jsonPath));

    /// <summary>
    /// 使用正则表达式选取单个元素
    /// </summary>
    /// <param name="pattern">正则表达式模式</param>
    /// <param name="options">正则表达式选项</param>
    /// <param name="replacement">替换模式，默认 "$0"（完整匹配）</param>
    /// <returns>匹配的可选择对象，未匹配返回 null</returns>
    public virtual ISelectable? Regex(string pattern, RegexOptions options = RegexOptions.None, string replacement = "$0") => Select(Selectors.Regex(pattern, options, replacement));
}
