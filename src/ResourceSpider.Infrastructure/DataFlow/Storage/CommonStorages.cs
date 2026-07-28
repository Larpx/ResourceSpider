using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Larpx.PersonalTools.ResourceSpider.Core.DataFlow;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.DataFlow.Storage;

/// <summary>
/// 控制台存储，将数据流上下文中的数据输出到控制台
/// </summary>
public class ConsoleStorage : DataFlowBase
{
    /// <summary>
    /// 初始化控制台存储
    /// </summary>
    public override Task InitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// 将上下文数据逐项输出到控制台，然后调用下一个处理器
    /// </summary>
    /// <param name="context">数据流上下文</param>
    /// <param name="next">下一个处理器的委托</param>
    public override async Task HandleAsync(DataFlowContext context, ResponseDelegate next)
    {
        if (!IsNullOrEmpty(context))
        {
            foreach (var kv in context.Data)
            {
                Console.WriteLine($"[{kv.Key}]: {kv.Value}");
            }
        }
        await next(context);
    }
}

/// <summary>
/// JSON 文件存储，将数据流上下文中的数据序列化为 JSON 格式并保存到文件
/// </summary>
public class JsonFileStorage : DataFlowBase
{
    private readonly string _folder;

    /// <summary>
    /// 通过输出目录初始化 JSON 文件存储
    /// </summary>
    /// <param name="folder">输出目录路径</param>
    public JsonFileStorage(string folder) { _folder = folder; }

    /// <summary>
    /// 初始化存储，确保输出目录存在
    /// </summary>
    public override Task InitializeAsync()
    {
        if (!string.IsNullOrEmpty(_folder) && !Directory.Exists(_folder)) Directory.CreateDirectory(_folder);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 将上下文数据序列化为 JSON 并写入文件，然后调用下一个处理器
    /// </summary>
    /// <param name="context">数据流上下文</param>
    /// <param name="next">下一个处理器的委托</param>
    public override async Task HandleAsync(DataFlowContext context, ResponseDelegate next)
    {
        if (!IsNullOrEmpty(context))
        {
            var fileName = $"{context.Request?.Url?.GetHashCode():X8}_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.json";
            var filePath = Path.Combine(_folder ?? ".", fileName);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(context.Data, Newtonsoft.Json.Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);
        }
        await next(context);
    }
}
