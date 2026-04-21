using ResourceSpider.Server.Entities;
using SqlSugar;

namespace ResourceSpider.Server.Repositories;

public interface IExpressionAvailabilityRepository
{
    Task<ExpressionAvailabilityEntity?> GetAsync(string expressionId, string agentId);
    Task<List<ExpressionAvailabilityEntity>> GetByExpressionIdAsync(string expressionId);
    Task<List<ExpressionAvailabilityEntity>> GetByAgentIdAsync(string agentId);
    Task AddOrUpdateAsync(ExpressionAvailabilityEntity entity);
    Task<List<ExpressionAvailabilityEntity>> GetUnavailableExpressionsAsync();
}

public class ExpressionAvailabilityRepository : IExpressionAvailabilityRepository
{
    private readonly SqlSugarClient _db;

    public ExpressionAvailabilityRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    public async Task<ExpressionAvailabilityEntity?> GetAsync(string expressionId, string agentId)
    {
        return await _db.Queryable<ExpressionAvailabilityEntity>()
            .FirstAsync(x => x.ExpressionId == expressionId && x.AgentId == agentId);
    }

    public async Task<List<ExpressionAvailabilityEntity>> GetByExpressionIdAsync(string expressionId)
    {
        return await _db.Queryable<ExpressionAvailabilityEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .ToListAsync();
    }

    public async Task<List<ExpressionAvailabilityEntity>> GetByAgentIdAsync(string agentId)
    {
        return await _db.Queryable<ExpressionAvailabilityEntity>()
            .Where(x => x.AgentId == agentId)
            .ToListAsync();
    }

    public async Task AddOrUpdateAsync(ExpressionAvailabilityEntity entity)
    {
        var existing = await GetAsync(entity.ExpressionId, entity.AgentId);
        if (existing != null)
        {
            existing.IsAvailable = entity.IsAvailable;
            existing.FailureReason = entity.FailureReason;
            existing.LastCheckedAt = entity.LastCheckedAt;
            existing.LastSuccessAt = entity.LastSuccessAt ?? existing.LastSuccessAt;
            existing.LastFailureAt = entity.LastFailureAt ?? existing.LastFailureAt;
            existing.ConsecutiveFailures = entity.ConsecutiveFailures;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.Updateable(existing)
                .IgnoreColumns(x => x.CreatedAt)
                .ExecuteCommandAsync();
        }
        else
        {
            await _db.Insertable(entity).ExecuteCommandAsync();
        }
    }

    public async Task<List<ExpressionAvailabilityEntity>> GetUnavailableExpressionsAsync()
    {
        return await _db.Queryable<ExpressionAvailabilityEntity>()
            .Where(x => !x.IsAvailable)
            .ToListAsync();
    }
}
