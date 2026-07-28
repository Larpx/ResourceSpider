using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Larpx.PersonalTools.ResourceSpider.Core.Selector;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using HtmlNode = HtmlAgilityPack.HtmlNode;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Selector;

/// <summary>
/// XPath 选择器实现，支持 XPath 表达式对 HTML 文档进行节点选取
/// 自动识别 XPath 末尾的属性选择器（如 @href），提取属性值而非元素
/// </summary>
public class XPathSelector : ISelector
{
    private static readonly Regex AttributeXPathRegex = new(@"@[\w\s-]+", RegexOptions.RightToLeft | RegexOptions.IgnoreCase);
    private readonly string _xpath;
    private readonly string? _attrName;

    /// <summary>
    /// 通过 XPath 表达式初始化，自动解析末尾的属性选择器
    /// </summary>
    /// <param name="xpath">XPath 表达式</param>
    public XPathSelector(string xpath)
    {
        _xpath = xpath;
        var match = AttributeXPathRegex.Match(_xpath);
        if (!string.IsNullOrWhiteSpace(match.Value) && _xpath.EndsWith(match.Value))
        {
            _attrName = match.Value.Replace("@", "");
            _xpath = _xpath.Replace("/" + match.Value, "");
        }
    }

    /// <summary>
    /// 使用 XPath 选取单个元素
    /// </summary>
    /// <param name="text">HTML 内容字符串</param>
    /// <returns>匹配的可选择对象，未匹配返回 null</returns>
    public ISelectable? Select(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var document = new HtmlDocument { OptionAutoCloseOnEnd = true };
        document.LoadHtml(text);
        var node = document.DocumentNode.SelectSingleNode(_xpath);
        if (node == null) return null;
        return HasAttribute ? new TextSelectable(node.Attributes[_attrName!]?.Value?.Trim() ?? "") : new HtmlSelectable(node);
    }

    /// <summary>
    /// 使用 XPath 选取多个元素
    /// </summary>
    /// <param name="text">HTML 内容字符串</param>
    /// <returns>匹配的可选择对象集合</returns>
    public IEnumerable<ISelectable> SelectList(string text)
    {
        var document = new HtmlDocument { OptionAutoCloseOnEnd = true };
        document.LoadHtml(text);
        var nodes = document.DocumentNode.SelectNodes(_xpath);
        if (nodes == null) return [];
        return HasAttribute
            ? nodes.Select(x => new TextSelectable(x.Attributes[_attrName!]?.Value?.Trim() ?? ""))
            : nodes.Select(node => new HtmlSelectable(node));
    }

    /// <summary>
    /// 获取是否指定了属性名提取
    /// </summary>
    public bool HasAttribute => !string.IsNullOrWhiteSpace(_attrName);
}
