using HtmlAgilityPack;
using Larpx.PersonalTools.ResourceSpider.Core.Interfaces;
using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Parser;

/// <summary>
/// CSS 选择器解析器，基于 HtmlAgilityPack 实现
/// 通过 CSS 规则列表从 HTML 文档中提取结构化数据
/// </summary>
public class CssSelectorParser : IParser
{
    /// <summary>
    /// CSS 解析规则列表
    /// </summary>
    private readonly List<CssRule> _rules;

    /// <summary>
    /// 初始化 CSS 选择器解析器
    /// </summary>
    /// <param name="rules">CSS 解析规则列表</param>
    public CssSelectorParser(List<CssRule> rules)
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
    /// 使用 CSS 规则解析 HTTP 响应内容，提取结构化数据记录
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

    /// <summary>
    /// 使用 CSS 选择器查询匹配的 HTML 节点集合
    /// 内部将 CSS 选择器转换为 XPath 进行查询
    /// </summary>
    /// <param name="root">根 HTML 节点</param>
    /// <param name="cssSelector">CSS 选择器表达式</param>
    /// <returns>匹配的 HTML 节点列表，查询失败返回 null</returns>
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

    /// <summary>
    /// 从 HTML 节点中提取字段值
    /// 如果指定了属性名则提取属性值，否则提取内部文本
    /// 可选的格式化函数对提取结果进行后处理
    /// </summary>
    /// <param name="node">HTML 节点</param>
    /// <param name="field">CSS 字段定义</param>
    /// <returns>提取的字段值，可能为 null</returns>
    private string? ExtractValue(HtmlNode node, CssField field)
    {
        var value = field.AttributeName != null
            ? node.GetAttributeValue(field.AttributeName, string.Empty)
            : node.InnerText;

        return field.Format?.Invoke(value) ?? value;
    }
}

/// <summary>
/// CSS 解析规则，定义 CSS 选择器与字段提取规则的映射关系
/// </summary>
public class CssRule
{
    /// <summary>
    /// CSS 选择器表达式，用于定位 HTML 元素
    /// </summary>
    public string CssSelector { get; set; } = string.Empty;

    /// <summary>
    /// 字段提取规则列表，定义从匹配元素中提取哪些字段
    /// </summary>
    public List<CssField> Fields { get; set; } = new();
}

/// <summary>
/// CSS 字段定义，描述从 HTML 元素中提取单个字段的方式
/// </summary>
public class CssField
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
