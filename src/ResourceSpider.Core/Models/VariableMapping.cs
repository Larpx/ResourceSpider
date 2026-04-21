namespace ResourceSpider.Core.Models;

public class VariableMapping
{
    public string SourceField { get; set; } = string.Empty;

    public string TargetVariable { get; set; } = string.Empty;

    public string? Transform { get; set; }
}
