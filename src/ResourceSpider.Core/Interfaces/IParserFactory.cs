using ResourceSpider.Core.Interfaces;

namespace ResourceSpider.Core.Interfaces;

public interface IParserFactory
{
    IParser CreateParser(ParserType type);
    
    void RegisterCustomParser(string name, IParser parser);
}

public enum ParserType
{
    XPath,
    CssSelector,
    JsonPath,
    Custom
}
