using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

public record RegisterAgentRequest(
    [Required, StringLength(64)] string AgentId,
    [Required, StringLength(128)] string AgentName,
    [Required, StringLength(45)] string IpAddress,
    int Port,
    Dictionary<string, object?>? Capabilities = null,
    [StringLength(100)] string? OS = null,
    [StringLength(50)] string? Version = null
);

public record RegisterAgentResponse(
    string AgentToken,
    int HeartbeatInterval,
    string ServerVersion
);

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

public record HeartbeatResponse(
    bool Ack,
    List<TaskDto>? NewTasks,
    Dictionary<string, object>? ConfigUpdate
);

public record UnregisterAgentRequest(
    [Required] string AgentId,
    [Required] string AgentToken,
    string? Reason
);

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

public record UpdateAgentRequest(
    string? AgentName,
    List<string>? Tags,
    string? GroupId,
    Dictionary<string, object?>? Config
);
