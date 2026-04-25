using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using ResourceSpider.Server.Observability;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Hubs;

/// <summary>
/// 爬虫 SignalR Hub，提供代理节点与服务端之间的实时通信功能
/// 支持代理注册、分组管理和消息确认
/// </summary>
public class SpiderHub : Hub
{
    /// <summary>
    /// 代理注册服务实例
    /// </summary>
    private readonly IAgentRegisterService _agentRegisterService;

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<SpiderHub> _logger;

    /// <summary>
    /// 运行时监控相关配置
    /// </summary>
    private readonly RuntimeMonitoringOptions _runtimeOptions;

    /// <summary>
    /// 初始化 SpiderHub
    /// </summary>
    /// <param name="agentRegisterService">代理注册服务</param>
    /// <param name="runtimeOptions">运行时监控配置</param>
    /// <param name="logger">日志记录器</param>
    public SpiderHub(
        IAgentRegisterService agentRegisterService,
        IOptions<RuntimeMonitoringOptions> runtimeOptions,
        ILogger<SpiderHub> logger)
    {
        _agentRegisterService = agentRegisterService;
        _runtimeOptions = runtimeOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// 处理客户端连接事件，记录代理连接信息
    /// </summary>
    /// <returns>异步任务</returns>
    public override async Task OnConnectedAsync()
    {
        var agentId = Context.UserIdentifier ?? Context.ConnectionId;
        _logger.LogInformation("Agent {AgentId} 已连接 SignalR，ConnectionId: {ConnectionId}", agentId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// 处理客户端断开连接事件，记录代理断开信息
    /// </summary>
    /// <param name="exception">断开连接的异常信息，正常断开时为 null</param>
    /// <returns>异步任务</returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var agentId = Context.UserIdentifier ?? Context.ConnectionId;
        _logger.LogInformation("Agent {AgentId} 断开 SignalR 连接", agentId);

        SpiderHubMethods.AdminRuntimeSnapshotIntervals.TryRemove(Context.ConnectionId, out _);

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// 将代理加入指定的 SignalR 分组，用于定向推送消息
    /// </summary>
    /// <param name="agentId">代理 ID</param>
    /// <returns>异步任务</returns>
    public async Task JoinAgentGroup(string agentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"agent-{agentId}");
        _logger.LogInformation("Agent {AgentId} 加入 SignalR 分组", agentId);
    }

    /// <summary>
    /// 将代理从指定的 SignalR 分组中移除
    /// </summary>
    /// <param name="agentId">代理 ID</param>
    /// <returns>异步任务</returns>
    public async Task LeaveAgentGroup(string agentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"agent-{agentId}");
    }

    /// <summary>
    /// 处理代理发送的消息确认
    /// </summary>
    /// <param name="messageId">消息 ID</param>
    /// <returns>异步任务</returns>
    public async Task Ack(string messageId)
    {
        _logger.LogDebug("收到消息确认：{MessageId}", messageId);
    }

    /// <summary>
    /// 管理端加入运行监控日志分组。
    /// </summary>
    public async Task JoinAdminRuntimeGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, SpiderHubMethods.AdminRuntimeGroup);

        var defaultInterval = NormalizeSnapshotInterval(_runtimeOptions.DefaultSnapshotPushIntervalSeconds);
        SpiderHubMethods.AdminRuntimeSnapshotIntervals[Context.ConnectionId] = defaultInterval;

        _logger.LogDebug("连接 {ConnectionId} 加入管理运行监控分组，默认推送策略 {Interval}s", Context.ConnectionId, defaultInterval);
    }

    /// <summary>
    /// 管理端离开运行监控日志分组。
    /// </summary>
    public async Task LeaveAdminRuntimeGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SpiderHubMethods.AdminRuntimeGroup);
        SpiderHubMethods.AdminRuntimeSnapshotIntervals.TryRemove(Context.ConnectionId, out _);
    }

    /// <summary>
    /// 设置当前连接的快照推送策略。
    /// </summary>
    /// <param name="intervalSeconds">推送间隔（秒）</param>
    /// <returns>最终生效的推送间隔（秒）</returns>
    public Task<int> SetAdminRuntimeSnapshotInterval(int intervalSeconds)
    {
        var normalized = NormalizeSnapshotInterval(intervalSeconds);
        SpiderHubMethods.AdminRuntimeSnapshotIntervals[Context.ConnectionId] = normalized;
        return Task.FromResult(normalized);
    }

    private int NormalizeSnapshotInterval(int intervalSeconds)
    {
        var allowed = _runtimeOptions.SnapshotPushIntervalsSeconds;
        if (allowed.Count == 0)
        {
            return 1;
        }

        return allowed.Contains(intervalSeconds)
            ? intervalSeconds
            : _runtimeOptions.DefaultSnapshotPushIntervalSeconds;
    }
}

/// <summary>
/// SpiderHub 静态扩展方法，提供向代理推送消息的便捷方法
/// 包括任务分配、配置更新和控制命令等
/// </summary>
public static class SpiderHubMethods
{
    /// <summary>
    /// 管理端运行时日志分组名。
    /// </summary>
    public const string AdminRuntimeGroup = "admin-runtime";

    /// <summary>
    /// 管理端连接的快照推送策略（按连接保存，单位秒）。
    /// </summary>
    public static ConcurrentDictionary<string, int> AdminRuntimeSnapshotIntervals { get; } = new();

    /// <summary>
    /// 向指定代理推送任务分配消息
    /// </summary>
    /// <param name="hubContext">SignalR Hub 上下文</param>
    /// <param name="agentId">目标代理 ID</param>
    /// <param name="taskData">任务数据</param>
    /// <returns>异步任务</returns>
    public static async Task SendTaskAssign(IHubContext<SpiderHub> hubContext, string agentId, object taskData)
    {
        await hubContext.Clients.Group($"agent-{agentId}").SendAsync("TaskAssign", taskData);
    }

    /// <summary>
    /// 向指定代理推送配置更新消息
    /// </summary>
    /// <param name="hubContext">SignalR Hub 上下文</param>
    /// <param name="agentId">目标代理 ID</param>
    /// <param name="configData">配置数据</param>
    /// <returns>异步任务</returns>
    public static async Task SendConfigUpdate(IHubContext<SpiderHub> hubContext, string agentId, object configData)
    {
        await hubContext.Clients.Group($"agent-{agentId}").SendAsync("ConfigUpdate", configData);
    }

    /// <summary>
    /// 向指定代理推送控制命令
    /// </summary>
    /// <param name="hubContext">SignalR Hub 上下文</param>
    /// <param name="agentId">目标代理 ID</param>
    /// <param name="command">控制命令名称</param>
    /// <param name="data">命令附带数据，可选</param>
    /// <returns>异步任务</returns>
    public static async Task SendControlCommand(IHubContext<SpiderHub> hubContext, string agentId, string command, object? data = null)
    {
        await hubContext.Clients.Group($"agent-{agentId}").SendAsync("ControlCommand", new { command, data });
    }

    /// <summary>
    /// 向所有代理广播控制命令
    /// </summary>
    /// <param name="hubContext">SignalR Hub 上下文</param>
    /// <param name="command">控制命令名称</param>
    /// <param name="data">命令附带数据，可选</param>
    /// <returns>异步任务</returns>
    public static async Task BroadcastControlCommand(IHubContext<SpiderHub> hubContext, string command, object? data = null)
    {
        await hubContext.Clients.All.SendAsync("ControlCommand", new { command, data });
    }
}
