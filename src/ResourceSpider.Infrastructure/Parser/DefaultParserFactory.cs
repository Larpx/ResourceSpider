using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ResourceSpider.Infrastructure.Parser;

namespace ResourceSpider.Infrastructure.Parser;

public class DefaultParserFactory : IParserFactory
{
    private readonly Dictionary<string, IParser> _customParsers = new();

    public IParser CreateParser(ParserType type)
    {
        return type switch
        {
            ParserType.XPath => CreateXPathParser(),
            ParserType.CssSelector => CreateCssSelectorParser(),
            ParserType.JsonPath => CreateJsonParser(),
            ParserType.Custom => throw new ArgumentException(
                "Use RegisterCustomParser for custom parsers"),
            _ => throw new ArgumentException($"Unsupported parser type: {type}")
        };
    }

    public IParser CreateFromExpressionConfig(ExpressionConfig config)
    {
        var containerType = Enum.TryParse<ParserType>(config.SelectorType.ToString(), out var ct)
            ? ct : ParserType.XPath;

        var rules = new List<object>();

        switch (containerType)
        {
            case ParserType.XPath:
            {
                var xPathRules = BuildXPathRules(config);
                return new ExpressionDrivenXPathParser(config, xPathRules);
            }
            case ParserType.CssSelector:
            {
                var cssRules = BuildCssRules(config);
                return new ExpressionDrivenCssParser(config, cssRules);
            }
            case ParserType.JsonPath:
            {
                var jsonFields = BuildJsonFields(config);
                return new ExpressionDrivenJsonParser(config, jsonFields);
            }
            default:
                throw new ArgumentException($"Unsupported container selector type: {containerType}");
        }
    }

    public void RegisterCustomParser(string name, IParser parser)
    {
        _customParsers[name] = parser;
    }

    public IParser GetCustomParser(string name)
    {
        if (_customParsers.TryGetValue(name, out var parser))
        {
            return parser;
        }
        throw new KeyNotFoundException($"Custom parser '{name}' not found");
    }

    private IParser CreateXPathParser()
    {
        return new XPathParser(new List<XPathRule>());
    }

    private IParser CreateCssSelectorParser()
    {
        return new CssSelectorParser(new List<CssRule>());
    }

    private IParser CreateJsonParser()
    {
        return new JsonParser(null, new List<JsonField>());
    }

    private static List<XPathRule> BuildXPathRules(ExpressionConfig config)
    {
        var containerExpr = !string.IsNullOrEmpty(config.ContainerExpression)
            ? config.ContainerExpression
            : "//body";

        var rule = new XPathRule
        {
            XPath = containerExpr,
            Fields = config.Fields.Select(f => new XPathField
            {
                Name = f.FieldName,
                AttributeName = f.AttributeName,
                Format = BuildFormatter(f)
            }).ToList()
        };

        return new List<XPathRule> { rule };
    }

    private static List<CssRule> BuildCssRules(ExpressionConfig config)
    {
        var containerExpr = !string.IsNullOrEmpty(config.ContainerExpression)
            ? config.ContainerExpression
            : "body";

        var rule = new CssRule
        {
            CssSelector = containerExpr,
            Fields = config.Fields.Select(f => new CssField
            {
                Name = f.FieldName,
                AttributeName = f.AttributeName,
                Format = BuildFormatter(f)
            }).ToList()
        };

        return new List<CssRule> { rule };
    }

    private static List<JsonField> BuildJsonFields(ExpressionConfig config)
    {
        return config.Fields.Select(f => new JsonField
        {
            Name = f.FieldName,
            JsonPath = f.Expression
        }).ToList();
    }

    private static Func<string, string>? BuildFormatter(ExpressionField field)
    {
        if (string.IsNullOrEmpty(field.Formatter)) return null;

        return field.Formatter.ToLower() switch
        {
            "trim" => v => v?.Trim() ?? string.Empty,
            "replace" when !string.IsNullOrEmpty(field.FormatterArgs) => v =>
            {
                var parts = field.FormatterArgs.Split('|');
                return parts.Length >= 2 ? v?.Replace(parts[0], parts[1]) ?? string.Empty : v ?? string.Empty;
            },
            "regex" when !string.IsNullOrEmpty(field.FormatterArgs) => v =>
            {
                try
                {
                    var match = System.Text.RegularExpressions.Regex.Match(v ?? string.Empty, field.FormatterArgs);
                    return match.Success ? match.Value : v ?? string.Empty;
                }
                catch
                {
                    return v ?? string.Empty;
                }
            },
            _ => null
        };
    }
}

public class ExpressionDrivenXPathParser : IParser
{
    private readonly ExpressionConfig _config;
    private readonly List<XPathRule> _rules;

    public ExpressionDrivenXPathParser(ExpressionConfig config, List<XPathRule> rules)
    {
        _config = config;
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

        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(response.TextContent);

        var containerExpr = !string.IsNullOrEmpty(_config.ContainerExpression)
            ? _config.ContainerExpression
            : "//body";

        var containerNodes = doc.DocumentNode.SelectNodes(containerExpr);
        if (containerNodes == null) yield break;

        foreach (var containerNode in containerNodes)
        {
            var record = new DataRecord
            {
                ExpressionId = _config.ExpressionId,
                SourceUrl = response.Url,
                RequestId = response.RequestId
            };

            foreach (var field in _config.Fields.OrderBy(f => f.Order))
            {
                string? value = null;

                if (field.SelectorType == Core.Enums.ExpressionType.Environment)
                {
                    value = ResolveEnvironmentValue(field.Expression);
                }
                else
                {
                    var fieldExpr = field.Expression;
                    if (!string.IsNullOrEmpty(fieldExpr))
                    {
                        var nodes = containerNode.SelectNodes(fieldExpr);
                        if (nodes != null && nodes.Count > 0)
                        {
                            value = !string.IsNullOrEmpty(field.AttributeName)
                                ? nodes[0].GetAttributeValue(field.AttributeName, string.Empty)
                                : nodes[0].InnerText;
                        }
                    }
                }

                if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(field.DefaultValue))
                {
                    value = field.DefaultValue;
                }

                if (string.IsNullOrEmpty(value) && field.IsRequired)
                {
                    record = null;
                    break;
                }

                if (record != null)
                {
                    record.Fields[field.FieldName] = value;
                    record.FieldExpressionMap[field.FieldName] = field.Expression;
                }
            }

            if (record != null)
            {
                yield return record;
            }
        }
    }

    private static string? ResolveEnvironmentValue(string? expression)
    {
        return expression?.ToUpper() switch
        {
            "GUID" => Guid.NewGuid().ToString(),
            "DATETIME" or "NOW" => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            "DATE" or "TODAY" => DateTime.UtcNow.ToString("yyyy-MM-dd"),
            _ => null
        };
    }
}

public class ExpressionDrivenCssParser : IParser
{
    private readonly ExpressionConfig _config;
    private readonly List<CssRule> _rules;

    public ExpressionDrivenCssParser(ExpressionConfig config, List<CssRule> rules)
    {
        _config = config;
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

        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(response.TextContent);

        var containerExpr = !string.IsNullOrEmpty(_config.ContainerExpression)
            ? _config.ContainerExpression
            : "body";

        var xpath = CssToXPath(containerExpr);
        var containerNodes = doc.DocumentNode.SelectNodes(xpath);
        if (containerNodes == null) yield break;

        foreach (var containerNode in containerNodes)
        {
            var record = new DataRecord
            {
                ExpressionId = _config.ExpressionId,
                SourceUrl = response.Url,
                RequestId = response.RequestId
            };

            foreach (var field in _config.Fields.OrderBy(f => f.Order))
            {
                string? value = null;
                var fieldExpr = field.Expression;
                if (!string.IsNullOrEmpty(fieldExpr))
                {
                    var fieldXpath = CssToXPath(fieldExpr);
                    var nodes = containerNode.SelectNodes(fieldXpath);
                    if (nodes != null && nodes.Count > 0)
                    {
                        value = !string.IsNullOrEmpty(field.AttributeName)
                            ? nodes[0].GetAttributeValue(field.AttributeName, string.Empty)
                            : nodes[0].InnerText;
                    }
                }

                if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(field.DefaultValue))
                {
                    value = field.DefaultValue;
                }

                if (string.IsNullOrEmpty(value) && field.IsRequired)
                {
                    record = null;
                    break;
                }

                if (record != null)
                {
                    record.Fields[field.FieldName] = value;
                    record.FieldExpressionMap[field.FieldName] = field.Expression;
                }
            }

            if (record != null)
            {
                yield return record;
            }
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
}

public class ExpressionDrivenJsonParser : IParser
{
    private readonly ExpressionConfig _config;
    private readonly List<JsonField> _fields;

    public ExpressionDrivenJsonParser(ExpressionConfig config, List<JsonField> fields)
    {
        _config = config;
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

        var json = Newtonsoft.Json.Linq.JObject.Parse(response.TextContent);
        var arrayPath = !string.IsNullOrEmpty(_config.ContainerExpression)
            ? _config.ContainerExpression
            : null;

        var tokens = arrayPath != null
            ? json.SelectTokens(arrayPath).Cast<Newtonsoft.Json.Linq.JObject>()
            : new[] { json };

        foreach (var token in tokens)
        {
            var record = new DataRecord
            {
                ExpressionId = _config.ExpressionId,
                SourceUrl = response.Url,
                RequestId = response.RequestId
            };

            foreach (var field in _config.Fields.OrderBy(f => f.Order))
            {
                var value = token[field.Expression]?.ToString();

                if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(field.DefaultValue))
                {
                    value = field.DefaultValue;
                }

                if (string.IsNullOrEmpty(value) && field.IsRequired)
                {
                    record = null;
                    break;
                }

                if (record != null)
                {
                    record.Fields[field.FieldName] = value;
                    record.FieldExpressionMap[field.FieldName] = field.Expression;
                }
            }

            if (record != null)
            {
                yield return record;
            }
        }
    }
}
