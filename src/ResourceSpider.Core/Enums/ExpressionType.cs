namespace ResourceSpider.Core.Enums;

public enum ExpressionType
{
    XPath,
    CssSelector,
    JsonPath,
    Regex,
    Environment
}

public enum ExpressionStatus
{
    Active = 1,
    Invalid = 2,
    Deprecated = 3,
    Testing = 4
}
