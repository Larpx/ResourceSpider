namespace ResourceSpider.Core.Selector;

public enum SelectorType
{
    XPath,
    Regex,
    Css,
    JsonPath,
    Environment
}

public enum SelectableType
{
    Text,
    Html,
    Json
}
