using DotnetSpider;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Downloader;
using DotnetSpider.Http;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;
using DotnetSpider.Selector;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace MainSpider
{
    class Program
    {
        public static async void Main(string[] args)
        {
            //http://www.ichemistry.cn/weixianpin/


            ThreadPool.SetMaxThreads(255, 255);
            ThreadPool.SetMinThreads(255, 255);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console().WriteTo.File("logs/spider.log")
                .CreateLogger();

            await ChemistrySpider.RunAsync();


            Console.WriteLine("Hello World!");
        }
    }

    public class ChemistrySpider : Spider
    {
        /// <summary>
        /// 启动方法
        /// </summary>
        /// <returns></returns>
        public static async Task RunAsync()
        {
            var builder = Builder.CreateDefaultBuilder<ChemistrySpider>();
            builder.UseSerilog();
            builder.IgnoreServerCertificateError();
            builder.UseDownloader<HttpClientDownloader>();
            builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();

            //builder.UseMySqlQueueBfsScheduler(x =>
            //{
            //    x.ConnectionString = builder.Configuration["SchedulerConnectionString"];
            //});
            await builder.Build().RunAsync();
        }

        /// <summary>
        /// 初始化构造函数
        /// </summary>
        /// <param name="options"></param>
        /// <param name="services"></param>
        /// <param name="logger"></param>
        public ChemistrySpider(IOptions<SpiderOptions> options, DependenceServices services, ILogger<Spider> logger) :
            base(options, services, logger)
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task InitializeAsync(CancellationToken stoppingToken = default)
        {
            // 添加自定义解析
            AddDataFlow(new DataParser<ChemicalDemo>());
            // 使用控制台存储器
            AddDataFlow(new ConsoleStorage());

            //添加采集链接
            await AddRequestsAsync(new Request("http://www.ichemistry.cn/weixianpin/")
            {
                Cookie = "",
                // 请求超时 10 秒
                Timeout = 10000
            });
        }

        /// <summary>
        /// 自定义解析类
        /// </summary>

        [Schema("iChemical", "weixianpin")]
        //[EntitySelector(Expression = "#BodyBox > table.padding5.chem_img > tbody > tr", Type = SelectorType.Css)]
        [EntitySelector(Expression = "#BodyBox > table.padding5.chem_img > tbody > tr", Type = SelectorType.Css)]
        [FollowRequestSelector(Expressions = new[] { "#BodyBox > table:nth-child(4) > tbody > tr > td > p > a:nth-child(1)" }, SelectorType = SelectorType.Css)]
        public class ChemicalDemo : EntityBase<ChemicalDemo>
        {
            protected override void Configure()
            {
                HasIndex(x => new { x.Guid }, true);
            }

            [StringLength(40)]
            [ValueSelector(Expression = "GUID", Type = SelectorType.Environment)]
            public string Guid { get; set; }

            [Required]
            [StringLength(256)]
            [ValueSelector(Expression = "/tr/td[1]", Type = SelectorType.XPath)]
            public string ID { get; set; }

            [Required]
            [StringLength(1024)]
            [ValueSelector(Expression = "/tr/td[4]", Type = SelectorType.XPath)]
            public string Name { get; set; }

            [Required]
            [StringLength(128)]
            [ValueSelector(Expression = "/tr/td[6]", Type = SelectorType.XPath)]
            public string Cas { get; set; }

        }
    }
}
