namespace ResourceSpider.Core.Models;

public class AgentGroup
{
    public string GroupId { get; set; } = Guid.NewGuid().ToString("N");

    public string GroupName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<string> AgentIds { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
