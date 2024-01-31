using Larpx.ResourceSpider.DotnetSpiderEx.DataFlow.Storage;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Larpx.ResourceSpider.DotnetSpiderEx.DataFlow
{
    /// <summary>
    /// 控制台打印(实体)解析结果
    /// </summary>
    public class ConsoleEntityStorage : EntityStorageBase
    {
        public static IDataFlow CreateFromOptions(IConfiguration configuration)
        {
            return new ConsoleEntityStorage();
        }

        public override Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        protected override Task HandleAsync(DataFlowContext context, IDictionary<Type, ICollection<dynamic>> entities)
        {
            foreach (var kv in entities)
            {
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(kv.Value));
            }

            return Task.CompletedTask;
        }
    }
}
