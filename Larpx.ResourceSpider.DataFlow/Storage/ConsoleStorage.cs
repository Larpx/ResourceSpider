using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Larpx.ResourceSpider.DataFlow.Storage
{
    /// <summary>
    /// 控制台打印解析结果(所有解析结果)
    /// </summary>
    public class ConsoleStorage : StorageBase
    {
        protected override Task StoreAsync(DataContext context)
        {
            var items = context.GetData();

            Console.WriteLine(JsonConvert.SerializeObject(items));
            return Task.CompletedTask;
        }
    }
}
