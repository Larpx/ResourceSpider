using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using ResourceSpider.Core.Selector;

namespace ResourceSpider.Infrastructure.Selector;

public class JsonSelectable(JToken token) : Selectable
{
    public override IEnumerable<string> Links() => throw new System.NotImplementedException();
    public override IEnumerable<ISelectable> Nodes() => token.Children().Select(x => new JsonSelectable(x));
    public override string Value => token?.ToString() ?? string.Empty;

    public override ISelectable? Select(ISelector selector)
    {
        if (selector == null) throw new System.ArgumentNullException(nameof(selector));
        return selector.Select(token.ToString());
    }

    public override IEnumerable<ISelectable> SelectList(ISelector selector)
    {
        if (selector == null) throw new System.ArgumentNullException(nameof(selector));
        return selector.SelectList(token.ToString());
    }

    public override SelectableType Type => SelectableType.Json;
}
