using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Parser;

/// <summary>
/// JSON 解析器，基于 Newtonsoft.Json 实现
/// 通过 JsonPath 表达式从 JSON 响应中提取结构化数据
/// </summary>
public class JsonParser : IParser
{
    /// <summary>
    /// JSON 数组路径，用于定位 JSON 数据中的数组容器
    /// 为 null 时直接解析根对象
    /// </summary>
    private readonly string? _arrayPath;

    /// <summary>
    /// JSON 字段提取规则列表
    /// </summary>
    private readonly List<JsonField> _fields;

    /// <summary>
    /// 初始化 JSON 解析器
    /// </summary>
    /// <param name="arrayPath">JSON 数组路径，可选</param>
    /// <param name="fields">字段提取规则列表</param>
    public JsonParser(string? arrayPath, List<JsonField> fields)
    {
        _arrayPath = arrayPath;
        _fields = fields;
    }

    /// <summary>
    /// 处理数据上下文，解析 JSON 响应内容并将结果添加到上下文中
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
    /// 使用 JsonPath 表达式解析 JSON 响应内容，提取结构化数据记录
    /// 支持从嵌套数组中提取多条记录
    /// </summary>
    /// <param name="response">HTTP 响应对象</param>
    /// <returns>提取的数据记录集合</returns>
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

/// <summary>
/// JSON 字段定义，描述从 JSON 对象中提取单个字段的方式
/// </summary>
public class JsonField
{
    /// <summary>
    /// 字段名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// JsonPath 表达式，用于定位 JSON 对象中的字段
    /// </summary>
    public string JsonPath { get; set; } = string.Empty;

    /// <summary>
    /// 值解析函数，对提取的原始字符串进行类型转换或格式化处理
    /// </summary>
    public Func<string?, object?>? Parse { get; set; }
}
