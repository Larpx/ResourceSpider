using HtmlAgilityPack;
using Larpx.PersonalTools.ResourceSpider.Core.Interfaces;
using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Parser;

/// <summary>
/// XPath 解析器，基于 HtmlAgilityPack 实现
/// 通过 XPath 规则列表从 HTML 文档中提取结构化数据
/// </summary>
public class XPathParser : IParser
{
    /// <summary>
    /// XPath 解析规则列表
    /// </summary>
    private readonly List<XPathRule> _rules;

    /// <summary>
    /// 初始化 XPath 解析器
    /// </summary>
    /// <param name="rules">XPath 解析规则列表</param>
    public XPathParser(List<XPathRule> rules)
    {
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
    /// 使用 XPath 规则解析 HTTP 响应内容，提取结构化数据记录
    /// </summary>
    /// <param name="response">HTTP 响应对象</param>
    /// <returns>提取的数据记录集合</returns>
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

    /// <summary>
    /// 从 HTML 节点中提取字段值
    /// 如果指定了属性名则提取属性值，否则提取内部文本
    /// 可选的格式化函数对提取结果进行后处理
    /// </summary>
    /// <param name="node">HTML 节点</param>
    /// <param name="field">XPath 字段定义</param>
    /// <returns>提取的字段值，可能为 null</returns>
    private string? ExtractValue(HtmlNode node, XPathField field)
    {
        var value = field.AttributeName != null 
            ? node.GetAttributeValue(field.AttributeName, string.Empty)
            : node.InnerText;

        return field.Format?.Invoke(value) ?? value;
    }
}

/// <summary>
/// XPath 解析规则，定义 XPath 表达式与字段提取规则的映射关系
/// </summary>
public class XPathRule
{
    /// <summary>
    /// XPath 表达式，用于定位 HTML 元素节点集合
    /// </summary>
    public string XPath { get; set; } = string.Empty;

    /// <summary>
    /// 字段提取规则列表，定义从匹配节点中提取哪些字段
    /// </summary>
    public List<XPathField> Fields { get; set; } = new();
}

/// <summary>
/// XPath 字段定义，描述从 HTML 节点中提取单个字段的方式
/// </summary>
public class XPathField
{
    /// <summary>
    /// 字段名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 要提取的 HTML 属性名，为 null 时提取元素内部文本
    /// </summary>
    public string? AttributeName { get; set; }

    /// <summary>
    /// 值格式化函数，对提取的原始值进行后处理（如去除空白、正则匹配等）
    /// </summary>
    public Func<string, string>? Format { get; set; }
}
