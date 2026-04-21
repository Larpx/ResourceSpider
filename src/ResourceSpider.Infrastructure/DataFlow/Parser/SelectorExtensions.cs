using System;
using System.Text.RegularExpressions;
using ResourceSpider.Core.Selector;
using ResourceSpider.Infrastructure.Selector;

namespace ResourceSpider.Infrastructure.DataFlow.Parser;

public static class SelectorExtensions
{
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

    private static void NotNullExpression(SelectorAttribute selector)
    {
        if (string.IsNullOrWhiteSpace(selector.Expression))
            throw new ArgumentException($"Expression of {selector.Type} selector should not be null/empty");
    }

    public static string? GetText(this ISelectable selectable) => selectable?.Value;
}
