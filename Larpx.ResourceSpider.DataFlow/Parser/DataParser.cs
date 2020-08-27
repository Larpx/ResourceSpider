using Larpx.ResourceSpider.BaseLibrary.Helpers;
using Larpx.ResourceSpider.DataFlow.Storage;
using Larpx.ResourceSpider.Http;
using Larpx.ResourceSpider.Selector;
using Larpx.ResourceSpider.Selector.Selectables;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Larpx.ResourceSpider.DataFlow.Parser
{
    /// <summary>
    /// 数据解析器
    /// </summary>
    public abstract class DataParser : AbstractDataFlow
    {
        private readonly List<Func<DataContext, IEnumerable<Request>>> _followRequestQueriers;
        private readonly List<Func<Request, bool>> _requiredValidator;

        /// <summary>
        /// 选择器的生成方法
        /// </summary>
        public Func<DataContext, ISelectable> SelectableBuilder { get; protected set; }

        /// <summary>
        /// 数据解析
        /// </summary>
        /// <param name="context">处理上下文</param>
        /// <returns></returns>
        protected abstract Task Parse(DataContext context);

        protected DataParser()
        {
            _followRequestQueriers = new List<Func<DataContext, IEnumerable<Request>>>();
            _requiredValidator = new List<Func<Request, bool>>();
        }

        /// <summary>
        /// 添加请求查询器到流中
        /// </summary>
        /// <param name="selector"></param>
        public void AddFollowRequestQuerier(ISelector selector)
        {
            _followRequestQueriers.Add(context =>
            {
                var oRequests = context.Selectable.SelectList(selector)
                .Where(x => x != null)
                .SelectMany(x => x.Links())
                .Select(x =>
                {
                    var oRequest = context.CreateNewRequest(x);
                    oRequest.RequestedTimes = 0;
                    return oRequest;
                });
                return oRequests;
            });

        }

        /// <summary>
        /// 添加请求验证器
        /// </summary>
        /// <param name="requiredValidator"></param>
        public void AddRequiredValidator(Func<Request, bool> requiredValidator)
        {
            _requiredValidator.Add(requiredValidator);
        }

        /// <summary>
        /// 添加请求验证器
        /// </summary>
        /// <param name="requiredValidator"></param>
        public void AddRequiredValidator(string pattern)
        {
            _requiredValidator.Add(request => Regex.IsMatch(request.RequestUri.ToString(), pattern));
        }

        /// <summary>
        /// 获取解析结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="results"></param>
        protected virtual void AddParsedResult<T>(DataContext context, IEnumerable<T> results)
            where T : EntityBase<T>, new()
        {
            if (results != null)
            {
                var type = typeof(T);
                var items = context.GetData(type);
                if (items == null)
                {
                    var list = new List<T>();
                    list.AddRange(results);
                    context.AddData(type, list);
                }
                else
                {
                    items.AddRange(results);
                }
            }
        }

        //internal void SetHtmlSelectableBuilder()
        //{
        //    ISelectable Builder(DataContext context)
        //    {
        //        var text = context.Response.ReadAsString().TrimStart();
        //        return GetHtmlSelectable(context, text);
        //    }

        //    SelectableBuilder = Builder;
        //}

        private ISelectable GetHtmlSelectable(DataContext context, string text)
        {
            var request = context.Request;
            var domain = request.RequestUri.Port == 80 || request.RequestUri.Port == 443
                ? $"{request.RequestUri.Scheme}://{request.RequestUri.Host}"
                : $"{request.RequestUri.Scheme}://{request.RequestUri.Host}:{request.RequestUri.Port}";
            return new HtmlSelectable(text, domain);
        }

        /// <summary>
        /// 数据解析
        /// </summary>
        /// <param name="context">处理上下文</param>
        /// <returns></returns>
        public override async Task HandleAsync(DataContext context)
        {
            //检查参数是否为空
            context.NotNull(nameof(context));
            context.Response.NotNull(nameof(context.Response));

            if (!IsValidRequest(context.Request))
            {
                Logger.LogInformation($"{GetType().Name} ignore request {context.Request.RequestUri}");
                return;
            }

            var request = context.Request;
            if (context.Selectable == null)
            {
                if (SelectableBuilder != null)
                {
                    context.Selectable = SelectableBuilder(context);
                }
                else
                {
                    var text = context.Response.ReadAsString().TrimStart();
                    if (text.StartsWith("<!DOCTYPE html>") || text.StartsWith("<html>"))
                    {
                        context.Selectable = GetHtmlSelectable(context, text);
                    }
                    else
                    {
                        try
                        {
                            var token = (JObject)JsonConvert.DeserializeObject(text);
                            context.Selectable = new JsonSelectable(token);
                        }
                        catch
                        {
                            context.Selectable = new TextSelectable(text);
                        }
                    }
                }
            }

            await Parse(context);

            var requests = new List<Request>();

            if (_followRequestQueriers != null)
            {
                foreach (var followRequestQuerier in _followRequestQueriers)
                {
                    var followRequests = followRequestQuerier(context);
                    if (followRequests != null)
                    {
                        requests.AddRange(followRequests);
                    }
                }
            }

            foreach (var followRequest in requests)
            {
                if (IsValidRequest(followRequest))
                {
                    // 在此强制设制 Owner, 防止用户忘记导致出错
                    followRequest.Owner = request.Owner;
                    followRequest.Agent = context.Response.Agent;
                    context.AddFollowRequests(followRequest);
                }
            }
        }

        private bool IsValidRequest(Request request)
        {
            return _requiredValidator.Count <= 0 || _requiredValidator.Any(validator => validator(request));
        }
    }
}
