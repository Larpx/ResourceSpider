using Larpx.PersonalTools.ResourceSpider.Core.Models;
using Larpx.PersonalTools.ResourceSpider.Core.Selector;

namespace Larpx.PersonalTools.ResourceSpider.Core.DataFlow;

/// <summary>
/// 数据流上下文，在数据流管道中传递请求数据、响应数据和中间处理结果
/// </summary>
public class DataFlowContext : IDisposable
{
    /// <summary>
    /// 上下文附加项，用于在数据流各阶段之间传递临时数据
    /// </summary>
    public readonly IDictionary<object, object> Items = new Dictionary<object, object>();

    /// <summary>
    /// 提取的数据存储，用于保存解析后的结构化数据
    /// </summary>
    public readonly IDictionary<object, object> Data = new Dictionary<object, object>();

    /// <summary>
    /// 当前可选择的文档对象，用于执行选择器操作
    /// </summary>
    public ISelectable? Selectable { get; set; }

    /// <summary>
    /// 爬虫配置选项
    /// </summary>
    public SpiderOptions Options { get; }

    /// <summary>
    /// 当前请求对应的响应数据
    /// </summary>
    public Response Response { get; }

    /// <summary>
    /// 当前正在处理的请求
    /// </summary>
    public Request Request { get; }

    /// <summary>
    /// 后续需要跟踪的请求列表，用于实现多页面爬取
    /// </summary>
    public List<Request> FollowRequests { get; }

    /// <summary>
    /// 服务提供者，用于获取依赖注入的服务实例
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// 初始化数据流上下文
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    /// <param name="options">爬虫配置选项</param>
    /// <param name="request">当前请求</param>
    /// <param name="response">响应数据</param>
    public DataFlowContext(IServiceProvider serviceProvider, SpiderOptions options, Request request, Response response)
    {
        Request = request;
        Response = response;
        Options = options;
        ServiceProvider = serviceProvider;
        FollowRequests = [];
    }

    /// <summary>
    /// 添加后续跟踪请求
    /// </summary>
    /// <param name="requests">要添加的请求数组</param>
    public void AddFollowRequests(params Request[] requests) => AddFollowRequests(requests.AsEnumerable());

    /// <summary>
    /// 添加后续跟踪请求
    /// </summary>
    /// <param name="requests">要添加的请求集合</param>
    public void AddFollowRequests(IEnumerable<Request> requests) { if (requests != null) FollowRequests.AddRange(requests); }

    /// <summary>
    /// 向数据存储中添加一条数据
    /// </summary>
    /// <param name="name">数据键名</param>
    /// <param name="data">数据值</param>
    public void AddData(object name, dynamic data) => Data[name] = data;

    /// <summary>
    /// 从数据存储中获取指定键的数据
    /// </summary>
    /// <param name="name">数据键名</param>
    /// <returns>对应的数据值，不存在时返回 null</returns>
    public dynamic? GetData(object name) => Data.TryGetValue(name, out var value) ? value : null;

    /// <summary>
    /// 获取当前数据存储是否为空
    /// </summary>
    public bool IsEmpty => Data.Count == 0;

    /// <summary>
    /// 释放资源，清空所有存储数据
    /// </summary>
    public void Dispose()
    {
        Items.Clear();
        Data.Clear();
    }
}
