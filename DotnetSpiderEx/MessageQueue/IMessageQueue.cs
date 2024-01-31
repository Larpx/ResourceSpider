namespace Larpx.ResourceSpider.DotnetSpiderEx.MessageQueue
{
    public interface IMessageQueue : IDisposable
    {
        Task PublishAsync(string queue, byte[] message);

        Task ConsumeAsync(AsyncMessageConsumer<byte[]> consumer, CancellationToken cancellationToken);

        void CloseQueue(string queue);

        bool IsDistributed { get; }
    }
}
