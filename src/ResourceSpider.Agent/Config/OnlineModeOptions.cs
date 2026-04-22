namespace ResourceSpider.Agent.Config;

/// <summary>
/// 在线模式配置选项，定义 Agent 与服务端通信所需的连接参数
/// </summary>
public class OnlineModeOptions
{
    /// <summary>
    /// 服务端地址，默认为 http://localhost:5000
    /// </summary>
    public string ServerUrl { get; set; } = "http://localhost:5000";

    /// <summary>
    /// Agent 唯一标识，用于在服务端注册和识别
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Agent 显示名称
    /// </summary>
    public string AgentName { get; set; } = string.Empty;

    /// <summary>
    /// Agent 认证令牌，由服务端在注册时分配
    /// </summary>
    public string AgentToken { get; set; } = string.Empty;

    /// <summary>
    /// 心跳发送间隔（秒），默认为 30 秒
    /// </summary>
    public int HeartbeatInterval { get; set; } = 30;

    public string? EncryptionKey { get; set; }

    public int MaxConcurrentTasks { get; set; } = 5;

    public int OfflineSyncIntervalMinutes { get; set; } = 5;
}
