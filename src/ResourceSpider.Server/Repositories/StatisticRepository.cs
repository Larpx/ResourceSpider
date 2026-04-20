using ResourceSpider.Server.Entities;
using SqlSugar;

namespace ResourceSpider.Server.Repositories;

public interface IStatisticRepository
{
    Task<StatisticEntity?> GetByAgentAndDateAsync(string agentId, DateTime date);
    Task<List<StatisticEntity>> GetByAgentAsync(string agentId, DateTime startDate, DateTime endDate);
    Task AddAsync(StatisticEntity entity);
    Task UpdateAsync(StatisticEntity entity);
    Task<List<StatisticEntity>> GetSystemTrendAsync(DateTime startDate, DateTime endDate);
}

public class StatisticRepository : IStatisticRepository
{
    private readonly SqlSugarClient _db;

    public StatisticRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    public async Task<StatisticEntity?> GetByAgentAndDateAsync(string agentId, DateTime date)
    {
        return await _db.Queryable<StatisticEntity>()
            .FirstAsync(x => x.AgentId == agentId && x.StatDate == date);
    }

    public async Task<List<StatisticEntity>> GetByAgentAsync(
        string agentId, DateTime startDate, DateTime endDate)
    {
        return await _db.Queryable<StatisticEntity>()
            .Where(x => x.AgentId == agentId 
                && x.StatDate >= startDate 
                && x.StatDate <= endDate)
            .OrderBy(x => x.StatDate)
            .ToListAsync();
    }

    public async Task AddAsync(StatisticEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    public async Task UpdateAsync(StatisticEntity entity)
    {
        await _db.Updateable(entity).ExecuteCommandAsync();
    }

    public async Task<List<StatisticEntity>> GetSystemTrendAsync(
        DateTime startDate, DateTime endDate)
    {
        return await _db.Queryable<StatisticEntity>()
            .Where(x => x.StatDate >= startDate && x.StatDate <= endDate)
            .OrderBy(x => x.StatDate)
            .ToListAsync();
    }
}
