using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

/// <summary>
/// 创建代理分组请求
/// </summary>
/// <param name="GroupName">分组名称，最大长度 128</param>
/// <param name="Description">分组描述，最大长度 512，可选</param>
/// <param name="AgentIds">初始代理 ID 列表，可选</param>
public record CreateAgentGroupRequest(
    [Required, StringLength(128)] string GroupName,
    [StringLength(512)] string? Description = null,
    List<string>? AgentIds = null
);

/// <summary>
/// 更新代理分组请求
/// </summary>
/// <param name="GroupName">分组名称，可选</param>
/// <param name="Description">分组描述，可选</param>
/// <param name="AgentIds">代理 ID 列表，可选</param>
public record UpdateAgentGroupRequest(
    [StringLength(128)] string? GroupName,
    [StringLength(512)] string? Description,
    List<string>? AgentIds
);

/// <summary>
/// 代理分组数据传输对象
/// </summary>
/// <param name="GroupId">分组 ID</param>
/// <param name="GroupName">分组名称</param>
/// <param name="Description">分组描述</param>
/// <param name="AgentIds">分组内的代理 ID 列表</param>
/// <param name="CreatedAt">创建时间</param>
/// <param name="UpdatedAt">更新时间</param>
public record AgentGroupDto(
    string GroupId,
    string GroupName,
    string? Description,
    List<string> AgentIds,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
