namespace ResourceSpider.Agent.Config;

public class OnlineModeOptions
{
    public string ServerUrl { get; set; } = "http://localhost:5000";
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string AgentToken { get; set; } = string.Empty;
    public int HeartbeatInterval { get; set; } = 30;
}
