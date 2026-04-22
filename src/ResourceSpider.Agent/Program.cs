using Microsoft.Extensions.Options;
using ResourceSpider.Agent.Config;
using ResourceSpider.Agent.Modes;
using ResourceSpider.Agent.Services;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ResourceSpider.Infrastructure.Downloader;
using ResourceSpider.Infrastructure.Duplicate;
using ResourceSpider.Infrastructure.MessageQueue;
using ResourceSpider.Infrastructure.Parser;
using ResourceSpider.Infrastructure.Proxy;
using ResourceSpider.Infrastructure.Scheduler;
using ResourceSpider.Infrastructure.Storage;
using Serilog;

namespace ResourceSpider.Agent;

/// <summary>
/// Agent 程序入口类，负责配置日志、依赖注入和服务启动
/// </summary>
public class Program
{
    /// <summary>
    /// 应用程序入口方法，初始化日志系统并启动主机
    /// </summary>
    /// <param name="args">命令行参数</param>
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/agent-.txt", rollingInterval: RollingInterval.Day)
            .MinimumLevel.Information()
            .CreateLogger();

        try
        {
            Log.Information("Starting ResourceSpider Agent");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Agent terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// 创建主机构建器，配置依赖注入容器
    /// 根据 Agent 运行模式（Local/Online）注册不同的服务集合
    /// </summary>
    /// <param name="args">命令行参数</param>
    /// <returns>配置完成的 IHostBuilder 实例</returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices((hostContext, services) =>
            {
                var config = hostContext.Configuration;
                var agentOptions = config.GetSection("Agent").Get<AgentOptions>() ?? new AgentOptions();

                services.Configure<AgentOptions>(config.GetSection("Agent"));

                services.AddSingleton<IMessageQueue, InMemoryMessageQueue>();
                services.AddSingleton<IDuplicateRemover, HashSetDuplicateRemover>();
                services.AddSingleton<IScheduler, BreadthFirstScheduler>();
                services.AddSingleton<IProxyPool, ProxyPool>();
                services.AddSingleton<IParserFactory, DefaultParserFactory>();

                services.AddTransient<HttpClientDownloader>();
                services.AddTransient<PlaywrightDownloader>();
                services.AddHttpClient<HttpClientDownloader>();
                services.AddSingleton<IDownloaderFactory, DefaultDownloaderFactory>();
                services.AddSingleton<IDownloader>(sp =>
                    sp.GetRequiredService<IDownloaderFactory>().CreateDownloader(DownloadType.HttpClient));

                services.AddSingleton<ITaskExecutor, TaskExecutor>();

                var mode = agentOptions.Mode;

                if (mode.Equals("Local", StringComparison.OrdinalIgnoreCase))
                {
                    services.AddSingleton(agentOptions.LocalConfig);
                    services.AddSingleton<IResultReporter, ResultReporter>();
                    services.AddSingleton<IStorage>(sp =>
                    {
                        var opts = new FileStorageOptions
                        {
                            OutputPath = agentOptions.LocalConfig.ResultOutputPath,
                            Format = agentOptions.LocalConfig.OutputFormat,
                            AgentId = $"agent-local-{Environment.MachineName}",
                            AgentName = $"Local Agent ({Environment.MachineName})",
                            HostName = Environment.MachineName,
                            Mode = "Local"
                        };
                        return ActivatorUtilities.CreateInstance<FileStorage>(sp, Options.Create(opts));
                    });
                    services.AddSingleton<IHostedService, LocalModeRunner>();
                }
                else
                {
                    services.AddSingleton(agentOptions.ServerConfig);
                    services.AddHttpClient<IServerApiClient, ServerApiClient>()
                        .ConfigureHttpClient(c =>
                        {
                            c.BaseAddress = new Uri(agentOptions.ServerConfig.ServerUrl);
                        });
                    services.AddSingleton<ISignalRClient, SignalRClient>();
                    services.AddSingleton<IOfflineTaskStore, OfflineTaskStore>();
                    services.AddSingleton<IAgentEncryptionService, AgentEncryptionService>();
                    services.AddSingleton<IResultReporter, ResultReporter>();
                    services.AddSingleton<IStorage>(sp =>
                    {
                        var opts = new FileStorageOptions
                        {
                            OutputPath = "./results",
                            Format = "csv",
                            AgentId = agentOptions.ServerConfig.AgentId,
                            AgentName = agentOptions.ServerConfig.AgentName,
                            HostName = Environment.MachineName,
                            Mode = "Online"
                        };
                        return ActivatorUtilities.CreateInstance<FileStorage>(sp, Options.Create(opts));
                    });
                    services.AddSingleton<IHostedService, OnlineModeRunner>();
                }
            });
}
