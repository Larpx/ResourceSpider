using System;
using System.Collections.Generic;
using System.Linq;
using ResourceSpider.Core.Selector;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using HtmlNode = HtmlAgilityPack.HtmlNode;

namespace ResourceSpider.Infrastructure.Selector;

public class HtmlSelectable : Selectable
{
    private readonly HtmlNode _node;

    public HtmlSelectable(HtmlNode node) { _node = node; }

    public HtmlSelectable(string html, string? relativeUri = null, bool removeOutboundLinks = true)
    {
        var document = new HtmlDocument { OptionAutoCloseOnEnd = true };
        document.LoadHtml(html);
        _node = document.DocumentNode;
    }

    public override IEnumerable<string> Links()
    {
        var links = SelectList(Selectors.XPath("./descendant-or-self::*/@href"))?.Select(x => x.Value);
        var sourceLinks = SelectList(Selectors.XPath("./descendant-or-self::*/@src"))?.Select(x => x.Value);
        var results = new HashSet<string>();
        if (links != null) foreach (var link in links) { if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out _)) results.Add(link); }
        if (sourceLinks != null) foreach (var link in sourceLinks) { if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out _)) results.Add(link); }
        return results;
    }

    public override IEnumerable<ISelectable> Nodes() => _node.ChildNodes.Select(x => new HtmlSelectable(x));
    public override string Value => _node.InnerText;
    public string InnerHtml => _node.InnerHtml;
    public string OuterHtml => _node.OuterHtml;

    public override ISelectable? Select(ISelector selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        return selector.Select(_node.OuterHtml);
    }

    public override IEnumerable<ISelectable> SelectList(ISelector selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        return selector.SelectList(_node.OuterHtml);
    }

    public override SelectableType Type => SelectableType.Html;
}
