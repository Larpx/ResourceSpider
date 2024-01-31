namespace Larpx.ResourceSpider.DotnetSpiderEx.MessageQueue
{
    public delegate Task AsyncMessageHandler<in TMessage>(TMessage message);
}