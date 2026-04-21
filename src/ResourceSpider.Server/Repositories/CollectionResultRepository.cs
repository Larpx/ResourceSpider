using ResourceSpider.Server.Entities;
using SqlSugar;

namespace ResourceSpider.Server.Repositories;

public interface ICollectionResultRepository
{
    Task<CollectionResultEntity?> GetByIdAsync(string resultId);
    Task<List<CollectionResultEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize);
    Task<List<CollectionResultEntity>> GetByExpressionIdAsync(string expressionId, int pageIndex, int pageSize);
    Task<long> CountByTaskIdAsync(string taskId);
    Task<long> CountByExpressionIdAsync(string expressionId);
    Task AddAsync(CollectionResultEntity entity);
    Task AddRangeAsync(List<CollectionResultEntity> entities);
    Task DeleteByTaskIdAsync(string taskId);
}

public class CollectionResultRepository : ICollectionResultRepository
{
    private readonly SqlSugarClient _db;

    public CollectionResultRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    public async Task<CollectionResultEntity?> GetByIdAsync(string resultId)
    {
        return await _db.Queryable<CollectionResultEntity>()
            .FirstAsync(x => x.ResultId == resultId);
    }

    public async Task<List<CollectionResultEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize)
    {
        return await _db.Queryable<CollectionResultEntity>()
            .Where(x => x.TaskId == taskId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<CollectionResultEntity>> GetByExpressionIdAsync(string expressionId, int pageIndex, int pageSize)
    {
        return await _db.Queryable<CollectionResultEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountByTaskIdAsync(string taskId)
    {
        return await _db.Queryable<CollectionResultEntity>()
            .Where(x => x.TaskId == taskId)
            .CountAsync();
    }

    public async Task<long> CountByExpressionIdAsync(string expressionId)
    {
        return await _db.Queryable<CollectionResultEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .CountAsync();
    }

    public async Task AddAsync(CollectionResultEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    public async Task AddRangeAsync(List<CollectionResultEntity> entities)
    {
        await _db.Insertable(entities).ExecuteCommandAsync();
    }

    public async Task DeleteByTaskIdAsync(string taskId)
    {
        await _db.Deleteable<CollectionResultEntity>()
            .Where(x => x.TaskId == taskId)
            .ExecuteCommandAsync();
    }
}
