using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface IAgentGroupService
{
    Task<AgentGroupDto> CreateAsync(CreateAgentGroupRequest request);
    Task<AgentGroupDto?> GetByIdAsync(string groupId);
    Task<List<AgentGroupDto>> GetAllAsync();
    Task<bool> UpdateAsync(string groupId, UpdateAgentGroupRequest request);
    Task<bool> DeleteAsync(string groupId);
}

public class AgentGroupService : IAgentGroupService
{
    private readonly IAgentGroupRepository _repository;
    private readonly ILogger<AgentGroupService> _logger;

    public AgentGroupService(IAgentGroupRepository repository, ILogger<AgentGroupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

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

    public async Task<AgentGroupDto?> GetByIdAsync(string groupId)
    {
        var entity = await _repository.GetByIdAsync(groupId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<List<AgentGroupDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto).ToList();
    }

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

    public async Task<bool> DeleteAsync(string groupId)
    {
        var entity = await _repository.GetByIdAsync(groupId);
        if (entity == null) return false;

        await _repository.DeleteAsync(groupId);
        return true;
    }

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
