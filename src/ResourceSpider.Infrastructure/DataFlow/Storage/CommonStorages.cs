using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ResourceSpider.Core.DataFlow;

namespace ResourceSpider.Infrastructure.DataFlow.Storage;

public class ConsoleStorage : DataFlowBase
{
    public override Task InitializeAsync() => Task.CompletedTask;

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

public class JsonFileStorage : DataFlowBase
{
    private readonly string _folder;

    public JsonFileStorage(string folder) { _folder = folder; }

    public override Task InitializeAsync()
    {
        if (!string.IsNullOrEmpty(_folder) && !Directory.Exists(_folder)) Directory.CreateDirectory(_folder);
        return Task.CompletedTask;
    }

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
