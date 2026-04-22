using ResourceSpider.Server.Entities;
using SqlSugar;

namespace ResourceSpider.Server.Repositories;

/// <summary>
/// 表达式字段数据仓储接口，定义表达式字段实体的数据访问操作
/// 支持按表达式查询字段列表、批量写入和删除
/// </summary>
public interface IExpressionFieldRepository
{
    /// <summary>
    /// 根据表达式 ID 获取字段列表，按排序序号排列
    /// </summary>
    /// <param name="expressionId">表达式唯一标识符</param>
    /// <returns>表达式字段列表</returns>
    Task<List<ExpressionFieldEntity>> GetByExpressionIdAsync(string expressionId);

    /// <summary>
    /// 新增单条表达式字段
    /// </summary>
    /// <param name="entity">表达式字段实体</param>
    Task AddAsync(ExpressionFieldEntity entity);

    /// <summary>
    /// 批量新增表达式字段
    /// </summary>
    /// <param name="entities">表达式字段实体列表</param>
    Task AddRangeAsync(List<ExpressionFieldEntity> entities);

    /// <summary>
    /// 根据表达式 ID 删除所有字段
    /// </summary>
    /// <param name="expressionId">表达式唯一标识符</param>
    Task DeleteByExpressionIdAsync(string expressionId);
}

/// <summary>
/// 表达式字段数据仓储实现，基于 SQLSugar 提供表达式字段的增删改查操作
/// </summary>
public class ExpressionFieldRepository : IExpressionFieldRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly SqlSugarClient _db;

    /// <summary>
    /// 初始化表达式字段仓储实例
    /// </summary>
    /// <param name="sqlSugarClient">SQLSugar 数据库客户端</param>
    public ExpressionFieldRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    /// <inheritdoc/>
    public async Task<List<ExpressionFieldEntity>> GetByExpressionIdAsync(string expressionId)
    {
        return await _db.Queryable<ExpressionFieldEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .OrderBy(x => x.Order)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task AddAsync(ExpressionFieldEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(List<ExpressionFieldEntity> entities)
    {
        await _db.Insertable(entities).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteByExpressionIdAsync(string expressionId)
    {
        await _db.Deleteable<ExpressionFieldEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .ExecuteCommandAsync();
    }
}
