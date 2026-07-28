using System.Collections.Generic;
using System.Linq;
using Larpx.PersonalTools.ResourceSpider.Core.Selector;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Larpx.PersonalTools.ResourceSpider.Infrastructure.HtmlAgilityPack.Css;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Selector;

/// <summary>
/// CSS 选择器实现，使用 HtmlAgilityPack.Css 扩展对 HTML 文档进行 CSS 选择器查询
/// 支持元素选择和属性提取
/// </summary>
public class CssSelector : ISelector
{
    private readonly string _selector;
    private readonly string? _attrName;

    /// <summary>
    /// 通过 CSS 选择器表达式初始化
    /// </summary>
    /// <param name="selector">CSS 选择器表达式</param>
    public CssSelector(string selector) { _selector = selector; }

    /// <summary>
    /// 通过 CSS 选择器表达式和属性名初始化
    /// </summary>
    /// <param name="selector">CSS 选择器表达式</param>
    /// <param name="attr">要提取的属性名</param>
    public CssSelector(string selector, string? attr) { _selector = selector; _attrName = attr; }

    /// <summary>
    /// 使用 CSS 选择器选取单个元素
    /// </summary>
    /// <param name="text">HTML 内容字符串</param>
    /// <returns>匹配的可选择对象，未匹配返回 null</returns>
    public ISelectable? Select(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var document = new HtmlDocument { OptionAutoCloseOnEnd = true };
        document.LoadHtml(text);
        var node = document.DocumentNode.QuerySelector(_selector);
        if (node == null) return null;
        return HasAttribute ? new TextSelectable(node.Attributes[_attrName!]?.Value?.Trim() ?? "") : new HtmlSelectable(node);
    }

    /// <summary>
    /// 使用 CSS 选择器选取多个元素
    /// </summary>
    /// <param name="text">HTML 内容字符串</param>
    /// <returns>匹配的可选择对象集合</returns>
    public IEnumerable<ISelectable> SelectList(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        var document = new HtmlDocument { OptionAutoCloseOnEnd = true };
        document.LoadHtml(text);
        var nodes = document.DocumentNode.QuerySelectorAll(_selector);
        return HasAttribute
            ? nodes.Select(x => (ISelectable)new TextSelectable(x.Attributes[_attrName!]?.Value?.Trim() ?? ""))
            : nodes.Select(node => new HtmlSelectable(node));
    }

    /// <summary>
    /// 获取是否指定了属性名提取
    /// </summary>
    public bool HasAttribute => !string.IsNullOrWhiteSpace(_attrName);
}
