using Microsoft.AspNetCore.SignalR;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Hubs;

public class SpiderHub : Hub
{
    private readonly IAgentRegisterService _agentRegisterService;
    private readonly ILogger<SpiderHub> _logger;

    public SpiderHub(IAgentRegisterService agentRegisterService, ILogger<SpiderHub> logger)
    {
        _agentRegisterService = agentRegisterService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var agentId = Context.UserIdentifier ?? Context.ConnectionId;
        _logger.LogInformation("Agent {AgentId} 已连接 SignalR，ConnectionId: {ConnectionId}", agentId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var agentId = Context.UserIdentifier ?? Context.ConnectionId;
        _logger.LogInformation("Agent {AgentId} 断开 SignalR 连接", agentId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinAgentGroup(string agentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"agent-{agentId}");
        _logger.LogInformation("Agent {AgentId} 加入 SignalR 分组", agentId);
    }

    public async Task LeaveAgentGroup(string agentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"agent-{agentId}");
    }

    public async Task Ack(string messageId)
    {
        _logger.LogDebug("收到消息确认：{MessageId}", messageId);
    }
}

public static class SpiderHubMethods
{
    public static async Task SendTaskAssign(IHubContext<SpiderHub> hubContext, string agentId, object taskData)
    {
        await hubContext.Clients.Group($"agent-{agentId}").SendAsync("TaskAssign", taskData);
    }

    public static async Task SendConfigUpdate(IHubContext<SpiderHub> hubContext, string agentId, object configData)
    {
        await hubContext.Clients.Group($"agent-{agentId}").SendAsync("ConfigUpdate", configData);
    }

    public static async Task SendControlCommand(IHubContext<SpiderHub> hubContext, string agentId, string command, object? data = null)
    {
        await hubContext.Clients.Group($"agent-{agentId}").SendAsync("ControlCommand", new { command, data });
    }

    public static async Task BroadcastControlCommand(IHubContext<SpiderHub> hubContext, string command, object? data = null)
    {
        await hubContext.Clients.All.SendAsync("ControlCommand", new { command, data });
    }
}
