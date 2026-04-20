using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ResourceSpider.Infrastructure.Parser;

namespace ResourceSpider.Infrastructure.Parser;

public class DefaultParserFactory : IParserFactory
{
    private readonly Dictionary<string, IParser> _customParsers = new();

    public IParser CreateParser(ParserType type)
    {
        return type switch
        {
            ParserType.XPath => CreateXPathParser(),
            ParserType.CssSelector => CreateCssSelectorParser(),
            ParserType.JsonPath => CreateJsonParser(),
            ParserType.Custom => throw new ArgumentException(
                "Use RegisterCustomParser for custom parsers"),
            _ => throw new ArgumentException($"Unsupported parser type: {type}")
        };
    }

    public void RegisterCustomParser(string name, IParser parser)
    {
        _customParsers[name] = parser;
    }

    public IParser GetCustomParser(string name)
    {
        if (_customParsers.TryGetValue(name, out var parser))
        {
            return parser;
        }
        throw new KeyNotFoundException($"Custom parser '{name}' not found");
    }

    private IParser CreateXPathParser()
    {
        return new XPathParser(new List<XPathRule>());
    }

    private IParser CreateCssSelectorParser()
    {
        return new CssSelectorParser(new List<CssRule>());
    }

    private IParser CreateJsonParser()
    {
        return new JsonParser(null, new List<JsonField>());
    }
}
