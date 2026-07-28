using System.Threading.Channels;
using Larpx.PersonalTools.ResourceSpider.Core.Interfaces;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.MessageQueue;

/// <summary>
/// 基于内存通道的消息队列实现，使用 System.Threading.Channels 实现高效的生产者-消费者模式
/// 适用于单机场景，数据不持久化，进程重启后队列清空
/// </summary>
public class InMemoryMessageQueue : IMessageQueue, IDisposable
{
    private readonly Channel<object> _channel;
    private readonly int _capacity;
    private bool _disposed;

    /// <summary>
    /// 初始化内存消息队列
    /// </summary>
    /// <param name="capacity">队列容量，默认 10000</param>
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

    /// <summary>
    /// 将消息加入队列
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="message">要入队的消息</param>
    /// <param name="ct">取消令牌</param>
    /// <exception cref="ArgumentNullException">消息为 null 时抛出</exception>
    public async Task EnqueueAsync<T>(T message, CancellationToken ct = default)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        await _channel.Writer.WriteAsync(message!, ct);
    }

    /// <summary>
    /// 从队列中取出消息
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="ct">取消令牌</param>
    /// <returns>出队的消息，队列为空时返回 default</returns>
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

    /// <summary>
    /// 尝试将消息加入队列，失败时返回 false
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="message">要入队的消息</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>入队成功返回 true，失败返回 false</returns>
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

    /// <summary>
    /// 释放资源，关闭通道
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _channel.Writer.Complete();
        _disposed = true;
    }
}
