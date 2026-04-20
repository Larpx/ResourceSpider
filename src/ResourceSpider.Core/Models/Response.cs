using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

public class Response
{
    public string RequestId { get; set; } = string.Empty;
    
    public string Url { get; set; } = string.Empty;
    
    public int StatusCode { get; set; }
    
    public Dictionary<string, string> Headers { get; set; } = new();
    
    public byte[] Content { get; set; } = Array.Empty<byte>();
    
    public string ContentType { get; set; } = string.Empty;
    
    public long ContentLength { get; set; }
    
    public int Duration { get; set; }
    
    public RequestStatus Status { get; set; }
    
    public string? Error { get; set; }
    
    public ErrorType? ErrorType { get; set; }
    
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    
    public string? TextContent => Content.Length > 0 
        ? System.Text.Encoding.UTF8.GetString(Content) 
        : null;
}
