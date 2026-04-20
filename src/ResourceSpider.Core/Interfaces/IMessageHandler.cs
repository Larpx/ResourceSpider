namespace ResourceSpider.Core.Interfaces;

public interface IMessageHandler<T>
{
    Task HandleAsync(T message, CancellationToken ct = default);
}
