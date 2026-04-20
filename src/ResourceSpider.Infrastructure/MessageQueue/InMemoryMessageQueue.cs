using System.Threading.Channels;
using ResourceSpider.Core.Interfaces;

namespace ResourceSpider.Infrastructure.MessageQueue;

public class InMemoryMessageQueue : IMessageQueue, IDisposable
{
    private readonly Channel<object> _channel;
    private readonly int _capacity;
    private bool _disposed;

    public InMemoryMessageQueue(int? capacity = null)
    {
        _capacity = capacity ?? 10000;
        var options = new BoundedChannelOptions(_capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<object>(options);
    }

    public async Task EnqueueAsync<T>(T message, CancellationToken ct = default)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        await _channel.Writer.WriteAsync(message!, ct);
    }

    public async Task<T?> DequeueAsync<T>(CancellationToken ct = default)
    {
        while (await _channel.Reader.WaitToReadAsync(ct))
        {
            if (_channel.Reader.TryRead(out var item) && item is T result)
            {
                return result;
            }
        }
        return default;
    }

    public async Task<bool> TryEnqueueAsync<T>(T message, CancellationToken ct = default)
    {
        if (message == null) return false;
        try
        {
            await _channel.Writer.WriteAsync(message!, ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _channel.Writer.Complete();
        _disposed = true;
    }
}
