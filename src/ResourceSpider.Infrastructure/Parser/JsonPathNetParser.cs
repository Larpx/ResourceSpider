using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Parser;

/// <summary>
/// JsonPath 解析器，基于 Newtonsoft.Json 的 SelectTokens 实现
/// 提供静态方法用于通过 JsonPath 表达式从 JSON 中提取数据
/// </summary>
public class JsonPathNetParser : IParser
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

        var record = new DataRecord
        {
            RequestId = response.RequestId,
            SourceUrl = response.Url
        };

        yield return record;
    }

    /// <summary>
    /// 使用 JsonPath 表达式从 JSON 字符串中提取匹配的数据
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <param name="jsonPath">JsonPath 表达式</param>
    /// <returns>匹配结果的字符串列表，解析失败返回空列表</returns>
    public static List<string> Extract(string json, string jsonPath)
    {
        var results = new List<string>();
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(jsonPath)) return results;

        try
        {
            var tokens = Newtonsoft.Json.Linq.JToken.Parse(json).SelectTokens(jsonPath);
            foreach (var token in tokens)
            {
                results.Add(token.ToString());
            }
        }
        catch (Exception)
        {
            return results;
        }

        return results;
    }
}
