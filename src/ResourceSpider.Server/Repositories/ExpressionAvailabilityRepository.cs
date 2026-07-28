using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Repositories;

/// <summary>
/// 表达式可用性数据仓储接口，定义表达式可用性实体的数据访问操作
/// 支持按表达式和代理节点维度查询可用性状态，以及获取不可用表达式列表
/// </summary>
public interface IExpressionAvailabilityRepository
{
    /// <summary>
    /// 根据表达式 ID 和代理节点 ID 获取可用性记录
    /// </summary>
    /// <param name="expressionId">表达式唯一标识符</param>
    /// <param name="agentId">代理节点唯一标识符</param>
    /// <returns>可用性实体，未找到时返回 null</returns>
    Task<ExpressionAvailabilityEntity?> GetAsync(string expressionId, string agentId);

    /// <summary>
    /// 根据表达式 ID 获取所有代理节点上的可用性记录
    /// </summary>
    /// <param name="expressionId">表达式唯一标识符</param>
    /// <returns>可用性记录列表</returns>
    Task<List<ExpressionAvailabilityEntity>> GetByExpressionIdAsync(string expressionId);

    /// <summary>
    /// 根据代理节点 ID 获取所有表达式的可用性记录
    /// </summary>
    /// <param name="agentId">代理节点唯一标识符</param>
    /// <returns>可用性记录列表</returns>
    Task<List<ExpressionAvailabilityEntity>> GetByAgentIdAsync(string agentId);

    /// <summary>
    /// 新增或更新可用性记录，已存在时更新状态信息
    /// </summary>
    /// <param name="entity">可用性实体</param>
    Task AddOrUpdateAsync(ExpressionAvailabilityEntity entity);

    /// <summary>
    /// 获取所有不可用的表达式记录
    /// </summary>
    /// <returns>不可用表达式记录列表</returns>
    Task<List<ExpressionAvailabilityEntity>> GetUnavailableExpressionsAsync();
}

/// <summary>
/// 表达式可用性数据仓储实现，基于 SQLSugar 提供表达式可用性的增删改查操作
/// </summary>
public class ExpressionAvailabilityRepository : IExpressionAvailabilityRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly SqlSugarClient _db;

    /// <summary>
    /// 初始化表达式可用性仓储实例
    /// </summary>
    /// <param name="sqlSugarClient">SQLSugar 数据库客户端</param>
    public ExpressionAvailabilityRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    /// <inheritdoc/>
    public async Task<ExpressionAvailabilityEntity?> GetAsync(string expressionId, string agentId)
    {
        return await _db.Queryable<ExpressionAvailabilityEntity>()
            .FirstAsync(x => x.ExpressionId == expressionId && x.AgentId == agentId);
    }

    /// <inheritdoc/>
    public async Task<List<ExpressionAvailabilityEntity>> GetByExpressionIdAsync(string expressionId)
    {
        return await _db.Queryable<ExpressionAvailabilityEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<ExpressionAvailabilityEntity>> GetByAgentIdAsync(string agentId)
    {
        return await _db.Queryable<ExpressionAvailabilityEntity>()
            .Where(x => x.AgentId == agentId)
            .ToListAsync();
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<List<ExpressionAvailabilityEntity>> GetUnavailableExpressionsAsync()
    {
        return await _db.Queryable<ExpressionAvailabilityEntity>()
            .Where(x => !x.IsAvailable)
            .ToListAsync();
    }
}
