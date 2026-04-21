using SqlSugar;
using ResourceSpider.Server.Entities;

namespace ResourceSpider.Server.Repositories;

public interface IAgentGroupRepository
{
    Task<AgentGroupEntity?> GetByIdAsync(string groupId);
    Task<List<AgentGroupEntity>> GetAllAsync();
    Task AddAsync(AgentGroupEntity entity);
    Task UpdateAsync(AgentGroupEntity entity);
    Task DeleteAsync(string groupId);
}

public class AgentGroupRepository : IAgentGroupRepository
{
    private readonly ISqlSugarClient _db;

    public AgentGroupRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<AgentGroupEntity?> GetByIdAsync(string groupId)
    {
        return await _db.Queryable<AgentGroupEntity>()
            .FirstAsync(g => g.GroupId == groupId);
    }

    public async Task<List<AgentGroupEntity>> GetAllAsync()
    {
        return await _db.Queryable<AgentGroupEntity>()
            .OrderBy(g => g.GroupName)
            .ToListAsync();
    }

    public async Task AddAsync(AgentGroupEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    public async Task UpdateAsync(AgentGroupEntity entity)
    {
        await _db.Updateable(entity)
            .IgnoreColumns(g => g.CreatedAt)
            .ExecuteCommandAsync();
    }

    public async Task DeleteAsync(string groupId)
    {
        await _db.Deleteable<AgentGroupEntity>()
            .Where(g => g.GroupId == groupId)
            .ExecuteCommandAsync();
    }
}
