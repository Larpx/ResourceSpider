using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Repositories;

/// <summary>
/// 表达式数据仓储接口，定义表达式实体的数据访问操作
/// 支持表达式的增删改查、成功/失败计数递增和过期表达式查询
/// </summary>
public interface IExpressionRepository
{
    /// <summary>
    /// 根据表达式 ID 获取表达式实体
    /// </summary>
    /// <param name="expressionId">表达式唯一标识符</param>
    /// <returns>表达式实体，未找到时返回 null</returns>
    Task<ExpressionEntity?> GetByIdAsync(string expressionId);

    /// <summary>
    /// 分页获取表达式列表，支持按状态筛选
    /// </summary>
    /// <param name="pageIndex">页码，从 1 开始</param>
    /// <param name="pageSize">每页记录数</param>
    /// <param name="status">状态筛选，为 null 时不筛选</param>
    /// <param name="keyword">关键字筛选，为 null 时不筛选</param>
    /// <returns>表达式列表，按创建时间倒序排列</returns>
    Task<List<ExpressionEntity>> GetAllAsync(int pageIndex, int pageSize, int? status = null, string? keyword = null);

    /// <summary>
    /// 获取所有启用状态的表达式
    /// </summary>
    /// <returns>启用状态的表达式列表</returns>
    Task<List<ExpressionEntity>> GetActiveAsync();

    /// <summary>
    /// 统计表达式数量，支持按状态筛选
    /// </summary>
    /// <param name="status">状态筛选，为 null 时不筛选</param>
    /// <param name="keyword">关键字筛选，为 null 时不筛选</param>
    /// <returns>表达式总数</returns>
    Task<long> CountAsync(int? status = null, string? keyword = null);

    /// <summary>
    /// 新增表达式实体
    /// </summary>
    /// <param name="entity">表达式实体</param>
    Task AddAsync(ExpressionEntity entity);

    /// <summary>
    /// 更新表达式实体，忽略 CreatedAt 字段
    /// </summary>
    /// <param name="entity">表达式实体</param>
    Task UpdateAsync(ExpressionEntity entity);

    /// <summary>
    /// 根据表达式 ID 删除表达式
    /// </summary>
    /// <param name="expressionId">表达式唯一标识符</param>
    Task DeleteAsync(string expressionId);

    /// <summary>
    /// 递增表达式成功计数，同时重置连续失败计数
    /// </summary>
    /// <param name="expressionId">表达式唯一标识符</param>
    Task IncrementSuccessAsync(string expressionId);

    /// <summary>
    /// 递增表达式失败计数和连续失败计数
    /// </summary>
    /// <param name="expressionId">表达式唯一标识符</param>
    Task IncrementFailureAsync(string expressionId);

    /// <summary>
    /// 获取连续失败次数超过阈值的启用表达式，用于自动禁用检测
    /// </summary>
    /// <param name="consecutiveFailureThreshold">连续失败次数阈值</param>
    /// <returns>超过阈值的表达式列表</returns>
    Task<List<ExpressionEntity>> GetExpiredExpressionsAsync(int consecutiveFailureThreshold);
}

/// <summary>
/// 表达式数据仓储实现，基于 SQLSugar 提供表达式的增删改查操作
/// </summary>
public class ExpressionRepository : IExpressionRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly SqlSugarClient _db;

    /// <summary>
    /// 初始化表达式仓储实例
    /// </summary>
    /// <param name="sqlSugarClient">SQLSugar 数据库客户端</param>
    public ExpressionRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    /// <inheritdoc/>
    public async Task<ExpressionEntity?> GetByIdAsync(string expressionId)
    {
        return await _db.Queryable<ExpressionEntity>()
            .FirstAsync(x => x.ExpressionId == expressionId);
    }

    /// <inheritdoc/>
    public async Task<List<ExpressionEntity>> GetAllAsync(int pageIndex, int pageSize, int? status = null, string? keyword = null)
    {
        var query = _db.Queryable<ExpressionEntity>();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.Name.Contains(keyword) || x.ExpressionId.Contains(keyword) || (x.Description != null && x.Description.Contains(keyword)));
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<ExpressionEntity>> GetActiveAsync()
    {
        return await _db.Queryable<ExpressionEntity>()
            .Where(x => x.Status == 1)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<long> CountAsync(int? status = null, string? keyword = null)
    {
        var query = _db.Queryable<ExpressionEntity>();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.Name.Contains(keyword) || x.ExpressionId.Contains(keyword) || (x.Description != null && x.Description.Contains(keyword)));
        }

        return await query.CountAsync();
    }

    /// <inheritdoc/>
    public async Task AddAsync(ExpressionEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(ExpressionEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.Updateable(entity)
            .IgnoreColumns(x => x.CreatedAt)
            .ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string expressionId)
    {
        await _db.Deleteable<ExpressionEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .ExecuteCommandAsync();
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<List<ExpressionEntity>> GetExpiredExpressionsAsync(int consecutiveFailureThreshold)
    {
        return await _db.Queryable<ExpressionEntity>()
            .Where(x => x.Status == 1 && x.ConsecutiveFailures >= consecutiveFailureThreshold)
            .ToListAsync();
    }
}
