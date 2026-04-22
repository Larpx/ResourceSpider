using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

/// <summary>
/// Agent 分组服务接口，提供 Agent 分组的增删改查功能
/// </summary>
public interface IAgentGroupService
{
    /// <summary>
    /// 创建新的 Agent 分组
    /// </summary>
    /// <param name="request">创建分组请求</param>
    /// <returns>创建后的分组 DTO</returns>
    Task<AgentGroupDto> CreateAsync(CreateAgentGroupRequest request);

    /// <summary>
    /// 根据分组标识获取分组详情
    /// </summary>
    /// <param name="groupId">分组唯一标识</param>
    /// <returns>分组 DTO，若不存在返回 null</returns>
    Task<AgentGroupDto?> GetByIdAsync(string groupId);

    /// <summary>
    /// 获取所有 Agent 分组列表
    /// </summary>
    /// <returns>分组 DTO 列表</returns>
    Task<List<AgentGroupDto>> GetAllAsync();

    /// <summary>
    /// 更新分组信息
    /// </summary>
    /// <param name="groupId">分组唯一标识</param>
    /// <param name="request">更新分组请求</param>
    /// <returns>更新成功返回 true，分组不存在返回 false</returns>
    Task<bool> UpdateAsync(string groupId, UpdateAgentGroupRequest request);

    /// <summary>
    /// 删除指定分组
    /// </summary>
    /// <param name="groupId">分组唯一标识</param>
    /// <returns>删除成功返回 true，分组不存在返回 false</returns>
    Task<bool> DeleteAsync(string groupId);
}

/// <summary>
/// Agent 分组服务实现，管理 Agent 分组的创建、查询、更新和删除操作
/// </summary>
public class AgentGroupService : IAgentGroupService
{
    /// <summary>
    /// Agent 分组数据仓库，用于分组实体的持久化操作
    /// </summary>
    private readonly IAgentGroupRepository _repository;

    /// <summary>
    /// 日志记录器，用于记录分组操作相关事件
    /// </summary>
    private readonly ILogger<AgentGroupService> _logger;

    /// <summary>
    /// 初始化 Agent 分组服务实例
    /// </summary>
    /// <param name="repository">Agent 分组数据仓库</param>
    /// <param name="logger">日志记录器</param>
    public AgentGroupService(IAgentGroupRepository repository, ILogger<AgentGroupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AgentGroupDto> CreateAsync(CreateAgentGroupRequest request)
    {
        var entity = new AgentGroupEntity
        {
            GroupId = Guid.NewGuid().ToString("N"),
            GroupName = request.GroupName,
            Description = request.Description,
            AgentIds = request.AgentIds != null
                ? Newtonsoft.Json.JsonConvert.SerializeObject(request.AgentIds)
                : null
        };

        await _repository.AddAsync(entity);
        _logger.LogInformation("创建 Agent 分组 {GroupId}：{GroupName}", entity.GroupId, entity.GroupName);

        return MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<AgentGroupDto?> GetByIdAsync(string groupId)
    {
        var entity = await _repository.GetByIdAsync(groupId);
        return entity != null ? MapToDto(entity) : null;
    }

    /// <inheritdoc />
    public async Task<List<AgentGroupDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(string groupId, UpdateAgentGroupRequest request)
    {
        var entity = await _repository.GetByIdAsync(groupId);
        if (entity == null) return false;

        if (request.GroupName != null) entity.GroupName = request.GroupName;
        if (request.Description != null) entity.Description = request.Description;
        if (request.AgentIds != null)
            entity.AgentIds = Newtonsoft.Json.JsonConvert.SerializeObject(request.AgentIds);

        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string groupId)
    {
        var entity = await _repository.GetByIdAsync(groupId);
        if (entity == null) return false;

        await _repository.DeleteAsync(groupId);
        return true;
    }

    /// <summary>
    /// 将 Agent 分组实体映射为分组 DTO
    /// </summary>
    /// <param name="entity">分组实体</param>
    /// <returns>分组 DTO</returns>
    private static AgentGroupDto MapToDto(AgentGroupEntity entity)
    {
        var agentIds = !string.IsNullOrEmpty(entity.AgentIds)
            ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(entity.AgentIds) ?? new()
            : new List<string>();

        return new AgentGroupDto(
            entity.GroupId,
            entity.GroupName,
            entity.Description,
            agentIds,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
