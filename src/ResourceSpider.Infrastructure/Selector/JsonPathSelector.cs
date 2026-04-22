using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ResourceSpider.Core.Selector;

namespace ResourceSpider.Infrastructure.Selector;

/// <summary>
/// JsonPath 选择器实现，使用 Newtonsoft.Json 的 SelectToken/SelectTokens 方法
/// 对 JSON 数据进行 JsonPath 查询
/// </summary>
public class JsonPathSelector : ISelector
{
    private readonly string _jsonPath;

    /// <summary>
    /// 通过 JsonPath 表达式初始化
    /// </summary>
    /// <param name="jsonPath">JsonPath 表达式</param>
    public JsonPathSelector(string jsonPath) { _jsonPath = jsonPath; }

    /// <summary>
    /// 使用 JsonPath 选取单个元素
    /// </summary>
    /// <param name="text">JSON 内容字符串</param>
    /// <returns>匹配的可选择对象，未匹配返回 null</returns>
    public ISelectable? Select(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (!(JsonConvert.DeserializeObject(text) is JToken token)) return null;
        var result = token.SelectToken(_jsonPath);
        return result == null ? null : new JsonSelectable(result);
    }

    /// <summary>
    /// 使用 JsonPath 选取多个元素
    /// </summary>
    /// <param name="text">JSON 内容字符串</param>
    /// <returns>匹配的可选择对象集合</returns>
    public IEnumerable<ISelectable> SelectList(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        if (!(JsonConvert.DeserializeObject(text) is JToken token)) return [];
        return token.SelectTokens(_jsonPath).Select(x => new JsonSelectable(x));
    }
}
