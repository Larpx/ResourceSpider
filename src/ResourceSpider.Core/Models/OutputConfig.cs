namespace ResourceSpider.Core.Models;

public class OutputConfig
{
    public List<string> OutputFields { get; set; } = new();

    public List<string> DedupFields { get; set; } = new();
}
