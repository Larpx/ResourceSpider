using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Larpx.PersonalTools.ResourceSpider.Core.DataFlow;
using Larpx.PersonalTools.ResourceSpider.Core.Models;
using Larpx.PersonalTools.ResourceSpider.Core.Selector;
using Larpx.PersonalTools.ResourceSpider.Infrastructure.Selector;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.DataFlow.Parser;

/// <summary>
/// 数据解析器抽象基类，提供页面内容解析和后续请求提取的通用逻辑
/// 自动根据响应内容类型（HTML/JSON/Text）创建对应的选择器
/// </summary>
public abstract class DataParser : DataFlowBase
{
    private readonly List<Func<DataFlowContext, IEnumerable<Request>>> _followRequestQueriers = [];
    private readonly List<Func<Request, bool>> _requiredValidator = [];

    /// <summary>
    /// 获取或设置可选择对象构建器，用于自定义上下文中的 Selectable 对象创建方式
    /// </summary>
    public Func<DataFlowContext, ISelectable>? SelectableBuilder { get; protected set; }

    /// <summary>
    /// 解析数据流上下文中的响应内容，子类必须实现具体的解析逻辑
    /// </summary>
    /// <param name="context">数据流上下文</param>
    protected abstract Task ParseAsync(DataFlowContext context);

    /// <summary>
    /// 添加后续请求提取器，通过选择器从当前页面提取后续要爬取的链接
    /// </summary>
    /// <param name="selector">选择器实例</param>
    public virtual void AddFollowRequestQuerier(ISelector selector)
    {
        _followRequestQueriers.Add(context =>
        {
            var selectable = context.Selectable?.SelectList(selector);
            if (selectable == null) return Enumerable.Empty<Request>();
            return selectable
                .SelectMany(x => x.Links())
                .Select(x => new Request { Url = x });
        });
    }

    /// <summary>
    /// 添加请求验证器，通过函数判断请求是否有效
    /// </summary>
    /// <param name="requiredValidator">请求验证函数</param>
    public virtual void AddRequiredValidator(Func<Request, bool> requiredValidator) => _requiredValidator.Add(requiredValidator);

    /// <summary>
    /// 添加请求验证器，通过正则表达式匹配 URL 判断请求是否有效
    /// </summary>
    /// <param name="pattern">URL 匹配的正则表达式</param>
    public virtual void AddRequiredValidator(string pattern) => _requiredValidator.Add(request => Regex.IsMatch(request.Url, pattern));

    /// <summary>
    /// 处理数据流上下文：自动创建 Selectable → 调用子类解析 → 提取后续请求
    /// </summary>
    /// <param name="context">数据流上下文</param>
    /// <param name="next">下一个处理器的委托</param>
    /// <exception cref="ArgumentNullException">上下文为 null 时抛出</exception>
    public override async Task HandleAsync(DataFlowContext context, ResponseDelegate next)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (!IsValidRequest(context.Request))
        {
            Logger.LogInformation("{ParserName} ignore parse request {Url}", GetType().Name, context.Request.Url);
        }
        else
        {
            if (context.Selectable == null)
            {
                if (SelectableBuilder != null)
                {
                    context.Selectable = SelectableBuilder(context);
                }
                else
                {
                    var text = context.Response.TextContent?.TrimStart();
                    if (text != null && (text.StartsWith("<!DOCTYPE html", StringComparison.InvariantCultureIgnoreCase) || text.StartsWith("<html", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        context.Selectable = new HtmlSelectable(text);
                    }
                    else
                    {
                        try { var token = (JObject?)JsonConvert.DeserializeObject(text ?? ""); context.Selectable = new JsonSelectable(token ?? JToken.Parse("{}")); }
                        catch { context.Selectable = new TextSelectable(text ?? ""); }
                    }
                }
            }

            await ParseAsync(context);

            var requests = new List<Request>();
            foreach (var followRequestQuerier in _followRequestQueriers)
            {
                var followRequests = followRequestQuerier(context);
                if (followRequests != null) requests.AddRange(followRequests);
            }

            foreach (var request in requests)
            {
                if (IsValidRequest(request)) context.AddFollowRequests(request);
            }
        }

        await next(context);
    }

    /// <summary>
    /// 判断请求是否通过所有验证器
    /// </summary>
    /// <param name="request">待验证的请求</param>
    /// <returns>通过所有验证器返回 true，否则返回 false</returns>
    public bool IsValidRequest(Request request) => _requiredValidator.Count <= 0 || _requiredValidator.Any(v => v(request));
}
