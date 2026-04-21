using ResourceSpider.Server.Entities;
using SqlSugar;

namespace ResourceSpider.Server.Repositories;

public interface IExpressionFieldRepository
{
    Task<List<ExpressionFieldEntity>> GetByExpressionIdAsync(string expressionId);
    Task AddAsync(ExpressionFieldEntity entity);
    Task AddRangeAsync(List<ExpressionFieldEntity> entities);
    Task DeleteByExpressionIdAsync(string expressionId);
}

public class ExpressionFieldRepository : IExpressionFieldRepository
{
    private readonly SqlSugarClient _db;

    public ExpressionFieldRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    public async Task<List<ExpressionFieldEntity>> GetByExpressionIdAsync(string expressionId)
    {
        return await _db.Queryable<ExpressionFieldEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .OrderBy(x => x.Order)
            .ToListAsync();
    }

    public async Task AddAsync(ExpressionFieldEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    public async Task AddRangeAsync(List<ExpressionFieldEntity> entities)
    {
        await _db.Insertable(entities).ExecuteCommandAsync();
    }

    public async Task DeleteByExpressionIdAsync(string expressionId)
    {
        await _db.Deleteable<ExpressionFieldEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .ExecuteCommandAsync();
    }
}
