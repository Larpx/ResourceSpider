using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

public class Request
{
    public string RequestId { get; set; } = Guid.NewGuid().ToString("N");
    
    public string Url { get; set; } = string.Empty;
    
    public string Method { get; set; } = "GET";
    
    public Dictionary<string, string> Headers { get; set; } = new();
    
    public byte[]? Body { get; set; }
    
    public string? TaskId { get; set; }
    
    public int Priority { get; set; } = 5;
    
    public int RetryCount { get; set; }
    
    public int MaxRetry { get; set; } = 3;
    
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? Fingerprint { get; set; }
    
    public Dictionary<string, object?> Metadata { get; set; } = new();
}
