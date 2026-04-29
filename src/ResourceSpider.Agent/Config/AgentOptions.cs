using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Agent.Config;

public class AgentOptions
{
    public string Mode { get; set; } = "Local";

    public LocalModeOptions LocalConfig { get; set; } = new();

    public OnlineModeOptions ServerConfig { get; set; } = new();

    public int MaxConcurrentTasks { get; set; } = 5;

    public int HeartbeatIntervalSeconds { get; set; } = 30;

    public List<string>? Tags { get; set; }

    public LocalModeOptions? LocalMode => LocalConfig;

    public OnlineModeOptions? OnlineMode => ServerConfig;
}
