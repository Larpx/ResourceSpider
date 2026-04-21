using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

public record CreateAgentGroupRequest(
    [Required, StringLength(128)] string GroupName,
    [StringLength(512)] string? Description = null,
    List<string>? AgentIds = null
);

public record UpdateAgentGroupRequest(
    [StringLength(128)] string? GroupName,
    [StringLength(512)] string? Description,
    List<string>? AgentIds
);

public record AgentGroupDto(
    string GroupId,
    string GroupName,
    string? Description,
    List<string> AgentIds,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
