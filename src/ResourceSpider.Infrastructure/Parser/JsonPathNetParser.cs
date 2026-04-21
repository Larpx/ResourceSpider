using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Parser;

public class JsonPathNetParser : IParser
{
    public IEnumerable<DataRecord> Parse(Response response)
    {
        var textContent = response.TextContent;
        if (string.IsNullOrEmpty(textContent)) yield break;

        var record = new DataRecord
        {
            RequestId = response.RequestId,
            SourceUrl = response.Url
        };

        yield return record;
    }

    public static List<string> Extract(string json, string jsonPath)
    {
        var results = new List<string>();
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(jsonPath)) return results;

        try
        {
            var tokens = Newtonsoft.Json.Linq.JToken.Parse(json).SelectTokens(jsonPath);
            foreach (var token in tokens)
            {
                results.Add(token.ToString());
            }
        }
        catch (Exception)
        {
            return results;
        }

        return results;
    }
}
