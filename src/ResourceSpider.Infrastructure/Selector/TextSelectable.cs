using System;
using System.Collections.Generic;
using System.Linq;
using Larpx.PersonalTools.ResourceSpider.Core.Selector;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Selector;

/// <summary>
/// 文本可选择对象，用于对纯文本内容进行选择操作
/// 支持正则表达式等方式进行文本提取
/// </summary>
public class TextSelectable : Selectable
{
    private readonly string _text;

    /// <summary>
    /// 获取可选择对象类型为文本
    /// </summary>
    public override SelectableType Type => SelectableType.Text;

    /// <summary>
    /// 通过文本字符串初始化文本可选择对象
    /// </summary>
    /// <param name="text">文本内容</param>
    public TextSelectable(string text) { _text = text; }

    /// <summary>
    /// 获取子节点集合（文本节点返回自身）
    /// </summary>
    /// <returns>包含自身的可选择对象集合</returns>
    public override IEnumerable<ISelectable> Nodes() => new[] { new TextSelectable(_text) };

    /// <summary>
    /// 使用正则表达式从文本中提取链接
    /// </summary>
    /// <returns>有效的 URI 字符串集合</returns>
    public override IEnumerable<string> Links()
    {
        var results = new HashSet<string>();
        var links = SelectList(Selectors.Regex(@"href\s*=\s*[""']([^""']+)[""']")).Select(x => x.Value);
        foreach (var link in links) { if (Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out _)) results.Add(link); }
        return results;
    }

    /// <summary>
    /// 获取文本内容
    /// </summary>
    public override string Value => _text;

    /// <summary>
    /// 使用指定选择器选取单个元素
    /// </summary>
    /// <param name="selector">选择器实例</param>
    /// <returns>匹配的可选择对象，未匹配返回 null</returns>
    /// <exception cref="ArgumentNullException">选择器为 null 时抛出</exception>
    public override ISelectable? Select(ISelector selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        return selector.Select(_text);
    }

    /// <summary>
    /// 使用指定选择器选取多个元素
    /// </summary>
    /// <param name="selector">选择器实例</param>
    /// <returns>匹配的可选择对象集合</returns>
    /// <exception cref="ArgumentNullException">选择器为 null 时抛出</exception>
    public override IEnumerable<ISelectable> SelectList(ISelector selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        return selector.SelectList(_text);
    }
}
