namespace ResourceSpider.Core.Models;

public class CrawlResult
{
    public string ResultId { get; set; } = Guid.NewGuid().ToString("N");

    public string ExecutionId { get; set; } = string.Empty;

    public string TaskId { get; set; } = string.Empty;

    public string? StepId { get; set; }

    public Dictionary<string, object?> ExtractedData { get; set; } = new();

    public string? SourceUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
