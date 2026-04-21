using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ResourceSpider.Core.Selector;

namespace ResourceSpider.Infrastructure.Selector;

public class JsonPathSelector : ISelector
{
    private readonly string _jsonPath;

    public JsonPathSelector(string jsonPath) { _jsonPath = jsonPath; }

    public ISelectable? Select(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (!(JsonConvert.DeserializeObject(text) is JToken token)) return null;
        var result = token.SelectToken(_jsonPath);
        return result == null ? null : new JsonSelectable(result);
    }

    public IEnumerable<ISelectable> SelectList(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        if (!(JsonConvert.DeserializeObject(text) is JToken token)) return [];
        return token.SelectTokens(_jsonPath).Select(x => new JsonSelectable(x));
    }
}
