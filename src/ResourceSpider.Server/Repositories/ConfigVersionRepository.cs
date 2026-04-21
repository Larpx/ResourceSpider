using SqlSugar;
using ResourceSpider.Server.Entities;

namespace ResourceSpider.Server.Repositories;

public interface IConfigVersionRepository
{
    Task<List<ConfigVersionEntity>> GetByTaskIdAsync(string taskId);
    Task<ConfigVersionEntity?> GetByVersionAsync(string taskId, int version);
    Task AddAsync(ConfigVersionEntity entity);
}

public class ConfigVersionRepository : IConfigVersionRepository
{
    private readonly ISqlSugarClient _db;

    public ConfigVersionRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<List<ConfigVersionEntity>> GetByTaskIdAsync(string taskId)
    {
        return await _db.Queryable<ConfigVersionEntity>()
            .Where(v => v.TaskId == taskId)
            .OrderByDescending(v => v.Version)
            .ToListAsync();
    }

    public async Task<ConfigVersionEntity?> GetByVersionAsync(string taskId, int version)
    {
        return await _db.Queryable<ConfigVersionEntity>()
            .FirstAsync(v => v.TaskId == taskId && v.Version == version);
    }

    public async Task AddAsync(ConfigVersionEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }
}
