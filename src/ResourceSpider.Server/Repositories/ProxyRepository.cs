using ResourceSpider.Server.Entities;
using SqlSugar;

namespace ResourceSpider.Server.Repositories;

public interface IProxyRepository
{
    Task<ProxyEntity?> GetByIdAsync(string proxyId);
    Task<List<ProxyEntity>> GetAllAsync(int pageIndex, int pageSize);
    Task<long> CountAsync();
    Task<List<ProxyEntity>> GetAvailableAsync();
    Task AddAsync(ProxyEntity entity);
    Task UpdateAsync(ProxyEntity entity);
    Task DeleteAsync(string proxyId);
}

public class ProxyRepository : IProxyRepository
{
    private readonly SqlSugarClient _db;

    public ProxyRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    public async Task<ProxyEntity?> GetByIdAsync(string proxyId)
    {
        return await _db.Queryable<ProxyEntity>()
            .FirstAsync(x => x.ProxyId == proxyId);
    }

    public async Task<List<ProxyEntity>> GetAllAsync(int pageIndex, int pageSize)
    {
        return await _db.Queryable<ProxyEntity>()
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountAsync()
    {
        return await _db.Queryable<ProxyEntity>().CountAsync();
    }

    public async Task<List<ProxyEntity>> GetAvailableAsync()
    {
        return await _db.Queryable<ProxyEntity>()
            .Where(x => x.Status == 1)
            .ToListAsync();
    }

    public async Task AddAsync(ProxyEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    public async Task UpdateAsync(ProxyEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.Updateable(entity)
            .IgnoreColumns(x => x.CreatedAt)
            .ExecuteCommandAsync();
    }

    public async Task DeleteAsync(string proxyId)
    {
        await _db.Deleteable<ProxyEntity>()
            .Where(x => x.ProxyId == proxyId)
            .ExecuteCommandAsync();
    }
}
