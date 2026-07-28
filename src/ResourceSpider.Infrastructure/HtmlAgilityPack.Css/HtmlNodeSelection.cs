using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.HtmlAgilityPack.Css;

public static class HtmlNodeSelection
{
    private static readonly HtmlNodeOps Ops = new();

    public static HtmlNode? QuerySelector(this HtmlNode node, string selector)
        => node.QuerySelectorAll(selector).FirstOrDefault();

    public static IEnumerable<HtmlNode> QuerySelectorAll(this HtmlNode node, string selector)
        => QuerySelectorAll(node, selector, null!);

    public static IEnumerable<HtmlNode> QuerySelectorAll(this HtmlNode node, string selector, Func<string, Func<HtmlNode, IEnumerable<HtmlNode>>> compiler)
        => (compiler ?? CachableCompile)(selector)(node);

    public static int CacheSize
    {
        get => _compilerCache.Capacity;
        set => _compilerCache.Capacity = value;
    }

    public static Func<HtmlNode, IEnumerable<HtmlNode>> Compile(string selector)
    {
        var compiled = CssParser.Parse(selector, new SelectorGenerator<HtmlNode>(Ops)).Selector;
        return node => compiled(Enumerable.Repeat(node, 1));
    }

    private const int DefaultCacheSize = 60;
    private static readonly LruCache<string, Func<HtmlNode, IEnumerable<HtmlNode>>> _compilerCache = new(Compile, DefaultCacheSize);
    private static readonly Func<string, Func<HtmlNode, IEnumerable<HtmlNode>>> _defaultCachingCompiler = _compilerCache.GetValue;

    public static Func<HtmlNode, IEnumerable<HtmlNode>> CachableCompile(string selector) => _defaultCachingCompiler(selector);
}
