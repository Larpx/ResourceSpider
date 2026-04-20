using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.Interfaces;

public class DataContext
{
    public Response? Response { get; set; }
    
    public List<DataRecord> DataRecords { get; set; } = new();
    
    public Dictionary<string, object?> Items { get; set; } = new();
    
    public string? TaskId { get; set; }
    
    public string? RequestId { get; set; }
}
