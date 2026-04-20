using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

public class Agent
{
    public string AgentId { get; set; } = Guid.NewGuid().ToString("N");
    
    public string AgentName { get; set; } = string.Empty;
    
    public string? AgentToken { get; set; }
    
    public string IpAddress { get; set; } = string.Empty;
    
    public int Port { get; set; }
    
    public List<string> Capabilities { get; set; } = new();
    
    public AgentStatus Status { get; set; } = AgentStatus.Offline;
    
    public decimal? CpuUsage { get; set; }
    
    public decimal? MemoryUsage { get; set; }
    
    public int TaskCount { get; set; }
    
    public DateTime? LastHeartbeat { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
