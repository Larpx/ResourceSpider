using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Larpx.ResourceSpider.DataFlow.Storage
{
    /// <summary>
    /// 控制台打印(实体)解析结果
    /// </summary>
    public class ConsoleEntityStorage : EntityStorageBase
    {
        protected override Task StoreAsync(DataContext context, Dictionary<Type, List<dynamic>> dict)
        {
            foreach (var item in dict)
            {
                Console.WriteLine(JsonConvert.SerializeObject(item.Value));
            }

            return Task.CompletedTask;
        }
    }
}
