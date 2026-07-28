using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Core.Interfaces;

/// <summary>
/// 解析器工厂接口，根据解析类型或表达式配置创建对应的解析器实例
/// </summary>
public interface IParserFactory
{
    /// <summary>
    /// 根据解析类型创建解析器
    /// </summary>
    /// <param name="type">解析器类型</param>
    /// <returns>解析器实例</returns>
    IParser CreateParser(ParserType type);

    /// <summary>
    /// 根据表达式配置创建解析器
    /// </summary>
    /// <param name="config">表达式配置</param>
    /// <returns>解析器实例</returns>
    IParser CreateFromExpressionConfig(ExpressionConfig config);

    /// <summary>
    /// 注册自定义解析器
    /// </summary>
    /// <param name="name">解析器名称</param>
    /// <param name="parser">解析器实例</param>
    void RegisterCustomParser(string name, IParser parser);
}

/// <summary>
/// 解析器类型枚举，定义支持的解析方式
/// </summary>
public enum ParserType
{
    /// <summary>
    /// XPath 解析器
    /// </summary>
    XPath,

    /// <summary>
    /// CSS 选择器解析器
    /// </summary>
    CssSelector,

    /// <summary>
    /// JSONPath 解析器
    /// </summary>
    JsonPath,

    /// <summary>
    /// 自定义解析器
    /// </summary>
    Custom
}
