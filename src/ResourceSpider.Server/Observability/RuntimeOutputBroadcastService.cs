using Microsoft.AspNetCore.SignalR;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Hubs;

namespace ResourceSpider.Server.Observability;

/// <summary>
/// 将运行时日志输出实时广播到管理端 SignalR 分组。
/// </summary>
public class RuntimeOutputBroadcastService : BackgroundService
{
    private readonly IHubContext<SpiderHub> _hubContext;
    private readonly ILogger<RuntimeOutputBroadcastService> _logger;

    public RuntimeOutputBroadcastService(
        IHubContext<SpiderHub> hubContext,
        ILogger<RuntimeOutputBroadcastService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var entry in RuntimeOutputStream.ReadAllAsync(stoppingToken))
        {
            try
            {
                var payload = new RuntimeOutputLogDto(
                    entry.Sequence,
                    entry.TimestampUtc,
                    entry.Level,
                    entry.Source,
                    entry.Message);

                await _hubContext.Clients
                    .Group(SpiderHubMethods.AdminRuntimeGroup)
                    .SendAsync("RuntimeOutputLog", payload, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "推送运行时日志到 SignalR 失败");
            }
        }
    }
}
