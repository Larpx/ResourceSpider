namespace ResourceSpider.Core.Models;

public class SystemLog
{
    public string LogId { get; set; } = Guid.NewGuid().ToString("N");

    public string Level { get; set; } = "Info";

    public string Category { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public Dictionary<string, object?>? Detail { get; set; }

    public string? UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
