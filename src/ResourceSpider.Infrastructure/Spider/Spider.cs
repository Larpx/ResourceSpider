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

public abstract class Spider : BackgroundService
{
    private readonly FlowBuilder _flowBuilder = new();
    private readonly RequestedQueue _requestedQueue = new();
    private readonly DependenceServices _services;
    private ResponseDelegate _delegate = null!;

    protected SpiderOptions Options { get; }
    protected ILogger Logger { get; }
    protected string SpiderId { get; private set; } = string.Empty;

    protected Spider(IOptions<SpiderOptions> options, DependenceServices services, ILogger<Spider> logger)
    {
        Logger = logger;
        Options = options.Value;
        _services = services;
    }

    protected abstract Task InitializeAsync(CancellationToken stoppingToken = default);

    protected virtual string GenerateSpiderId() => Guid.NewGuid().ToString("N");

    protected virtual Spider AddDataFlow<T>() where T : Core.DataFlow.IDataFlow { _flowBuilder.AddFlow<T>(); return this; }
    protected virtual Spider AddDataFlow(Func<Core.DataFlow.IDataFlow> factory) { _flowBuilder.AddFlow(factory); return this; }

    protected async Task<int> AddRequestsAsync(params string[] urls)
    {
        if (urls == null || urls.Length == 0) return 0;
        return await AddRequestsAsync(urls.Select(x => new Request { Url = x }));
    }

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

public class DependenceServices : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    public IScheduler Scheduler { get; }
    public IDownloaderFactory DownloaderFactory { get; }

    public DependenceServices(IServiceProvider serviceProvider, IScheduler scheduler, IDownloaderFactory downloaderFactory)
    {
        ServiceProvider = serviceProvider;
        Scheduler = scheduler;
        DownloaderFactory = downloaderFactory;
    }

    public void Dispose() { (Scheduler as IDisposable)?.Dispose(); }
}

public class RequestedQueue
{
    private readonly Dictionary<string, Request> _requests = new();
    private readonly object _lock = new();

    public int Count { get { lock (_lock) { return _requests.Count; } } }

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
