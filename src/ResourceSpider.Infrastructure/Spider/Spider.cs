using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ResourceSpider.Core.DataFlow;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ResourceSpider.Infrastructure.DataFlow;

namespace ResourceSpider.Infrastructure.Spider;

/// <summary>
/// 爬虫抽象基类，继承自 BackgroundService，提供完整的爬虫执行生命周期管理
/// 包括：初始化、请求调度、下载、数据流处理、速率控制、重试机制等核心功能
/// </summary>
public abstract class Spider : BackgroundService
{
    /// <summary>
    /// 数据流构建器，用于组装数据处理管道
    /// </summary>
    private readonly FlowBuilder _flowBuilder = new();

    /// <summary>
    /// 已请求队列，跟踪正在处理中的请求，防止重复处理和检测超时
    /// </summary>
    private readonly RequestedQueue _requestedQueue = new();

    /// <summary>
    /// 依赖服务集合，包含调度器、下载器工厂等核心依赖
    /// </summary>
    private readonly DependenceServices _services;

    /// <summary>
    /// 数据流处理委托链，由 FlowBuilder 构建完成后生成
    /// </summary>
    private ResponseDelegate _delegate = null!;

    /// <summary>
    /// 获取爬虫配置选项
    /// </summary>
    protected SpiderOptions Options { get; }

    /// <summary>
    /// 获取日志记录器实例
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// 获取爬虫唯一标识符，在执行时自动生成
    /// </summary>
    protected string SpiderId { get; private set; } = string.Empty;

    /// <summary>
    /// 初始化爬虫实例，注入配置选项、依赖服务和日志记录器
    /// </summary>
    /// <param name="options">爬虫配置选项</param>
    /// <param name="services">依赖服务集合</param>
    /// <param name="logger">日志记录器</param>
    protected Spider(IOptions<SpiderOptions> options, DependenceServices services, ILogger<Spider> logger)
    {
        Logger = logger;
        Options = options.Value;
        _services = services;
    }

    /// <summary>
    /// 爬虫初始化方法，子类必须实现以完成自定义初始化逻辑
    /// 通常在此方法中添加数据流处理器和初始请求
    /// </summary>
    /// <param name="stoppingToken">取消令牌</param>
    /// <returns>异步任务</returns>
    protected abstract Task InitializeAsync(CancellationToken stoppingToken = default);

    /// <summary>
    /// 生成爬虫唯一标识符，默认使用 GUID，子类可重写以自定义标识生成逻辑
    /// </summary>
    /// <returns>爬虫标识字符串</returns>
    protected virtual string GenerateSpiderId() => Guid.NewGuid().ToString("N");

    /// <summary>
    /// 添加数据流处理器到管道中（泛型方式）
    /// </summary>
    /// <typeparam name="T">数据流处理器类型，必须实现 IDataFlow 接口</typeparam>
    /// <returns>当前爬虫实例，支持链式调用</returns>
    protected virtual Spider AddDataFlow<T>() where T : Core.DataFlow.IDataFlow { _flowBuilder.AddFlow<T>(); return this; }

    /// <summary>
    /// 添加数据流处理器到管道中（工厂方式）
    /// </summary>
    /// <param name="factory">数据流处理器工厂委托</param>
    /// <returns>当前爬虫实例，支持链式调用</returns>
    protected virtual Spider AddDataFlow(Func<Core.DataFlow.IDataFlow> factory) { _flowBuilder.AddFlow(factory); return this; }

    /// <summary>
    /// 批量添加请求 URL 到调度器中
    /// </summary>
    /// <param name="urls">请求 URL 数组</param>
    /// <returns>成功入队的请求数量</returns>
    protected async Task<int> AddRequestsAsync(params string[] urls)
    {
        if (urls == null || urls.Length == 0) return 0;
        return await AddRequestsAsync(urls.Select(x => new Request { Url = x }));
    }

    /// <summary>
    /// 批量添加请求到调度器中，会过滤超过重试次数和深度限制的请求
    /// </summary>
    /// <param name="requests">请求集合</param>
    /// <returns>成功入队的请求数量</returns>
    protected async Task<int> AddRequestsAsync(IEnumerable<Request> requests)
    {
        if (requests == null) return 0;
        var list = new List<Request>();
        foreach (var request in requests)
        {
            if (request.RetryCount > Options.RetriedTimes) continue;
            if (Options.Depth > 0 && request.Metadata.TryGetValue("Depth", out var depth) && depth is int d && d > Options.Depth) continue;
            list.Add(request);
        }
        return await _services.Scheduler.EnqueueAsync(list);
    }

    /// <summary>
    /// 爬虫执行入口，由 BackgroundService 框架调用
    /// 执行流程：生成标识 → 初始化调度器 → 调用用户初始化 → 构建数据流管道 → 运行爬取循环
    /// </summary>
    /// <param name="stoppingToken">取消令牌</param>
    /// <returns>异步任务</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SpiderId = GenerateSpiderId();
        Logger.LogInformation("Initialize spider {SpiderId}", SpiderId);
        await _services.Scheduler.InitializeAsync(SpiderId);
        await InitializeAsync(stoppingToken);
        var tuple = await _flowBuilder.BuildAsync();
        _delegate = tuple.Item2;
        Logger.LogInformation("Spider {SpiderId}, {DataFlow}", SpiderId, tuple.Item1);
        await RunAsync(stoppingToken);
    }

    /// <summary>
    /// 爬取主循环，持续从调度器获取请求并下载处理
    /// 使用令牌桶算法控制请求速率，支持空闲休眠和超时退出
    /// </summary>
    /// <param name="stoppingToken">取消令牌</param>
    /// <returns>异步任务</returns>
    private async Task RunAsync(CancellationToken stoppingToken)
    {
        var bucket = CreateBucket(Options.Speed);
        var sleepTime = 0;
        var sleepTimeLimit = Options.EmptySleepTime * 1000;

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_requestedQueue.Count > Options.RequestedQueueCount)
            {
                sleepTime += 10;
                if (sleepTime > sleepTimeLimit) break;
                await Task.Delay(10, stoppingToken);
                continue;
            }

            var requests = (await _services.Scheduler.DequeueAsync((int)Options.Batch)).ToArray();
            if (requests.Length > 0)
            {
                sleepTime = 0;
                foreach (var request in requests)
                {
                    using var lease = await bucket.AcquireAsync(1);
                    if (!lease.IsAcquired) continue;

                    var downloader = _services.DownloaderFactory.CreateDownloader(Core.Interfaces.DownloadType.HttpClient);
                    var response = await downloader.DownloadAsync(request, stoppingToken);

                    if (response.Status == Core.Enums.RequestStatus.Success)
                    {
                        await HandleResponseAsync(request, response);
                    }
                    else
                    {
                        request.RetryCount++;
                        if (request.RetryCount <= Options.RetriedTimes) await AddRequestsAsync(new[] { request });
                    }
                }
            }
            else
            {
                sleepTime += 10;
                if (sleepTime > sleepTimeLimit) break;
                await Task.Delay(10, stoppingToken);
            }
        }
    }

    /// <summary>
    /// 处理下载成功的响应，创建数据流上下文并通过管道处理
    /// 处理完成后将后续请求添加到调度器
    /// </summary>
    /// <param name="request">原始请求</param>
    /// <param name="response">下载响应</param>
    /// <returns>异步任务</returns>
    private async Task HandleResponseAsync(Request request, Response response)
    {
        DataFlowContext? context = null;
        try
        {
            using var scope = _services.ServiceProvider.CreateScope();
            context = new DataFlowContext(scope.ServiceProvider, Options, request, response);
            await _delegate.Invoke(context);
            await AddRequestsAsync(context.FollowRequests);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Spider {SpiderId} handle {Url} failed", SpiderId, request.Url);
            request.RetryCount++;
            if (request.RetryCount <= Options.RetriedTimes) await AddRequestsAsync(new[] { request });
        }
        finally
        {
            context?.Dispose();
        }
    }

    /// <summary>
    /// 根据速率配置创建令牌桶限流器
    /// 速率大于等于 1 时，按毫秒间隔补充令牌；速率小于 1 时，按秒间隔补充令牌
    /// </summary>
    /// <param name="speed">每秒请求速率</param>
    /// <returns>令牌桶限流器实例</returns>
    private static TokenBucketRateLimiter CreateBucket(double speed)
    {
        var intervalMs = speed >= 1 ? (int)(1000 / speed) : (int)((1 / speed) * 1000);
        return new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 1,
            TokensPerPeriod = 1,
            ReplenishmentPeriod = TimeSpan.FromMilliseconds(intervalMs),
            AutoReplenishment = true,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = int.MaxValue
        });
    }
}

/// <summary>
/// 依赖服务集合，聚合爬虫运行所需的核心依赖
/// 包含服务提供者、调度器和下载器工厂
/// </summary>
public class DependenceServices : IDisposable
{
    /// <summary>
    /// 服务提供者，用于创建作用域和获取依赖服务
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// 请求调度器，管理请求的入队和出队
    /// </summary>
    public IScheduler Scheduler { get; }

    /// <summary>
    /// 下载器工厂，根据下载类型创建对应的下载器实例
    /// </summary>
    public IDownloaderFactory DownloaderFactory { get; }

    /// <summary>
    /// 初始化依赖服务集合
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    /// <param name="scheduler">请求调度器</param>
    /// <param name="downloaderFactory">下载器工厂</param>
    public DependenceServices(IServiceProvider serviceProvider, IScheduler scheduler, IDownloaderFactory downloaderFactory)
    {
        ServiceProvider = serviceProvider;
        Scheduler = scheduler;
        DownloaderFactory = downloaderFactory;
    }

    /// <summary>
    /// 释放资源，如果调度器实现了 IDisposable 则释放
    /// </summary>
    public void Dispose() { (Scheduler as IDisposable)?.Dispose(); }
}

/// <summary>
/// 已请求队列，跟踪正在处理中的请求
/// 用于防止重复处理同一请求，并检测超时请求
/// </summary>
public class RequestedQueue
{
    /// <summary>
    /// 请求存储字典，键为请求指纹或请求 ID
    /// </summary>
    private readonly Dictionary<string, Request> _requests = new();

    /// <summary>
    /// 线程同步锁
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// 获取当前队列中的请求数量
    /// </summary>
    public int Count { get { lock (_lock) { return _requests.Count; } } }

    /// <summary>
    /// 将请求加入队列，如果已存在则返回 false
    /// </summary>
    /// <param name="request">要加入的请求</param>
    /// <returns>成功加入返回 true，请求已存在返回 false</returns>
    public bool Enqueue(Request request)
    {
        lock (_lock)
        {
            var key = request.Fingerprint ?? request.RequestId;
            if (_requests.ContainsKey(key)) return false;
            _requests[key] = request;
            return true;
        }
    }

    /// <summary>
    /// 根据指纹从队列中移除并返回请求
    /// </summary>
    /// <param name="fingerprint">请求指纹</param>
    /// <returns>匹配的请求，未找到返回 null</returns>
    public Request? Dequeue(string fingerprint)
    {
        lock (_lock)
        {
            if (fingerprint != null && _requests.TryGetValue(fingerprint, out var request))
            {
                _requests.Remove(fingerprint);
                return request;
            }
            return null;
        }
    }

    /// <summary>
    /// 获取所有超时请求（创建时间超过 30 秒），并从队列中移除
    /// </summary>
    /// <returns>超时请求数组</returns>
    public Request[] GetAllTimeoutList()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var timeout = _requests.Values.Where(r => (now - r.CreatedAt).TotalSeconds > 30).ToArray();
            foreach (var r in timeout) _requests.Remove(r.Fingerprint ?? r.RequestId);
            return timeout;
        }
    }
}
