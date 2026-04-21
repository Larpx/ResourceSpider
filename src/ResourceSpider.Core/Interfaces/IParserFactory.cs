using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.Interfaces;

public interface IParserFactory
{
    IParser CreateParser(ParserType type);

    IParser CreateFromExpressionConfig(ExpressionConfig config);

    void RegisterCustomParser(string name, IParser parser);
}

public enum ParserType
{
    XPath,
    CssSelector,
    JsonPath,
    Custom
}
