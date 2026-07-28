using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Larpx.PersonalTools.ResourceSpider.Core.Selector;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Selector;

/// <summary>
/// 选择器工厂类，提供各种类型选择器的创建方法，并缓存已创建的选择器实例
/// 避免重复创建相同表达式的选择器，提升性能
/// </summary>
public static class Selectors
{
    private static readonly ConcurrentDictionary<string, ISelector> Cache = new();

    /// <summary>
    /// 创建或获取正则表达式选择器
    /// </summary>
    /// <param name="expr">正则表达式模式</param>
    /// <param name="options">正则表达式选项</param>
    /// <param name="replacement">替换模式，默认 "$0"</param>
    /// <returns>正则表达式选择器实例</returns>
    public static ISelector Regex(string expr, RegexOptions options = RegexOptions.None, string replacement = "$0")
    {
        var key = $"r_{expr}_{replacement}";
        if (!Cache.ContainsKey(key)) Cache.TryAdd(key, new RegexSelector(expr, options, replacement));
        return Cache[key];
    }

    /// <summary>
    /// 创建或获取 CSS 选择器
    /// </summary>
    /// <param name="expr">CSS 选择器表达式</param>
    /// <param name="attr">要提取的属性名，为 null 时返回元素本身</param>
    /// <returns>CSS 选择器实例</returns>
    public static ISelector Css(string expr, string? attr = null)
    {
        var key = $"c_{expr}_{attr}";
        if (!Cache.ContainsKey(key)) Cache.TryAdd(key, new CssSelector(expr, attr));
        return Cache[key];
    }

    /// <summary>
    /// 创建或获取 XPath 选择器
    /// </summary>
    /// <param name="expr">XPath 表达式</param>
    /// <returns>XPath 选择器实例</returns>
    public static ISelector XPath(string expr)
    {
        var key = $"x_{expr}";
        if (!Cache.ContainsKey(key)) Cache.TryAdd(key, new XPathSelector(expr));
        return Cache[key];
    }

    /// <summary>
    /// 创建或获取 JsonPath 选择器
    /// </summary>
    /// <param name="expr">JsonPath 表达式</param>
    /// <returns>JsonPath 选择器实例</returns>
    public static ISelector JsonPath(string expr)
    {
        var key = $"j_{expr}";
        if (!Cache.ContainsKey(key)) Cache.TryAdd(key, new JsonPathSelector(expr));
        return Cache[key];
    }
}
