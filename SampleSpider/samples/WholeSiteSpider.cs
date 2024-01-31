using System.Threading;
using System.Threading.Tasks;
using Larpx.ResourceSpider.DotnetSpiderEx;
using Larpx.ResourceSpider.DotnetSpiderEx.DataFlow;
using Larpx.ResourceSpider.DotnetSpiderEx.DataFlow.Parser;
using Larpx.ResourceSpider.DotnetSpiderEx.DataFlow.Storage;
using Larpx.ResourceSpider.DotnetSpiderEx.Downloader;
using Larpx.ResourceSpider.DotnetSpiderEx.Infrastructure;
using Larpx.ResourceSpider.DotnetSpiderEx.Scheduler;
using Larpx.ResourceSpider.DotnetSpiderEx.Scheduler.Component;
using Larpx.ResourceSpider.DotnetSpiderEx.Selector;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Larpx.ResourceSpider.SapmleSpider.samples
{
    public class WholeSiteSpider : Spider
    {
        public static async Task RunAsync()
        {
            var builder = Builder.CreateDefaultBuilder<WholeSiteSpider>(options =>
            {
                options.Depth = 1000;
            });
            builder.UseDownloader<HttpClientDownloader>();
            builder.UseSerilog();
            builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
            await builder.Build().RunAsync();
        }

        public WholeSiteSpider(IOptions<SpiderOptions> options,
            DependenceServices services,
            ILogger<Spider> logger) : base(
            options, services, logger)
        {
        }

        protected override async Task InitializeAsync(CancellationToken stoppingToken)
        {
            AddDataFlow(new MyDataParser());
            AddDataFlow(new ConsoleStorage()); // 控制台打印采集结果
            await AddRequestsAsync("http://www.cnblogs.com/"); // 设置起始链接
        }

        protected override SpiderId GenerateSpiderId()
        {
            return new(ObjectId.CreateId().ToString(), "博客园全站采集");
        }

        class MyDataParser : DataParser
        {
            public override Task InitializeAsync()
            {
                AddRequiredValidator("cnblogs\\.com");
                AddFollowRequestQuerier(Selectors.XPath("."));
                return Task.CompletedTask;
            }

            protected override Task ParseAsync(DataFlowContext context)
            {
                context.AddData("URL", context.Request.RequestUri);
                context.AddData("Title", context.Selectable.XPath(".//title")?.Value);
                return Task.CompletedTask;
            }
        }
    }
}
