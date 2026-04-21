using ResourceSpider.Core.Models;
using ResourceSpider.Core.Selector;

namespace ResourceSpider.Core.DataFlow;

public class DataFlowContext : IDisposable
{
    public readonly IDictionary<object, object> Items = new Dictionary<object, object>();
    public readonly IDictionary<object, object> Data = new Dictionary<object, object>();

    public ISelectable? Selectable { get; set; }
    public SpiderOptions Options { get; }
    public Response Response { get; }
    public Request Request { get; }
    public List<Request> FollowRequests { get; }
    public IServiceProvider ServiceProvider { get; }

    public DataFlowContext(IServiceProvider serviceProvider, SpiderOptions options, Request request, Response response)
    {
        Request = request;
        Response = response;
        Options = options;
        ServiceProvider = serviceProvider;
        FollowRequests = [];
    }

    public void AddFollowRequests(params Request[] requests) => AddFollowRequests(requests.AsEnumerable());
    public void AddFollowRequests(IEnumerable<Request> requests) { if (requests != null) FollowRequests.AddRange(requests); }

    public void AddData(object name, dynamic data) => Data[name] = data;
    public dynamic? GetData(object name) => Data.TryGetValue(name, out var value) ? value : null;
    public bool IsEmpty => Data.Count == 0;

    public void Dispose()
    {
        Items.Clear();
        Data.Clear();
    }
}
