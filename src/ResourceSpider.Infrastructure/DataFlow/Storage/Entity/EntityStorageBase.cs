using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Larpx.PersonalTools.ResourceSpider.Core.DataFlow;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.DataFlow.Storage.Entity;

/// <summary>
/// 实体存储基类，从数据流上下文中提取 IEntity 类型的实体数据
/// 并调用子类实现的具体存储逻辑
/// </summary>
public abstract class EntityStorageBase : DataFlowBase
{
    private readonly Type _baseType = typeof(IEntity);

    /// <summary>
    /// 子类实现的具体实体存储逻辑
    /// </summary>
    /// <param name="context">数据流上下文</param>
    /// <param name="entities">按类型分组的实体数据字典</param>
    protected abstract Task HandleAsync(DataFlowContext context, IDictionary<Type, IList<dynamic>> entities);

    /// <summary>
    /// 处理数据流上下文，提取 IEntity 实体并调用子类存储逻辑
    /// </summary>
    /// <param name="context">数据流上下文</param>
    /// <param name="next">下一个处理器的委托</param>
    public override async Task HandleAsync(DataFlowContext context, ResponseDelegate next)
    {
        if (IsNullOrEmpty(context)) Logger.LogWarning("数据流上下文不包含实体解析结果");
        else
        {
            var data = context.Data;
            var result = new Dictionary<Type, IList<dynamic>>();
            foreach (var kv in data)
            {
                if (kv.Key is not Type type || !_baseType.IsAssignableFrom(type)) continue;
                if (kv.Value is IEnumerable list) { foreach (var obj in list) AddResult(result, type, obj); }
                else AddResult(result, type, kv.Value);
            }
            await HandleAsync(context, result);
        }
        await next(context);
    }

    /// <summary>
    /// 将实体添加到结果字典中
    /// </summary>
    /// <param name="dict">结果字典</param>
    /// <param name="type">实体类型</param>
    /// <param name="obj">实体实例</param>
    private void AddResult(IDictionary<Type, IList<dynamic>> dict, Type type, dynamic obj)
    {
        if (!_baseType.IsInstanceOfType(obj)) return;
        if (!dict.ContainsKey(type)) dict.Add(type, new List<dynamic>());
        dict[type].Add(obj);
    }
}
