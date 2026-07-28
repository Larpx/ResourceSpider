using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Larpx.PersonalTools.ResourceSpider.Core.Selector;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Selector;

/// <summary>
/// JSON 可选择对象，基于 Newtonsoft.Json 的 JToken 实现
/// 支持 JsonPath 等方式对 JSON 数据进行元素选取
/// </summary>
public class JsonSelectable(JToken token) : Selectable
{
    /// <summary>
    /// JSON 不支持链接提取，调用时抛出异常
    /// </summary>
    /// <exception cref="NotImplementedException">始终抛出</exception>
    public override IEnumerable<string> Links() => throw new System.NotImplementedException();

    /// <summary>
    /// 获取子节点集合
    /// </summary>
    /// <returns>子 JSON 节点的可选择对象集合</returns>
    public override IEnumerable<ISelectable> Nodes() => token.Children().Select(x => new JsonSelectable(x));

    /// <summary>
    /// 获取当前 JSON 节点的字符串表示
    /// </summary>
    public override string Value => token?.ToString() ?? string.Empty;

    /// <summary>
    /// 使用指定选择器选取单个元素
    /// </summary>
    /// <param name="selector">选择器实例</param>
    /// <returns>匹配的可选择对象，未匹配返回 null</returns>
    /// <exception cref="ArgumentNullException">选择器为 null 时抛出</exception>
    public override ISelectable? Select(ISelector selector)
    {
        if (selector == null) throw new System.ArgumentNullException(nameof(selector));
        return selector.Select(token.ToString());
    }

    /// <summary>
    /// 使用指定选择器选取多个元素
    /// </summary>
    /// <param name="selector">选择器实例</param>
    /// <returns>匹配的可选择对象集合</returns>
    /// <exception cref="ArgumentNullException">选择器为 null 时抛出</exception>
    public override IEnumerable<ISelectable> SelectList(ISelector selector)
    {
        if (selector == null) throw new System.ArgumentNullException(nameof(selector));
        return selector.SelectList(token.ToString());
    }

    /// <summary>
    /// 获取可选择对象类型为 JSON
    /// </summary>
    public override SelectableType Type => SelectableType.Json;
}
