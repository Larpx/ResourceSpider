using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ResourceSpider.Core.Interfaces;
using System.Text;
using System.Text.Json;

namespace ResourceSpider.Infrastructure.MessageQueue;

/// <summary>
/// RabbitMQ 消息队列配置选项
/// </summary>
public class RabbitMqOptions
{
    /// <summary>
    /// RabbitMQ 服务器主机地址
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// RabbitMQ 服务器端口号
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// 认证用户名
    /// </summary>
    public string Username { get; set; } = "guest";

    /// <summary>
    /// 认证密码
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// 队列名称
    /// </summary>
    public string QueueName { get; set; } = "resource_spider_queue";

    /// <summary>
    /// 连接超时时间（毫秒），默认 10 秒
    /// </summary>
    public int ConnectionTimeoutMs { get; set; } = 10000;

    /// <summary>
    /// 握手超时时间（毫秒），默认 10 秒
    /// </summary>
    public int HandshakeTimeoutMs { get; set; } = 10000;
}

/// <summary>
/// 基于 RabbitMQ 的分布式消息队列实现，支持跨进程消息传递
/// 适用于分布式部署场景，消息持久化在 RabbitMQ 中
/// 采用延迟初始化模式，避免构造函数中同步阻塞异步方法
/// </summary>
public class RabbitMqMessageQueue : IMessageQueue, IDisposable, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqMessageQueue> _logger;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _disposed;
    private bool _initialized;

    /// <summary>
    /// 初始化 RabbitMQ 消息队列
    /// </summary>
    /// <param name="options">RabbitMQ 配置选项</param>
    /// <param name="logger">日志记录器</param>
    public RabbitMqMessageQueue(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqMessageQueue> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// 异步初始化 RabbitMQ 连接和通道，确保队列已声明
    /// 使用双重检查锁定避免重复初始化
    /// </summary>
    /// <param name="ct">取消令牌</param>
    private async Task EnsureInitializedAsync(CancellationToken ct = default)
    {
        if (_initialized) return;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_initialized) return;

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                RequestedConnectionTimeout = TimeSpan.FromMilliseconds(_options.ConnectionTimeoutMs),
                HandshakeContinuationTimeout = TimeSpan.FromMilliseconds(_options.HandshakeTimeoutMs)
            };

            _connection = await factory.CreateConnectionAsync(ct);
            _channel = await _connection.CreateChannelAsync(cancellationToken: ct);
            await _channel.QueueDeclareAsync(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: ct);

            _initialized = true;
            _logger.LogInformation("RabbitMQ 连接初始化完成: {Host}:{Port}/{Queue}", _options.Host, _options.Port, _options.QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ 连接初始化失败: {Host}:{Port}", _options.Host, _options.Port);
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// 将消息发布到 RabbitMQ 队列
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="message">要发布的消息</param>
    /// <param name="ct">取消令牌</param>
    public async Task EnqueueAsync<T>(T message, CancellationToken ct = default)
    {
        if (message == null) return;

        await EnsureInitializedAsync(ct);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await _channel!.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _options.QueueName,
            mandatory: false,
            basicProperties: properties,
            body: new ReadOnlyMemory<byte>(body),
            cancellationToken: ct);
    }

    /// <summary>
    /// 不支持出队操作，RabbitMQ 使用发布/订阅模型
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="ct">取消令牌</param>
    /// <exception cref="NotSupportedException">始终抛出</exception>
    public Task<T?> DequeueAsync<T>(CancellationToken ct = default)
    {
        throw new NotSupportedException(
            "RabbitMQ uses publish/subscribe model. Use IMessageConsumer instead.");
    }

    /// <summary>
    /// 尝试将消息发布到 RabbitMQ 队列
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="message">要发布的消息</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>发布成功返回 true，失败返回 false</returns>
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

    /// <summary>
    /// 同步释放资源，关闭通道和连接
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _channel?.Dispose();
        _connection?.Dispose();
        _initLock.Dispose();
    }

    /// <summary>
    /// 异步释放资源，关闭通道和连接
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if (_channel != null)
            await _channel.DisposeAsync();
        if (_connection != null)
            await _connection.DisposeAsync();
        _initLock.Dispose();
    }
}
