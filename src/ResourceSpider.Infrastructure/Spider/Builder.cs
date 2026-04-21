using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ResourceSpider.Core.DataFlow;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Infrastructure.DataFlow;
using ResourceSpider.Infrastructure.Downloader;
using ResourceSpider.Infrastructure.Duplicate;
using ResourceSpider.Infrastructure.Scheduler;

namespace ResourceSpider.Infrastructure.Spider;

public class Builder : HostBuilder
{
    private Builder() { }

    public static Builder CreateDefaultBuilder<T>(Action<SpiderOptions>? configureDelegate = null) where T : Spider
        => CreateDefaultBuilder<T>([], configureDelegate);

    public static Builder CreateDefaultBuilder<T>(string[] args, Action<SpiderOptions>? configureDelegate = null) where T : Spider
        => CreateDefaultBuilder(typeof(T), args, configureDelegate);

    public static Builder CreateDefaultBuilder(Type type, string[]? args = null, Action<SpiderOptions>? configure = null)
    {
        var builder = new Builder();
        ConfigureBuilder(builder, type, args, configure);
        return builder;
    }

    public static Builder CreateBuilder<T>(Action<SpiderOptions>? configureDelegate = null) where T : Spider
        => CreateBuilder<T>(null, configureDelegate);

    public static Builder CreateBuilder<T>(string[]? args, Action<SpiderOptions>? configureDelegate = null) where T : Spider
        => CreateBuilder(typeof(T), args, configureDelegate);

    public static Builder CreateBuilder(Type type, string[]? args = null, Action<SpiderOptions>? configureDelegate = null)
    {
        var builder = new Builder();
        ConfigureBuilder(builder, type, args, configureDelegate);
        return builder;
    }

    private static void ConfigureBuilder(Builder builder, Type type, string[]? args = null, Action<SpiderOptions>? configureDelegate = null)
    {
        if (!type.IsAssignableTo(typeof(Spider))) throw new ArgumentException($"Type {type.FullName} is not a spider");

        builder.UseContentRoot(Directory.GetCurrentDirectory());
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddEnvironmentVariables("DOTNET_");
            if (args != null) config.AddCommandLine(args);
        });
        builder.ConfigureAppConfiguration((hostingContext, config) =>
        {
            var hostingEnvironment = hostingContext.HostingEnvironment;
            config.AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("appsettings." + hostingEnvironment.EnvironmentName + ".json", true, true);
            config.AddEnvironmentVariables();
            var list = new List<string> { "--DOTNET_SPIDER_MODEL", "LOCAL" };
            if (args != null) list.AddRange(args);
            config.AddCommandLine(list.ToArray());
        }).ConfigureServices((context, services) =>
        {
            var configuration = context.Configuration;
            services.Configure<SpiderOptions>(configuration);
            if (configureDelegate != null) services.Configure(configureDelegate);

            services.AddHttpClient();
            services.AddSingleton<IMessageQueue, MessageQueue.InMemoryMessageQueue>();
            services.AddSingleton<IDuplicateRemover, HashSetDuplicateRemover>();
            services.AddSingleton<IScheduler, BreadthFirstScheduler>();
            services.AddTransient<HttpClientDownloader>();
            services.AddSingleton<IDownloaderFactory, DefaultDownloaderFactory>();
            services.AddSingleton<DependenceServices>();
            services.AddSingleton(typeof(IHostedService), type);
        });
    }
}
