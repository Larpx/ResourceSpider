namespace ResourceSpider.Core.Models;

public class DataRecord
{
    public string RecordId { get; set; } = Guid.NewGuid().ToString("N");

    public string? TaskId { get; set; }

    public string? RequestId { get; set; }

    public string? StepId { get; set; }

    public string? ExpressionId { get; set; }

    public Dictionary<string, object?> Fields { get; set; } = new();

    public Dictionary<string, string> FieldExpressionMap { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? SourceUrl { get; set; }

    public string? AgentId { get; set; }
}
