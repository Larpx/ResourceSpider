using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ResourceSpider.Core.Interfaces;

namespace ResourceSpider.Infrastructure.MessageQueue;

public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string QueueName { get; set; } = "resource_spider_queue";
}

public class RabbitMqMessageQueue : IMessageQueue, IDisposable
{
    private readonly IBus _bus;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqMessageQueue> _logger;
    private bool _disposed;

    public RabbitMqMessageQueue(
        IBus bus, 
        IOptions<RabbitMqOptions> options, 
        ILogger<RabbitMqMessageQueue> logger)
    {
        _bus = bus;
        _options = options.Value;
        _logger = logger;
    }

    public async Task EnqueueAsync<T>(T message, CancellationToken ct = default)
    {
        if (message != null)
        {
            await _bus.Publish(message, ct);
        }
    }

    public Task<T?> DequeueAsync<T>(CancellationToken ct = default)
    {
        throw new NotSupportedException(
            "RabbitMQ uses publish/subscribe model. Use IMessageConsumer instead.");
    }

    public async Task<bool> TryEnqueueAsync<T>(T message, CancellationToken ct = default)
    {
        try
        {
            await EnqueueAsync(message, ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue message");
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
