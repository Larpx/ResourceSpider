using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace ResourceSpider.Infrastructure.HtmlAgilityPack.Css;

public static class HtmlNodeExtensions
{
    public static bool IsElement(this HtmlNode node)
    {
        if (node == null) throw new ArgumentNullException("node");
        return node.NodeType == HtmlNodeType.Element;
    }

    public static IEnumerable<HtmlNode> Elements(this IEnumerable<HtmlNode> nodes)
    {
        if (nodes == null) throw new ArgumentNullException("nodes");
        return nodes.Where(n => n.IsElement());
    }

    public static IEnumerable<HtmlNode> Elements(this HtmlNode node)
    {
        if (node == null) throw new ArgumentNullException("node");
        return node.ChildNodes.Elements();
    }

    public static IEnumerable<HtmlNode> ElementsAfterSelf(this HtmlNode node)
    {
        if (node == null) throw new ArgumentNullException("node");
        return NodesAfterSelfImpl(node).Elements();
    }

    public static IEnumerable<HtmlNode> NodesAfterSelf(this HtmlNode node)
    {
        if (node == null) throw new ArgumentNullException("node");
        return NodesAfterSelfImpl(node);
    }

    private static IEnumerable<HtmlNode> NodesAfterSelfImpl(HtmlNode node) { while ((node = node.NextSibling) != null) yield return node; }

    public static IEnumerable<HtmlNode> ElementsBeforeSelf(this HtmlNode node)
    {
        if (node == null) throw new ArgumentNullException("node");
        return NodesBeforeSelfImpl(node).Elements();
    }

    public static IEnumerable<HtmlNode> NodesBeforeSelf(this HtmlNode node)
    {
        if (node == null) throw new ArgumentNullException("node");
        return NodesBeforeSelfImpl(node);
    }

    private static IEnumerable<HtmlNode> NodesBeforeSelfImpl(HtmlNode node) { while ((node = node.PreviousSibling) != null) yield return node; }

    public static IEnumerable<HtmlNode> Descendants(this HtmlNode node)
    {
        if (node == null) throw new ArgumentNullException("node");
        return DescendantsImpl(node);
    }

    private static IEnumerable<HtmlNode> DescendantsImpl(HtmlNode node)
    {
        foreach (var child in node.ChildNodes)
        {
            yield return child;
            foreach (var descendant in child.Descendants()) yield return descendant;
        }
    }
}
