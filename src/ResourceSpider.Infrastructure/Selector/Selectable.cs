using System.Collections.Generic;
using System.Text.RegularExpressions;
using ResourceSpider.Core.Selector;

namespace ResourceSpider.Infrastructure.Selector;

public abstract class Selectable : ISelectable
{
    public abstract IEnumerable<string> Links();
    public abstract SelectableType Type { get; }
    public abstract IEnumerable<ISelectable> Nodes();
    public abstract string Value { get; }
    public abstract ISelectable? Select(ISelector selector);
    public abstract IEnumerable<ISelectable> SelectList(ISelector selector);

    public virtual ISelectable? XPath(string xpath) => Select(Selectors.XPath(xpath));
    public ISelectable? Css(string css, string? attr = null) => Select(Selectors.Css(css, attr));
    public virtual ISelectable? JsonPath(string jsonPath) => Select(Selectors.JsonPath(jsonPath));
    public virtual ISelectable? Regex(string pattern, RegexOptions options = RegexOptions.None, string replacement = "$0") => Select(Selectors.Regex(pattern, options, replacement));
}
