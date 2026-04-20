namespace ResourceSpider.Agent.Config;

public class AgentOptions
{
    public string Mode { get; set; } = "Local";
    public LocalModeOptions LocalConfig { get; set; } = new();
    public OnlineModeOptions ServerConfig { get; set; } = new();
}
