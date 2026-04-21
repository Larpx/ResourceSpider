using SqlSugar;
using ResourceSpider.Server.Entities;

namespace ResourceSpider.Server.Repositories;

public interface ICrawlResultRepository
{
    Task<CrawlResultEntity?> GetByIdAsync(string resultId);
    Task<List<CrawlResultEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize);
    Task<List<CrawlResultEntity>> GetByExecutionIdAsync(string executionId, int pageIndex, int pageSize);
    Task<long> CountByTaskIdAsync(string taskId);
    Task AddAsync(CrawlResultEntity entity);
    Task AddRangeAsync(List<CrawlResultEntity> entities);
    Task DeleteByTaskIdAsync(string taskId);
}

public class CrawlResultRepository : ICrawlResultRepository
{
    private readonly ISqlSugarClient _db;

    public CrawlResultRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<CrawlResultEntity?> GetByIdAsync(string resultId)
    {
        return await _db.Queryable<CrawlResultEntity>()
            .FirstAsync(r => r.ResultId == resultId);
    }

    public async Task<List<CrawlResultEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize)
    {
        return await _db.Queryable<CrawlResultEntity>()
            .Where(r => r.TaskId == taskId)
            .OrderByDescending(r => r.CreatedAt)
            .ToPageListAsync(pageIndex, pageSize);
    }

    public async Task<List<CrawlResultEntity>> GetByExecutionIdAsync(string executionId, int pageIndex, int pageSize)
    {
        return await _db.Queryable<CrawlResultEntity>()
            .Where(r => r.ExecutionId == executionId)
            .OrderByDescending(r => r.CreatedAt)
            .ToPageListAsync(pageIndex, pageSize);
    }

    public async Task<long> CountByTaskIdAsync(string taskId)
    {
        return await _db.Queryable<CrawlResultEntity>()
            .Where(r => r.TaskId == taskId)
            .CountAsync();
    }

    public async Task AddAsync(CrawlResultEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    public async Task AddRangeAsync(List<CrawlResultEntity> entities)
    {
        await _db.Insertable(entities).ExecuteCommandAsync();
    }

    public async Task DeleteByTaskIdAsync(string taskId)
    {
        await _db.Deleteable<CrawlResultEntity>()
            .Where(r => r.TaskId == taskId)
            .ExecuteCommandAsync();
    }
}
