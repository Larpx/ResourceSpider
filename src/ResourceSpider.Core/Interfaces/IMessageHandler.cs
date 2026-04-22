namespace ResourceSpider.Core.Interfaces;

/// <summary>
/// 消息处理器接口，定义消息的通用处理契约
/// </summary>
/// <typeparam name="T">消息类型</typeparam>
public interface IMessageHandler<T>
{
    /// <summary>
    /// 异步处理指定消息
    /// </summary>
    /// <param name="message">待处理的消息</param>
    /// <param name="ct">取消令牌</param>
    Task HandleAsync(T message, CancellationToken ct = default);
}
