using System;
using System.Collections.Generic;
using System.IO;
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

/// <summary>
/// 爬虫构建器，继承自 HostBuilder，提供爬虫应用的默认配置和依赖注入设置
/// 封装了主机配置、应用配置、服务注册等通用逻辑，简化爬虫应用的创建过程
/// </summary>
public class Builder : HostBuilder
{
    /// <summary>
    /// 私有构造函数，强制通过工厂方法创建实例
    /// </summary>
    private Builder() { }

    /// <summary>
    /// 使用默认配置创建爬虫构建器，支持命令行参数
    /// 默认配置包括：环境变量、JSON 配置文件、内存消息队列、哈希去重、广度优先调度等
    /// </summary>
    /// <typeparam name="T">爬虫类型，必须继承自 Spider 基类</typeparam>
    /// <param name="args">命令行参数</param>
    /// <param name="configureDelegate">爬虫选项配置委托，可选</param>
    /// <returns>配置完成的爬虫构建器实例</returns>
    public static Builder CreateDefaultBuilder<T>(Action<SpiderOptions>? configureDelegate = null) where T : Spider
        => CreateDefaultBuilder<T>([], configureDelegate);

    /// <summary>
    /// 使用默认配置创建爬虫构建器（带命令行参数）
    /// </summary>
    /// <typeparam name="T">爬虫类型，必须继承自 Spider 基类</typeparam>
    /// <param name="args">命令行参数数组</param>
    /// <param name="configureDelegate">爬虫选项配置委托，可选</param>
    /// <returns>配置完成的爬虫构建器实例</returns>
    public static Builder CreateDefaultBuilder<T>(string[] args, Action<SpiderOptions>? configureDelegate = null) where T : Spider
        => CreateDefaultBuilder(typeof(T), args, configureDelegate);

    /// <summary>
    /// 使用默认配置创建爬虫构建器（通过类型参数指定爬虫类）
    /// </summary>
    /// <param name="type">爬虫类型，必须继承自 Spider 基类</param>
    /// <param name="args">命令行参数，可选</param>
    /// <param name="configure">爬虫选项配置委托，可选</param>
    /// <returns>配置完成的爬虫构建器实例</returns>
    /// <exception cref="ArgumentException">当传入的类型不是 Spider 子类时抛出</exception>
    public static Builder CreateDefaultBuilder(Type type, string[]? args = null, Action<SpiderOptions>? configure = null)
    {
        var builder = new Builder();
        ConfigureBuilder(builder, type, args, configure);
        return builder;
    }

    /// <summary>
    /// 创建爬虫构建器（不带默认配置），支持自定义配置
    /// </summary>
    /// <typeparam name="T">爬虫类型，必须继承自 Spider 基类</typeparam>
    /// <param name="configureDelegate">爬虫选项配置委托，可选</param>
    /// <returns>构建器实例</returns>
    public static Builder CreateBuilder<T>(Action<SpiderOptions>? configureDelegate = null) where T : Spider
        => CreateBuilder<T>(null, configureDelegate);

    /// <summary>
    /// 创建爬虫构建器（不带默认配置），支持命令行参数和自定义配置
    /// </summary>
    /// <typeparam name="T">爬虫类型，必须继承自 Spider 基类</typeparam>
    /// <param name="args">命令行参数，可选</param>
    /// <param name="configureDelegate">爬虫选项配置委托，可选</param>
    /// <returns>构建器实例</returns>
    public static Builder CreateBuilder<T>(string[]? args, Action<SpiderOptions>? configureDelegate = null) where T : Spider
        => CreateBuilder(typeof(T), args, configureDelegate);

    /// <summary>
    /// 创建爬虫构建器（不带默认配置），通过类型参数指定爬虫类
    /// </summary>
    /// <param name="type">爬虫类型，必须继承自 Spider 基类</param>
    /// <param name="args">命令行参数，可选</param>
    /// <param name="configureDelegate">爬虫选项配置委托，可选</param>
    /// <returns>构建器实例</returns>
    /// <exception cref="ArgumentException">当传入的类型不是 Spider 子类时抛出</exception>
    public static Builder CreateBuilder(Type type, string[]? args = null, Action<SpiderOptions>? configureDelegate = null)
    {
        var builder = new Builder();
        ConfigureBuilder(builder, type, args, configureDelegate);
        return builder;
    }

    /// <summary>
    /// 统一的构建器配置方法，设置主机配置、应用配置和服务注册
    /// 包括：内容根目录、环境变量、JSON 配置文件、命令行参数、
    /// HttpClient、消息队列、去重器、调度器、下载器等核心服务
    /// </summary>
    /// <param name="builder">构建器实例</param>
    /// <param name="type">爬虫类型</param>
    /// <param name="args">命令行参数，可选</param>
    /// <param name="configureDelegate">爬虫选项配置委托，可选</param>
    /// <exception cref="ArgumentException">当传入的类型不是 Spider 子类时抛出</exception>
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
