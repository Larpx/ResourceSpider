namespace ResourceSpider.Core.Interfaces;

/// <summary>
/// 消息队列接口，提供异步消息的入队和出队操作
/// </summary>
public interface IMessageQueue
{
    /// <summary>
    /// 将消息加入队列
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="message">要入队的消息</param>
    /// <param name="ct">取消令牌</param>
    Task EnqueueAsync<T>(T message, CancellationToken ct = default);

    /// <summary>
    /// 从队列中取出消息
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="ct">取消令牌</param>
    /// <returns>出队的消息，队列为空时返回 default</returns>
    Task<T?> DequeueAsync<T>(CancellationToken ct = default);

    /// <summary>
    /// 尝试将消息加入队列，队列满时返回 false
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="message">要入队的消息</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>入队成功返回 true，队列满返回 false</returns>
    Task<bool> TryEnqueueAsync<T>(T message, CancellationToken ct = default);
}
