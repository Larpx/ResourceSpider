using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ResourceSpider.Core.Selector;

namespace ResourceSpider.Infrastructure.Selector;

/// <summary>
/// 正则表达式选择器实现，使用 Regex 对文本进行模式匹配和提取
/// 支持捕获组替换和正则选项配置
/// </summary>
public class RegexSelector : ISelector
{
    private readonly Regex _regex;
    private readonly string _replacement;

    /// <summary>
    /// 通过正则表达式模式初始化
    /// </summary>
    /// <param name="pattern">正则表达式模式</param>
    /// <param name="options">正则表达式选项</param>
    /// <param name="replacement">替换模式，默认 "$0"（完整匹配）</param>
    /// <exception cref="ArgumentException">模式为空时抛出</exception>
    public RegexSelector(string pattern, RegexOptions options = RegexOptions.None, string replacement = "$0")
    {
        if (string.IsNullOrWhiteSpace(pattern)) throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));
        _regex = new Regex(pattern, options);
        _replacement = replacement;
    }

    /// <summary>
    /// 使用正则表达式选取第一个匹配项
    /// </summary>
    /// <param name="text">待匹配的文本</param>
    /// <returns>匹配的可选择对象，未匹配返回 null</returns>
    public ISelectable? Select(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var match = _regex.Match(text);
        return match.Success ? new TextSelectable(match.Result(_replacement)) : null;
    }

    /// <summary>
    /// 使用正则表达式选取所有匹配项
    /// </summary>
    /// <param name="text">待匹配的文本</param>
    /// <returns>匹配的可选择对象集合</returns>
    public IEnumerable<ISelectable> SelectList(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Enumerable.Empty<ISelectable>();
        var matches = _regex.Matches(text);
        var results = new List<string>();
        foreach (Match match in matches)
        {
            var value = match.Result(_replacement);
            if (!string.IsNullOrWhiteSpace(value)) results.Add(value);
        }
        return results.Select(x => new TextSelectable(x));
    }
}
