using AngleSharp;
using AngleSharp.Dom;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Parser;

/// <summary>
/// 基于 AngleSharp 的 CSS 选择器解析器，使用 AngleSharp 库解析 HTML 文档
/// 提供静态方法用于通过 CSS 选择器提取文本内容和属性值
/// </summary>
public class AngleSharpCssParser : IParser
{
    /// <summary>
    /// 解析 HTTP 响应内容，创建基础数据记录
    /// </summary>
    /// <param name="response">HTTP 响应对象</param>
    /// <returns>数据记录集合</returns>
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

    /// <summary>
    /// 使用 AngleSharp 加载 HTML 文档
    /// </summary>
    /// <param name="html">HTML 内容字符串</param>
    /// <returns>解析后的 IDocument 对象</returns>
    private static IDocument LoadDocument(string html)
    {
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        return context.OpenAsync(req => req.Content(html)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 使用 CSS 选择器从 HTML 中提取匹配元素的文本内容
    /// </summary>
    /// <param name="html">HTML 内容字符串</param>
    /// <param name="cssSelector">CSS 选择器表达式</param>
    /// <returns>匹配元素的文本内容列表</returns>
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

    /// <summary>
    /// 使用 CSS 选择器从 HTML 中提取匹配元素的指定属性值
    /// </summary>
    /// <param name="html">HTML 内容字符串</param>
    /// <param name="cssSelector">CSS 选择器表达式</param>
    /// <param name="attributeName">要提取的属性名称</param>
    /// <returns>匹配元素的属性值列表</returns>
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
