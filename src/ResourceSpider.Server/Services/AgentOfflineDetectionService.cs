using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Larpx.PersonalTools.ResourceSpider.Core.Enums;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;

namespace Larpx.PersonalTools.ResourceSpider.Server.Services;

/// <summary>
/// Agent 离线检测服务，继承自 BackgroundService，定期检查 Agent 的心跳状态
/// 当 Agent 超过指定时间未上报心跳时，将其标记为离线状态
/// </summary>
public class AgentOfflineDetectionService : BackgroundService
{
    /// <summary>
    /// 服务提供者，用于创建作用域获取仓储服务
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<AgentOfflineDetectionService> _logger;

    /// <summary>
    /// 离线超时时间（秒），超过此时间未上报心跳则视为离线
    /// </summary>
    private const int OfflineTimeoutSeconds = 90;

    /// <summary>
    /// 检查间隔时间（秒）
    /// </summary>
    private const int CheckIntervalSeconds = 30;

    /// <summary>
    /// 初始化 Agent 离线检测服务
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    /// <param name="logger">日志记录器</param>
    public AgentOfflineDetectionService(
        IServiceProvider serviceProvider,
        ILogger<AgentOfflineDetectionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// 服务执行入口，启动周期性离线检测任务
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Agent 离线检测服务启动，超时阈值: {Timeout}s", OfflineTimeoutSeconds);

        using var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(CheckIntervalSeconds));

        while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CheckOfflineAgentsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "离线检测出错");
            }
        }
    }

    /// <summary>
    /// 检查离线 Agent，更新状态并记录日志
    /// </summary>
    private async Task CheckOfflineAgentsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var agentRepository = scope.ServiceProvider.GetRequiredService<Repositories.AgentRepository>();
        var systemLogRepository = scope.ServiceProvider.GetRequiredService<Repositories.SystemLogRepository>();

        var agents = await agentRepository.GetAllAsync();
        var now = DateTime.UtcNow;

        foreach (var agent in agents)
        {
            var statusStr = ((AgentStatus)agent.Status).ToString();
            if (statusStr != AgentStatus.Online.ToString()) continue;

            if (!agent.LastHeartbeat.HasValue) continue;

            var elapsed = (now - agent.LastHeartbeat.Value).TotalSeconds;

            if (elapsed > OfflineTimeoutSeconds)
            {
                _logger.LogWarning("Agent {AgentId} 已离线，最后心跳: {LastHeartbeat}", agent.AgentId, agent.LastHeartbeat);

                agent.Status = (int)AgentStatus.Offline;
                agent.UpdatedAt = now;
                await agentRepository.UpdateAsync(agent);

                await systemLogRepository.AddAsync(new SystemLogEntity
                {
                    Level = "Warning",
                    Category = "Agent",
                    Message = $"Agent {agent.AgentName} ({agent.AgentId}) 已离线",
                    Detail = $"最后心跳时间: {agent.LastHeartbeat.Value:O}, 超时阈值: {OfflineTimeoutSeconds}s",
                    UserId = null,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }
}
