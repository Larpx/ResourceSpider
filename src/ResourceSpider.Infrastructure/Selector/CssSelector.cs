using System.Collections.Generic;
using System.Linq;
using ResourceSpider.Core.Selector;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using ResourceSpider.Infrastructure.HtmlAgilityPack.Css;

namespace ResourceSpider.Infrastructure.Selector;

public class CssSelector : ISelector
{
    private readonly string _selector;
    private readonly string? _attrName;

    public CssSelector(string selector) { _selector = selector; }
    public CssSelector(string selector, string? attr) { _selector = selector; _attrName = attr; }

    public ISelectable? Select(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var document = new HtmlDocument { OptionAutoCloseOnEnd = true };
        document.LoadHtml(text);
        var node = document.DocumentNode.QuerySelector(_selector);
        if (node == null) return null;
        return HasAttribute ? new TextSelectable(node.Attributes[_attrName!]?.Value?.Trim() ?? "") : new HtmlSelectable(node);
    }

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

    public bool HasAttribute => !string.IsNullOrWhiteSpace(_attrName);
}
