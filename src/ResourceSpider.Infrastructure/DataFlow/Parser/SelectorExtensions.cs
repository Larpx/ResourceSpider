using System;
using System.Text.RegularExpressions;
using ResourceSpider.Core.Selector;
using ResourceSpider.Infrastructure.Selector;

namespace ResourceSpider.Infrastructure.DataFlow.Parser;

/// <summary>
/// 选择器属性扩展方法，提供选择器属性到选择器实例的转换和文本提取功能
/// </summary>
public static class SelectorExtensions
{
    /// <summary>
    /// 将选择器属性转换为对应的选择器实例
    /// </summary>
    /// <param name="selector">选择器属性</param>
    /// <returns>选择器实例，属性为 null 时返回 null</returns>
    /// <exception cref="NotSupportedException">不支持的选择器类型时抛出</exception>
    public static ISelector? ToSelector(this SelectorAttribute selector)
    {
        if (selector == null) return null;
        var expression = selector.Expression;
        switch (selector.Type)
        {
            case SelectorType.Css: NotNullExpression(selector); return Selectors.Css(expression);
            case SelectorType.JsonPath: NotNullExpression(selector); return Selectors.JsonPath(expression);
            case SelectorType.Regex:
                NotNullExpression(selector);
                if (string.IsNullOrEmpty(selector.Arguments)) return Selectors.Regex(expression);
                var arguments = selector.Arguments.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var options = (RegexOptions)Enum.Parse(typeof(RegexOptions), arguments[0]);
                var replacement = arguments.Length > 1 ? arguments[1] : "$0";
                return Selectors.Regex(expression, options, replacement);
            case SelectorType.XPath: NotNullExpression(selector); return Selectors.XPath(expression);
            default: throw new NotSupportedException($"{selector.Type} unsupported");
        }
    }

    /// <summary>
    /// 验证选择器属性的表达式不为空
    /// </summary>
    /// <param name="selector">选择器属性</param>
    /// <exception cref="ArgumentException">表达式为空时抛出</exception>
    private static void NotNullExpression(SelectorAttribute selector)
    {
        if (string.IsNullOrWhiteSpace(selector.Expression))
            throw new ArgumentException($"Expression of {selector.Type} selector should not be null/empty");
    }

    /// <summary>
    /// 获取可选择对象的文本值
    /// </summary>
    /// <param name="selectable">可选择对象</param>
    /// <returns>文本值，对象为 null 时返回 null</returns>
    public static string? GetText(this ISelectable selectable) => selectable?.Value;
}
