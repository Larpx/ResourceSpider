using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

public class Agent
{
    public string AgentId { get; set; } = Guid.NewGuid().ToString("N");

    public string AgentName { get; set; } = string.Empty;

    public string? AgentToken { get; set; }

    public string IpAddress { get; set; } = string.Empty;

    public int Port { get; set; }

    public Dictionary<string, object?>? Capabilities { get; set; }

    public AgentStatus Status { get; set; } = AgentStatus.Offline;

    public decimal? CpuUsage { get; set; }

    public decimal? MemoryUsage { get; set; }

    public int TaskCount { get; set; }

    public DateTime? LastHeartbeat { get; set; }

    public List<string>? Tags { get; set; }

    public string? GroupId { get; set; }

    public string? OS { get; set; }

    public string? Version { get; set; }

    public Dictionary<string, object?>? Config { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
