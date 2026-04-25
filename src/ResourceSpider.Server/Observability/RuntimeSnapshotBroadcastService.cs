using Microsoft.AspNetCore.SignalR;
using ResourceSpider.Server.Hubs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Observability;

/// <summary>
/// 将系统运行时快照实时广播到管理端 SignalR 分组。
/// </summary>
public class RuntimeSnapshotBroadcastService : BackgroundService
{
    private readonly IHubContext<SpiderHub> _hubContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RuntimeSnapshotBroadcastService> _logger;

    public RuntimeSnapshotBroadcastService(
        IHubContext<SpiderHub> hubContext,
        IServiceScopeFactory scopeFactory,
        ILogger<RuntimeSnapshotBroadcastService> logger)
    {
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        var tick = 0;

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                tick++;

                try
                {
                    var intervals = SpiderHubMethods.AdminRuntimeSnapshotIntervals.ToArray();
                    if (intervals.Length == 0)
                    {
                        continue;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var runtimeSnapshotService = scope.ServiceProvider.GetRequiredService<IRuntimeSnapshotService>();
                    var snapshot = await runtimeSnapshotService.GetSnapshotAsync();

                    foreach (var item in intervals)
                    {
                        var connectionId = item.Key;
                        var intervalSeconds = item.Value <= 0 ? 1 : item.Value;

                        if (tick % intervalSeconds != 0)
                        {
                            continue;
                        }

                        await _hubContext.Clients
                            .Client(connectionId)
                            .SendAsync("RuntimeSnapshot", snapshot, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "推送系统快照到 SignalR 失败");
                }
            }
        }
        finally
        {
            timer.Dispose();
        }
    }
}
