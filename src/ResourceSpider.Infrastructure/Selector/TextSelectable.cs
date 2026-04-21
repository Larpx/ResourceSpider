using System;
using System.Collections.Generic;
using System.Linq;
using ResourceSpider.Core.Selector;

namespace ResourceSpider.Infrastructure.Selector;

public class TextSelectable : Selectable
{
    private readonly string _text;

    public override SelectableType Type => SelectableType.Text;

    public TextSelectable(string text) { _text = text; }

    public override IEnumerable<ISelectable> Nodes() => new[] { new TextSelectable(_text) };

    public override IEnumerable<string> Links()
    {
        var results = new HashSet<string>();
        var links = SelectList(Selectors.Regex(@"href\s*=\s*[""']([^""']+)[""']")).Select(x => x.Value);
        foreach (var link in links) { if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out _)) results.Add(link); }
        return results;
    }

    public override string Value => _text;

    public override ISelectable? Select(ISelector selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        return selector.Select(_text);
    }

    public override IEnumerable<ISelectable> SelectList(ISelector selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        return selector.SelectList(_text);
    }
}
