using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ResourceSpider.Core.Selector;

public interface ISelector
{
    ISelectable Select(string text);
    IEnumerable<ISelectable> SelectList(string text);
}

public interface ISelectable
{
    SelectableType Type { get; }
    ISelectable XPath(string xpath);
    ISelectable Css(string css, string attr = null);
    IEnumerable<string> Links();
    ISelectable JsonPath(string jsonPath);
    IEnumerable<ISelectable> Nodes();
    ISelectable Regex(string pattern, RegexOptions options = RegexOptions.None, string replacement = "$0");
    string Value { get; }
    ISelectable Select(ISelector selector);
    IEnumerable<ISelectable> SelectList(ISelector selector);
}
