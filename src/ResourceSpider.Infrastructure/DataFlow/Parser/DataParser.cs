using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ResourceSpider.Core.DataFlow;
using ResourceSpider.Core.Models;
using ResourceSpider.Core.Selector;
using ResourceSpider.Infrastructure.Selector;

namespace ResourceSpider.Infrastructure.DataFlow.Parser;

public abstract class DataParser : DataFlowBase
{
    private readonly List<Func<DataFlowContext, IEnumerable<Request>>> _followRequestQueriers = [];
    private readonly List<Func<Request, bool>> _requiredValidator = [];

    public Func<DataFlowContext, ISelectable>? SelectableBuilder { get; protected set; }

    protected abstract Task ParseAsync(DataFlowContext context);

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

    public virtual void AddRequiredValidator(Func<Request, bool> requiredValidator) => _requiredValidator.Add(requiredValidator);
    public virtual void AddRequiredValidator(string pattern) => _requiredValidator.Add(request => Regex.IsMatch(request.Url, pattern));

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

    public bool IsValidRequest(Request request) => _requiredValidator.Count <= 0 || _requiredValidator.Any(v => v(request));
}
