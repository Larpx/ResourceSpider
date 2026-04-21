using SqlSugar;
using ResourceSpider.Server.Entities;

namespace ResourceSpider.Server.Repositories;

public interface ISystemLogRepository
{
    Task<List<SystemLogEntity>> GetListAsync(int pageIndex, int pageSize, string? level = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<long> CountAsync(string? level = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null);
    Task AddAsync(SystemLogEntity entity);
}

public class SystemLogRepository : ISystemLogRepository
{
    private readonly ISqlSugarClient _db;

    public SystemLogRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<List<SystemLogEntity>> GetListAsync(int pageIndex, int pageSize, string? level = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _db.Queryable<SystemLogEntity>();
        if (!string.IsNullOrEmpty(level))
            query = query.Where(l => l.Level == level);
        if (!string.IsNullOrEmpty(category))
            query = query.Where(l => l.Category == category);
        if (startDate.HasValue)
            query = query.Where(l => l.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(l => l.CreatedAt <= endDate.Value);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .ToPageListAsync(pageIndex, pageSize);
    }

    public async Task<long> CountAsync(string? level = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _db.Queryable<SystemLogEntity>();
        if (!string.IsNullOrEmpty(level))
            query = query.Where(l => l.Level == level);
        if (!string.IsNullOrEmpty(category))
            query = query.Where(l => l.Category == category);
        if (startDate.HasValue)
            query = query.Where(l => l.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(l => l.CreatedAt <= endDate.Value);

        return await query.CountAsync();
    }

    public async Task AddAsync(SystemLogEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }
}
