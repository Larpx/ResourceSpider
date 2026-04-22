using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ResourceSpider.Infrastructure.Parser;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace ResourceSpider.Infrastructure.Parser;

/// <summary>
/// 默认解析器工厂实现，根据解析器类型或表达式配置创建对应的解析器实例
/// 支持 XPath、CSS 选择器、JsonPath 三种内置解析器，以及自定义解析器注册
/// </summary>
public class DefaultParserFactory : IParserFactory
{
    /// <summary>
    /// 自定义解析器注册表，键为解析器名称，值为解析器实例
    /// </summary>
    private readonly Dictionary<string, IParser> _customParsers = new();

    /// <summary>
    /// 根据解析器类型创建对应的解析器实例
    /// </summary>
    /// <param name="type">解析器类型（XPath、CssSelector、JsonPath、Custom）</param>
    /// <returns>解析器实例</returns>
    /// <exception cref="ArgumentException">当传入 Custom 类型或不支持的类型时抛出</exception>
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

    /// <summary>
    /// 根据表达式配置创建对应的表达式驱动解析器
    /// 根据配置中的选择器类型自动选择 XPath、CSS 或 JSON 解析器
    /// </summary>
    /// <param name="config">表达式配置，包含选择器类型、容器表达式和字段定义</param>
    /// <returns>表达式驱动的解析器实例</returns>
    /// <exception cref="ArgumentException">当配置了不支持的选择器类型时抛出</exception>
    public IParser CreateFromExpressionConfig(ExpressionConfig config)
    {
        var containerType = Enum.TryParse<ParserType>(config.SelectorType.ToString(), out var ct)
            ? ct : ParserType.XPath;

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

    /// <summary>
    /// 注册自定义解析器到工厂中
    /// </summary>
    /// <param name="name">解析器名称，用于后续获取</param>
    /// <param name="parser">解析器实例</param>
    public void RegisterCustomParser(string name, IParser parser)
    {
        _customParsers[name] = parser;
    }

    /// <summary>
    /// 根据名称获取已注册的自定义解析器
    /// </summary>
    /// <param name="name">解析器名称</param>
    /// <returns>解析器实例</returns>
    /// <exception cref="KeyNotFoundException">当指定名称的解析器未注册时抛出</exception>
    public IParser GetCustomParser(string name)
    {
        if (_customParsers.TryGetValue(name, out var parser))
        {
            return parser;
        }
        throw new KeyNotFoundException($"Custom parser '{name}' not found");
    }

    /// <summary>
    /// 创建空的 XPath 解析器实例
    /// </summary>
    /// <returns>XPath 解析器实例</returns>
    private IParser CreateXPathParser()
    {
        return new XPathParser(new List<XPathRule>());
    }

    /// <summary>
    /// 创建空的 CSS 选择器解析器实例
    /// </summary>
    /// <returns>CSS 选择器解析器实例</returns>
    private IParser CreateCssSelectorParser()
    {
        return new CssSelectorParser(new List<CssRule>());
    }

    /// <summary>
    /// 创建空的 JSON 解析器实例
    /// </summary>
    /// <returns>JSON 解析器实例</returns>
    private IParser CreateJsonParser()
    {
        return new JsonParser(null, new List<JsonField>());
    }

    /// <summary>
    /// 根据表达式配置构建 XPath 规则列表
    /// 将容器表达式作为根 XPath，字段表达式映射为 XPathField
    /// </summary>
    /// <param name="config">表达式配置</param>
    /// <returns>XPath 规则列表</returns>
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

    /// <summary>
    /// 根据表达式配置构建 CSS 规则列表
    /// 将容器表达式作为根 CSS 选择器，字段表达式映射为 CssField
    /// </summary>
    /// <param name="config">表达式配置</param>
    /// <returns>CSS 规则列表</returns>
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

    /// <summary>
    /// 根据表达式配置构建 JSON 字段列表
    /// 将字段表达式直接映射为 JsonPath 表达式
    /// </summary>
    /// <param name="config">表达式配置</param>
    /// <returns>JSON 字段列表</returns>
    private static List<JsonField> BuildJsonFields(ExpressionConfig config)
    {
        return config.Fields.Select(f => new JsonField
        {
            Name = f.FieldName,
            JsonPath = f.Expression
        }).ToList();
    }

    /// <summary>
    /// 根据表达式字段的格式化器配置构建格式化函数
    /// 支持 trim（去除空白）、replace（字符串替换）、regex（正则匹配）三种格式化器
    /// </summary>
    /// <param name="field">表达式字段定义</param>
    /// <returns>格式化函数，无格式化器时返回 null</returns>
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

/// <summary>
/// 表达式驱动的 XPath 解析器，根据表达式配置从 HTML 文档中提取数据
/// 支持环境变量解析（GUID、DATETIME、DATE）和必填字段验证
/// </summary>
public class ExpressionDrivenXPathParser : IParser
{
    /// <summary>
    /// 表达式配置
    /// </summary>
    private readonly ExpressionConfig _config;

    /// <summary>
    /// XPath 规则列表
    /// </summary>
    private readonly List<XPathRule> _rules;

    /// <summary>
    /// 初始化表达式驱动的 XPath 解析器
    /// </summary>
    /// <param name="config">表达式配置</param>
    /// <param name="rules">XPath 规则列表</param>
    public ExpressionDrivenXPathParser(ExpressionConfig config, List<XPathRule> rules)
    {
        _config = config;
        _rules = rules;
    }

    /// <summary>
    /// 处理数据上下文，解析响应内容并将结果添加到上下文中
    /// </summary>
    /// <param name="context">数据上下文</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    public Task HandleAsync(DataContext context, CancellationToken ct = default)
    {
        if (context?.Response == null) return Task.CompletedTask;

        var records = Parse(context.Response);
        context.DataRecords.AddRange(records);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 使用 XPath 表达式解析 HTML 响应内容
    /// 按容器表达式定位元素组，再按字段表达式提取各字段值
    /// 支持环境变量字段、默认值填充和必填字段验证
    /// </summary>
    /// <param name="response">HTTP 响应对象</param>
    /// <returns>提取的数据记录集合，必填字段缺失时跳过该记录</returns>
    public IEnumerable<DataRecord> Parse(Response response)
    {
        if (response.TextContent == null) yield break;

        var doc = new HtmlDocument();
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

    /// <summary>
    /// 解析环境变量表达式，支持 GUID、DATETIME/NOW、DATE/TODAY
    /// </summary>
    /// <param name="expression">环境变量表达式</param>
    /// <returns>解析后的值，不匹配时返回 null</returns>
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

/// <summary>
/// 表达式驱动的 CSS 选择器解析器，根据表达式配置从 HTML 文档中提取数据
/// 内部将 CSS 选择器转换为 XPath 进行查询，支持必填字段验证
/// </summary>
public class ExpressionDrivenCssParser : IParser
{
    /// <summary>
    /// 表达式配置
    /// </summary>
    private readonly ExpressionConfig _config;

    /// <summary>
    /// CSS 规则列表
    /// </summary>
    private readonly List<CssRule> _rules;

    /// <summary>
    /// 初始化表达式驱动的 CSS 选择器解析器
    /// </summary>
    /// <param name="config">表达式配置</param>
    /// <param name="rules">CSS 规则列表</param>
    public ExpressionDrivenCssParser(ExpressionConfig config, List<CssRule> rules)
    {
        _config = config;
        _rules = rules;
    }

    /// <summary>
    /// 处理数据上下文，解析响应内容并将结果添加到上下文中
    /// </summary>
    /// <param name="context">数据上下文</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    public Task HandleAsync(DataContext context, CancellationToken ct = default)
    {
        if (context?.Response == null) return Task.CompletedTask;

        var records = Parse(context.Response);
        context.DataRecords.AddRange(records);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 使用 CSS 选择器解析 HTML 响应内容
    /// 按容器 CSS 选择器定位元素组，再按字段 CSS 选择器提取各字段值
    /// 支持默认值填充和必填字段验证
    /// </summary>
    /// <param name="response">HTTP 响应对象</param>
    /// <returns>提取的数据记录集合，必填字段缺失时跳过该记录</returns>
    public IEnumerable<DataRecord> Parse(Response response)
    {
        if (response.TextContent == null) yield break;

        var doc = new HtmlDocument();
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

    /// <summary>
    /// 将简单的 CSS 选择器转换为 XPath 表达式
    /// 支持 ID 选择器（#id）、类选择器（.class）和标签选择器
    /// </summary>
    /// <param name="cssSelector">CSS 选择器表达式</param>
    /// <returns>对应的 XPath 表达式</returns>
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

/// <summary>
/// 表达式驱动的 JSON 解析器，根据表达式配置从 JSON 响应中提取数据
/// 支持通过容器表达式定位 JSON 数组，以及必填字段验证
/// </summary>
public class ExpressionDrivenJsonParser : IParser
{
    /// <summary>
    /// 表达式配置
    /// </summary>
    private readonly ExpressionConfig _config;

    /// <summary>
    /// JSON 字段列表
    /// </summary>
    private readonly List<JsonField> _fields;

    /// <summary>
    /// 初始化表达式驱动的 JSON 解析器
    /// </summary>
    /// <param name="config">表达式配置</param>
    /// <param name="fields">JSON 字段列表</param>
    public ExpressionDrivenJsonParser(ExpressionConfig config, List<JsonField> fields)
    {
        _config = config;
        _fields = fields;
    }

    /// <summary>
    /// 处理数据上下文，解析响应内容并将结果添加到上下文中
    /// </summary>
    /// <param name="context">数据上下文</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    public Task HandleAsync(DataContext context, CancellationToken ct = default)
    {
        if (context?.Response == null) return Task.CompletedTask;

        var records = Parse(context.Response);
        context.DataRecords.AddRange(records);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 使用 JsonPath 表达式解析 JSON 响应内容
    /// 按容器表达式定位 JSON 对象数组，再按字段表达式提取各字段值
    /// 支持默认值填充和必填字段验证
    /// </summary>
    /// <param name="response">HTTP 响应对象</param>
    /// <returns>提取的数据记录集合，必填字段缺失时跳过该记录</returns>
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
