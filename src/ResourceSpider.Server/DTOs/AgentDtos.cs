using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

/// <summary>
/// 代理注册请求
/// </summary>
/// <param name="AgentId">代理唯一标识，最大长度 64</param>
/// <param name="AgentName">代理名称，最大长度 128</param>
/// <param name="IpAddress">代理 IP 地址，最大长度 45</param>
/// <param name="Port">代理通信端口</param>
/// <param name="Capabilities">代理能力描述，可选</param>
/// <param name="OS">操作系统信息，可选</param>
/// <param name="Version">代理版本号，可选</param>
public record RegisterAgentRequest(
    [Required, StringLength(64)] string AgentId,
    [Required, StringLength(128)] string AgentName,
    [Required, StringLength(45)] string IpAddress,
    int Port,
    Dictionary<string, object?>? Capabilities = null,
    [StringLength(100)] string? OS = null,
    [StringLength(50)] string? Version = null
);

/// <summary>
/// 代理注册响应，返回认证令牌和服务器配置
/// </summary>
/// <param name="AgentToken">代理认证令牌，用于后续请求验证</param>
/// <param name="HeartbeatInterval">心跳间隔（秒）</param>
/// <param name="ServerVersion">服务器版本号</param>
public record RegisterAgentResponse(
    string AgentToken,
    int HeartbeatInterval,
    string ServerVersion
);

/// <summary>
/// 代理心跳请求，代理定期发送以维持在线状态
/// </summary>
/// <param name="AgentId">代理 ID</param>
/// <param name="AgentToken">代理认证令牌</param>
/// <param name="CpuUsage">CPU 使用率百分比</param>
/// <param name="MemoryUsage">内存使用率百分比</param>
/// <param name="TaskCount">当前执行的任务数量</param>
/// <param name="Status">代理状态</param>
/// <param name="OS">操作系统信息，可选</param>
/// <param name="Version">代理版本号，可选</param>
public record HeartbeatRequest(
    [Required] string AgentId,
    [Required] string AgentToken,
    decimal? CpuUsage,
    decimal? MemoryUsage,
    int TaskCount,
    int Status,
    [StringLength(100)] string? OS = null,
    [StringLength(50)] string? Version = null
);

/// <summary>
/// 代理心跳响应，返回服务端确认和待分配任务
/// </summary>
/// <param name="Ack">心跳确认标识，true 表示令牌有效</param>
/// <param name="NewTasks">新分配的任务列表，可选</param>
/// <param name="ConfigUpdate">配置更新数据，可选</param>
public record HeartbeatResponse(
    bool Ack,
    List<TaskDto>? NewTasks,
    Dictionary<string, object>? ConfigUpdate
);

/// <summary>
/// 代理注销请求
/// </summary>
/// <param name="AgentId">代理 ID</param>
/// <param name="AgentToken">代理认证令牌</param>
/// <param name="Reason">注销原因，可选</param>
public record UnregisterAgentRequest(
    [Required] string AgentId,
    [Required] string AgentToken,
    string? Reason
);

/// <summary>
/// 代理信息数据传输对象
/// </summary>
/// <param name="AgentId">代理 ID</param>
/// <param name="AgentName">代理名称</param>
/// <param name="IpAddress">IP 地址</param>
/// <param name="Port">通信端口</param>
/// <param name="Status">代理状态</param>
/// <param name="CpuUsage">CPU 使用率</param>
/// <param name="MemoryUsage">内存使用率</param>
/// <param name="TaskCount">当前任务数量</param>
/// <param name="LastHeartbeat">最后心跳时间</param>
/// <param name="Tags">标签列表</param>
/// <param name="GroupId">所属分组 ID</param>
/// <param name="OS">操作系统信息</param>
/// <param name="Version">代理版本号</param>
/// <param name="CreatedAt">注册时间</param>
public record AgentDto(
    string AgentId,
    string AgentName,
    string IpAddress,
    int Port,
    int Status,
    decimal? CpuUsage,
    decimal? MemoryUsage,
    int TaskCount,
    DateTime? LastHeartbeat,
    List<string>? Tags,
    string? GroupId,
    string? OS,
    string? Version,
    DateTime CreatedAt
);

/// <summary>
/// 更新代理请求
/// </summary>
/// <param name="AgentName">代理名称，可选</param>
/// <param name="Tags">标签列表，可选</param>
/// <param name="GroupId">所属分组 ID，可选</param>
/// <param name="Config">代理配置，可选</param>
public record UpdateAgentRequest(
    string? AgentName,
    List<string>? Tags,
    string? GroupId,
    Dictionary<string, object?>? Config
);
