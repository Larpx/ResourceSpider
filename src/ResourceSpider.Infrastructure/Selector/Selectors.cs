using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using ResourceSpider.Core.Selector;

namespace ResourceSpider.Infrastructure.Selector;

public static class Selectors
{
    private static readonly ConcurrentDictionary<string, ISelector> Cache = new();

    public static ISelector Regex(string expr, RegexOptions options = RegexOptions.None, string replacement = "$0")
    {
        var key = $"r_{expr}_{replacement}";
        if (!Cache.ContainsKey(key)) Cache.TryAdd(key, new RegexSelector(expr, options, replacement));
        return Cache[key];
    }

    public static ISelector Css(string expr, string attr = null)
    {
        var key = $"c_{expr}_{attr}";
        if (!Cache.ContainsKey(key)) Cache.TryAdd(key, new CssSelector(expr, attr));
        return Cache[key];
    }

    public static ISelector XPath(string expr)
    {
        var key = $"x_{expr}";
        if (!Cache.ContainsKey(key)) Cache.TryAdd(key, new XPathSelector(expr));
        return Cache[key];
    }

    public static ISelector JsonPath(string expr)
    {
        var key = $"j_{expr}";
        if (!Cache.ContainsKey(key)) Cache.TryAdd(key, new JsonPathSelector(expr));
        return Cache[key];
    }
}
