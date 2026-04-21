using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

public class ExtractionRule
{
    public string FieldName { get; set; } = string.Empty;

    public ExpressionType ExpressionType { get; set; }

    public string Expression { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public string? DefaultValue { get; set; }

    public List<TransformRule>? TransformRules { get; set; }

    public bool IsArray { get; set; }
}

public class TransformRule
{
    public string Type { get; set; } = string.Empty;

    public string? Pattern { get; set; }

    public string? Replacement { get; set; }
}
