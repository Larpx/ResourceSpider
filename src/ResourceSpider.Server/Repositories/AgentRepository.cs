using ResourceSpider.Server.Entities;
using SqlSugar;

namespace ResourceSpider.Server.Repositories;

public interface IAgentRepository
{
    Task<AgentEntity?> GetByIdAsync(string agentId);
    Task<AgentEntity?> GetByTokenAsync(string token);
    Task<List<AgentEntity>> GetAllAsync();
    Task<List<AgentEntity>> GetOnlineAsync();
    Task AddAsync(AgentEntity entity);
    Task UpdateAsync(AgentEntity entity);
    Task DeleteAsync(string agentId);
    Task<long> CountAsync();
    Task<long> CountOnlineAsync();
}

public class AgentRepository : IAgentRepository
{
    private readonly SqlSugarClient _db;

    public AgentRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    public async Task<AgentEntity?> GetByIdAsync(string agentId)
    {
        return await _db.Queryable<AgentEntity>()
            .FirstAsync(x => x.AgentId == agentId);
    }

    public async Task<AgentEntity?> GetByTokenAsync(string token)
    {
        return await _db.Queryable<AgentEntity>()
            .FirstAsync(x => x.AgentToken == token);
    }

    public async Task<List<AgentEntity>> GetAllAsync()
    {
        return await _db.Queryable<AgentEntity>().ToListAsync();
    }

    public async Task<List<AgentEntity>> GetOnlineAsync()
    {
        return await _db.Queryable<AgentEntity>()
            .Where(x => x.Status == 1)
            .ToListAsync();
    }

    public async Task AddAsync(AgentEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    public async Task UpdateAsync(AgentEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.Updateable(entity)
            .IgnoreColumns(x => x.CreatedAt)
            .ExecuteCommandAsync();
    }

    public async Task DeleteAsync(string agentId)
    {
        await _db.Deleteable<AgentEntity>()
            .Where(x => x.AgentId == agentId)
            .ExecuteCommandAsync();
    }

    public async Task<long> CountAsync()
    {
        return await _db.Queryable<AgentEntity>().CountAsync();
    }

    public async Task<long> CountOnlineAsync()
    {
        return await _db.Queryable<AgentEntity>()
            .Where(x => x.Status == 1)
            .CountAsync();
    }
}
