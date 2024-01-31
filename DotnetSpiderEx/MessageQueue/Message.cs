using Larpx.ResourceSpider.DotnetSpiderEx.Infrastructure;

namespace Larpx.ResourceSpider.DotnetSpiderEx.MessageQueue
{
    public abstract class Message
    {
        public long Timestamp { get; set; }
        public string MessageId { get; set; }

        protected Message()
        {
            MessageId = ObjectId.CreateId().ToString();
        }
    }
}
