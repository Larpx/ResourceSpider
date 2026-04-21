using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ResourceSpider.Core.Selector;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using HtmlNode = HtmlAgilityPack.HtmlNode;

namespace ResourceSpider.Infrastructure.Selector;

public class XPathSelector : ISelector
{
    private static readonly Regex AttributeXPathRegex = new(@"@[\w\s-]+", RegexOptions.RightToLeft | RegexOptions.IgnoreCase);
    private readonly string _xpath;
    private readonly string? _attrName;

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

    public ISelectable? Select(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var document = new HtmlDocument { OptionAutoCloseOnEnd = true };
        document.LoadHtml(text);
        var node = document.DocumentNode.SelectSingleNode(_xpath);
        if (node == null) return null;
        return HasAttribute ? new TextSelectable(node.Attributes[_attrName!]?.Value?.Trim() ?? "") : new HtmlSelectable(node);
    }

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

    public bool HasAttribute => !string.IsNullOrWhiteSpace(_attrName);
}
