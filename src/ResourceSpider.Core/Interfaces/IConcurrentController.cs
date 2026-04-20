namespace ResourceSpider.Core.Interfaces;

public interface IConcurrentController
{
    Task StartAsync(CancellationToken ct = default);
    
    Task StopAsync(CancellationToken ct = default);
    
    int GetCurrentConcurrency();
    
    int MaxConcurrency { get; set; }
}
