using ResourceSpider.Server.Entities;
using SqlSugar;

namespace ResourceSpider.Server.Repositories;

public interface IExpressionRepository
{
    Task<ExpressionEntity?> GetByIdAsync(string expressionId);
    Task<List<ExpressionEntity>> GetAllAsync(int pageIndex, int pageSize, int? status = null);
    Task<List<ExpressionEntity>> GetActiveAsync();
    Task<long> CountAsync(int? status = null);
    Task AddAsync(ExpressionEntity entity);
    Task UpdateAsync(ExpressionEntity entity);
    Task DeleteAsync(string expressionId);
    Task IncrementSuccessAsync(string expressionId);
    Task IncrementFailureAsync(string expressionId);
    Task<List<ExpressionEntity>> GetExpiredExpressionsAsync(int consecutiveFailureThreshold);
}

public class ExpressionRepository : IExpressionRepository
{
    private readonly SqlSugarClient _db;

    public ExpressionRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    public async Task<ExpressionEntity?> GetByIdAsync(string expressionId)
    {
        return await _db.Queryable<ExpressionEntity>()
            .FirstAsync(x => x.ExpressionId == expressionId);
    }

    public async Task<List<ExpressionEntity>> GetAllAsync(int pageIndex, int pageSize, int? status = null)
    {
        var query = _db.Queryable<ExpressionEntity>();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }
        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<ExpressionEntity>> GetActiveAsync()
    {
        return await _db.Queryable<ExpressionEntity>()
            .Where(x => x.Status == 1)
            .ToListAsync();
    }

    public async Task<long> CountAsync(int? status = null)
    {
        var query = _db.Queryable<ExpressionEntity>();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }
        return await query.CountAsync();
    }

    public async Task AddAsync(ExpressionEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    public async Task UpdateAsync(ExpressionEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.Updateable(entity)
            .IgnoreColumns(x => x.CreatedAt)
            .ExecuteCommandAsync();
    }

    public async Task DeleteAsync(string expressionId)
    {
        await _db.Deleteable<ExpressionEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .ExecuteCommandAsync();
    }

    public async Task IncrementSuccessAsync(string expressionId)
    {
        await _db.Updateable<ExpressionEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .SetColumns(x => x.SuccessCount == x.SuccessCount + 1)
            .SetColumns(x => x.ConsecutiveFailures == 0)
            .SetColumns(x => x.LastValidatedAt == DateTime.UtcNow)
            .SetColumns(x => x.LastUsedAt == DateTime.UtcNow)
            .SetColumns(x => x.UpdatedAt == DateTime.UtcNow)
            .ExecuteCommandAsync();
    }

    public async Task IncrementFailureAsync(string expressionId)
    {
        await _db.Updateable<ExpressionEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .SetColumns(x => x.FailureCount == x.FailureCount + 1)
            .SetColumns(x => x.ConsecutiveFailures == x.ConsecutiveFailures + 1)
            .SetColumns(x => x.LastUsedAt == DateTime.UtcNow)
            .SetColumns(x => x.UpdatedAt == DateTime.UtcNow)
            .ExecuteCommandAsync();
    }

    public async Task<List<ExpressionEntity>> GetExpiredExpressionsAsync(int consecutiveFailureThreshold)
    {
        return await _db.Queryable<ExpressionEntity>()
            .Where(x => x.Status == 1 && x.ConsecutiveFailures >= consecutiveFailureThreshold)
            .ToListAsync();
    }
}
