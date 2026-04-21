namespace ResourceSpider.Core.Models;

public class ConfigVersion
{
    public string VersionId { get; set; } = Guid.NewGuid().ToString("N");

    public string TaskId { get; set; } = string.Empty;

    public int Version { get; set; }

    public Dictionary<string, object?> ConfigContent { get; set; } = new();

    public string? ChangeDescription { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
