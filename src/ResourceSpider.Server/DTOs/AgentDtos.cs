using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

public record RegisterAgentRequest(
    [Required, StringLength(64)] string AgentId,
    [Required, StringLength(128)] string AgentName,
    [Required, StringLength(45)] string IpAddress,
    int Port,
    List<string>? Capabilities
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
    int Status
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
