using HtmlAgilityPack;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Parser;

public class XPathParser : IParser
{
    private readonly List<XPathRule> _rules;

    public XPathParser(List<XPathRule> rules)
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
            var nodes = doc.DocumentNode.SelectNodes(rule.XPath);
            if (nodes == null) continue;

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

    private string? ExtractValue(HtmlNode node, XPathField field)
    {
        var value = field.AttributeName != null 
            ? node.GetAttributeValue(field.AttributeName, string.Empty)
            : node.InnerText;

        return field.Format?.Invoke(value) ?? value;
    }
}

public class XPathRule
{
    public string XPath { get; set; } = string.Empty;
    public List<XPathField> Fields { get; set; } = new();
}

public class XPathField
{
    public string Name { get; set; } = string.Empty;
    public string? AttributeName { get; set; }
    public Func<string, string>? Format { get; set; }
}
