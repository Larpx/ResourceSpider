using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ResourceSpider.Core.Selector;

namespace ResourceSpider.Infrastructure.Selector;

public class RegexSelector : ISelector
{
    private readonly Regex _regex;
    private readonly string _replacement;

    public RegexSelector(string pattern, RegexOptions options = RegexOptions.None, string replacement = "$0")
    {
        if (string.IsNullOrWhiteSpace(pattern)) throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));
        _regex = new Regex(pattern, options);
        _replacement = replacement;
    }

    public ISelectable Select(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var match = _regex.Match(text);
        return match.Success ? new TextSelectable(match.Result(_replacement)) : null;
    }

    public IEnumerable<ISelectable> SelectList(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
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
