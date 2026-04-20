using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Parser;

public class JsonParser : IParser
{
    private readonly string? _arrayPath;
    private readonly List<JsonField> _fields;

    public JsonParser(string? arrayPath, List<JsonField> fields)
    {
        _arrayPath = arrayPath;
        _fields = fields;
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

        var json = JObject.Parse(response.TextContent);
        var tokens = _arrayPath != null 
            ? json.SelectTokens(_arrayPath).Cast<JObject>() 
            : new[] { json };

        foreach (var token in tokens)
        {
            var record = new Dictionary<string, object?>();
            foreach (var field in _fields)
            {
                var value = token[field.JsonPath]?.ToString();
                record[field.Name] = field.Parse != null 
                    ? field.Parse(value) 
                    : value;
            }

            yield return new DataRecord
            {
                Fields = record,
                SourceUrl = response.Url,
                RequestId = response.RequestId
            };
        }
    }
}

public class JsonField
{
    public string Name { get; set; } = string.Empty;
    public string JsonPath { get; set; } = string.Empty;
    public Func<string?, object?>? Parse { get; set; }
}
