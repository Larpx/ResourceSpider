namespace ResourceSpider.Core.DataFlow;

public class SpiderOptions
{
    public int RequestedQueueCount { get; set; } = 1000;
    public int Depth { get; set; }
    public int RetriedTimes { get; set; } = 3;
    public int EmptySleepTime { get; set; } = 60;
    public double Speed { get; set; } = 1;
    public uint Batch { get; set; } = 4;
    public bool RemoveOutboundLinks { get; set; }
    public string StorageType { get; set; } = string.Empty;
    public int RefreshProxy { get; set; } = 30;
}
