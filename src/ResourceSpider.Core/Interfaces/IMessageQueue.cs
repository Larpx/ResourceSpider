namespace ResourceSpider.Core.Interfaces;

public interface IMessageQueue
{
    Task EnqueueAsync<T>(T message, CancellationToken ct = default);
    
    Task<T?> DequeueAsync<T>(CancellationToken ct = default);
    
    Task<bool> TryEnqueueAsync<T>(T message, CancellationToken ct = default);
}
