using HtmlAgilityPack;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Parser;

public class CssSelectorParser : IParser
{
    private readonly List<CssRule> _rules;

    public CssSelectorParser(List<CssRule> rules)
    {
        _rules = rules;
    }

    public Task HandleAsync(DataContext context, CancellationToken ct = default)
    {
        if (context?.Response == null) return Task.CompletedTask;
        
        var records = Parse(context.Response);
        context.DataRecords.AddRange(records);
        return Task.CompletedTask;
    }

    public IEnumerable<DataRecord> Parse(Response response)
    {
        if (response.TextContent == null) yield break;

        var doc = new HtmlDocument();
        doc.LoadHtml(response.TextContent);

        var records = new List<Dictionary<string, object?>>();

        foreach (var rule in _rules)
        {
            var nodes = QuerySelectorAll(doc.DocumentNode, rule.CssSelector);
            if (nodes == null || !nodes.Any()) continue;

            foreach (var node in nodes)
            {
                var record = new Dictionary<string, object?>();
                foreach (var field in rule.Fields)
                {
                    var value = ExtractValue(node, field);
                    record[field.Name] = value;
                }
                records.Add(record);
            }
        }

        foreach (var record in records)
        {
            yield return new DataRecord
            {
                Fields = record,
                SourceUrl = response.Url,
                RequestId = response.RequestId
            };
        }
    }

    private static List<HtmlNode>? QuerySelectorAll(HtmlNode root, string cssSelector)
    {
        var xpath = CssToXPath(cssSelector);
        try
        {
            return root.SelectNodes(xpath)?.ToList();
        }
        catch
        {
            return null;
        }
    }

    private static string CssToXPath(string cssSelector)
    {
        if (cssSelector.StartsWith("#"))
        {
            return $"//*[@id='{cssSelector.Substring(1)}']";
        }
        if (cssSelector.StartsWith("."))
        {
            return $"//*[contains(concat(' ', normalize-space(@class), ' '), ' {cssSelector.Substring(1)} ')]";
        }
        return $"//{cssSelector}";
    }

    private string? ExtractValue(HtmlNode node, CssField field)
    {
        var value = field.AttributeName != null
            ? node.GetAttributeValue(field.AttributeName, string.Empty)
            : node.InnerText;

        return field.Format?.Invoke(value) ?? value;
    }
}

public class CssRule
{
    public string CssSelector { get; set; } = string.Empty;
    public List<CssField> Fields { get; set; } = new();
}

public class CssField
{
    public string Name { get; set; } = string.Empty;
    public string? AttributeName { get; set; }
    public Func<string, string>? Format { get; set; }
}
