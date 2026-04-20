namespace ResourceSpider.Core.Models;

public class Proxy
{
    public string ProxyId { get; set; } = Guid.NewGuid().ToString("N");
    
    public string Host { get; set; } = string.Empty;
    
    public int Port { get; set; }
    
    public string Protocol { get; set; } = "HTTP";
    
    public string? Username { get; set; }
    
    public string? Password { get; set; }
    
    public bool IsAvailable { get; set; }
    
    public int SuccessCount { get; set; }
    
    public int FailureCount { get; set; }
    
    public double HealthScore { get; set; } = 1.0;
    
    public DateTime? LastCheckedAt { get; set; }
    
    public DateTime? NextCheckAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string Address => $"{Host}:{Port}";
}
