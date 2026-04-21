using AngleSharp;
using AngleSharp.Dom;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Parser;

public class AngleSharpCssParser : IParser
{
    public IEnumerable<DataRecord> Parse(Response response)
    {
        var textContent = response.TextContent;
        if (string.IsNullOrEmpty(textContent)) yield break;

        var document = LoadDocument(textContent);

        var record = new DataRecord
        {
            RequestId = response.RequestId,
            SourceUrl = response.Url
        };

        yield return record;
    }

    private static IDocument LoadDocument(string html)
    {
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        return context.OpenAsync(req => req.Content(html)).GetAwaiter().GetResult();
    }

    public static List<string> Extract(string html, string cssSelector)
    {
        var results = new List<string>();
        if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(cssSelector)) return results;

        var document = LoadDocument(html);
        var elements = document.QuerySelectorAll(cssSelector);

        foreach (var element in elements)
        {
            results.Add(element.TextContent.Trim());
        }

        return results;
    }

    public static List<string> ExtractAttribute(string html, string cssSelector, string attributeName)
    {
        var results = new List<string>();
        if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(cssSelector)) return results;

        var document = LoadDocument(html);
        var elements = document.QuerySelectorAll(cssSelector);

        foreach (var element in elements)
        {
            var attrValue = element.GetAttribute(attributeName);
            if (attrValue != null)
            {
                results.Add(attrValue);
            }
        }

        return results;
    }
}
