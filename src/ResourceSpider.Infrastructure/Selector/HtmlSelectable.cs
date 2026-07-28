using System;
using System.Collections.Generic;
using System.Linq;
using Larpx.PersonalTools.ResourceSpider.Core.Selector;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using HtmlNode = HtmlAgilityPack.HtmlNode;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Selector;

/// <summary>
/// HTML 可选择对象，基于 HtmlAgilityPack 的 HtmlNode 实现
/// 支持 XPath、CSS 等方式对 HTML 文档进行元素选取
/// </summary>
public class HtmlSelectable : Selectable
{
    private readonly HtmlNode _node;

    /// <summary>
    /// 通过 HtmlNode 初始化 HTML 可选择对象
    /// </summary>
    /// <param name="node">HTML 节点</param>
    public HtmlSelectable(HtmlNode node) { _node = node; }

    /// <summary>
    /// 通过 HTML 字符串初始化 HTML 可选择对象
    /// </summary>
    /// <param name="html">HTML 内容字符串</param>
    /// <param name="relativeUri">相对 URI 基址</param>
    /// <param name="removeOutboundLinks">是否移除外部链接</param>
    public HtmlSelectable(string html, string? relativeUri = null, bool removeOutboundLinks = true)
    {
        var document = new HtmlDocument { OptionAutoCloseOnEnd = true };
        document.LoadHtml(html);
        _node = document.DocumentNode;
    }

    /// <summary>
    /// 获取当前 HTML 节点中的所有链接（href 和 src 属性值）
    /// </summary>
    /// <returns>有效的 URI 字符串集合</returns>
    public override IEnumerable<string> Links()
    {
        var links = SelectList(Selectors.XPath("./descendant-or-self::*/@href"))?.Select(x => x.Value);
        var sourceLinks = SelectList(Selectors.XPath("./descendant-or-self::*/@src"))?.Select(x => x.Value);
        var results = new HashSet<string>();
        if (links != null) foreach (var link in links) { if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out _)) results.Add(link); }
        if (sourceLinks != null) foreach (var link in sourceLinks) { if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out _)) results.Add(link); }
        return results;
    }

    /// <summary>
    /// 获取子节点集合
    /// </summary>
    /// <returns>子 HTML 节点的可选择对象集合</returns>
    public override IEnumerable<ISelectable> Nodes() => _node.ChildNodes.Select(x => new HtmlSelectable(x));

    /// <summary>
    /// 获取当前节点的内部文本
    /// </summary>
    public override string Value => _node.InnerText;

    /// <summary>
    /// 获取当前节点的内部 HTML
    /// </summary>
    public string InnerHtml => _node.InnerHtml;

    /// <summary>
    /// 获取当前节点的外部 HTML（包含自身标签）
    /// </summary>
    public string OuterHtml => _node.OuterHtml;

    /// <summary>
    /// 使用指定选择器选取单个元素
    /// </summary>
    /// <param name="selector">选择器实例</param>
    /// <returns>匹配的可选择对象，未匹配返回 null</returns>
    /// <exception cref="ArgumentNullException">选择器为 null 时抛出</exception>
    public override ISelectable? Select(ISelector selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        return selector.Select(_node.OuterHtml);
    }

    /// <summary>
    /// 使用指定选择器选取多个元素
    /// </summary>
    /// <param name="selector">选择器实例</param>
    /// <returns>匹配的可选择对象集合</returns>
    /// <exception cref="ArgumentNullException">选择器为 null 时抛出</exception>
    public override IEnumerable<ISelectable> SelectList(ISelector selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        return selector.SelectList(_node.OuterHtml);
    }

    /// <summary>
    /// 获取可选择对象类型为 HTML
    /// </summary>
    public override SelectableType Type => SelectableType.Html;
}
