using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Repositories;

/// <summary>
/// 采集结果数据仓储接口，定义采集结果实体的数据访问操作
/// 支持按任务和表达式查询采集结果，以及批量写入和删除
/// </summary>
public interface ICollectionResultRepository
{
    /// <summary>
    /// 根据结果 ID 获取采集结果实体
    /// </summary>
    /// <param name="resultId">结果唯一标识符</param>
    /// <returns>采集结果实体，未找到时返回 null</returns>
    Task<CollectionResultEntity?> GetByIdAsync(string resultId);

    /// <summary>
    /// 根据任务 ID 分页获取采集结果列表
    /// </summary>
    /// <param name="taskId">任务唯一标识符</param>
    /// <param name="pageIndex">页码，从 1 开始</param>
    /// <param name="pageSize">每页记录数</param>
    /// <returns>采集结果列表，按创建时间倒序排列</returns>
    Task<List<CollectionResultEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize);

    /// <summary>
    /// 根据表达式 ID 分页获取采集结果列表
    /// </summary>
    /// <param name="expressionId">表达式唯一标识符</param>
    /// <param name="pageIndex">页码，从 1 开始</param>
    /// <param name="pageSize">每页记录数</param>
    /// <returns>采集结果列表，按创建时间倒序排列</returns>
    Task<List<CollectionResultEntity>> GetByExpressionIdAsync(string expressionId, int pageIndex, int pageSize);

    /// <summary>
    /// 统计指定任务的采集结果数量
    /// </summary>
    /// <param name="taskId">任务唯一标识符</param>
    /// <returns>采集结果总数</returns>
    Task<long> CountByTaskIdAsync(string taskId);

    /// <summary>
    /// 统计指定表达式的采集结果数量
    /// </summary>
    /// <param name="expressionId">表达式唯一标识符</param>
    /// <returns>采集结果总数</returns>
    Task<long> CountByExpressionIdAsync(string expressionId);

    /// <summary>
    /// 按条件分页查询采集结果。
    /// </summary>
    Task<List<CollectionResultEntity>> QueryAsync(string? taskId, string? stepId, string? agentId, string? keyword, DateTime? startTime, DateTime? endTime, bool? isDuplicate, int pageIndex, int pageSize);

    /// <summary>
    /// 统计符合条件的采集结果数量。
    /// </summary>
    Task<long> CountAsync(string? taskId, string? stepId, string? agentId, string? keyword, DateTime? startTime, DateTime? endTime, bool? isDuplicate);

    /// <summary>
    /// 根据数据指纹判断记录是否已存在。
    /// </summary>
    Task<bool> ExistsByFingerprintAsync(string taskId, string agentId, string fingerprint);

    /// <summary>
    /// 新增单条采集结果
    /// </summary>
    /// <param name="entity">采集结果实体</param>
    Task AddAsync(CollectionResultEntity entity);

    /// <summary>
    /// 批量新增采集结果
    /// </summary>
    /// <param name="entities">采集结果实体列表</param>
    Task AddRangeAsync(List<CollectionResultEntity> entities);

    /// <summary>
    /// 根据任务 ID 删除所有采集结果
    /// </summary>
    /// <param name="taskId">任务唯一标识符</param>
    Task DeleteByTaskIdAsync(string taskId);
}

/// <summary>
/// 采集结果数据仓储实现，基于 SQLSugar 提供采集结果的增删改查操作
/// </summary>
public class CollectionResultRepository : ICollectionResultRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly SqlSugarClient _db;

    /// <summary>
    /// 初始化采集结果仓储实例
    /// </summary>
    /// <param name="sqlSugarClient">SQLSugar 数据库客户端</param>
    public CollectionResultRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    /// <inheritdoc/>
    public async Task<CollectionResultEntity?> GetByIdAsync(string resultId)
    {
        return await _db.Queryable<CollectionResultEntity>()
            .FirstAsync(x => x.ResultId == resultId);
    }

    /// <inheritdoc/>
    public async Task<List<CollectionResultEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize)
    {
        return await _db.Queryable<CollectionResultEntity>()
            .Where(x => x.TaskId == taskId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<CollectionResultEntity>> GetByExpressionIdAsync(string expressionId, int pageIndex, int pageSize)
    {
        return await _db.Queryable<CollectionResultEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<long> CountByTaskIdAsync(string taskId)
    {
        return await _db.Queryable<CollectionResultEntity>()
            .Where(x => x.TaskId == taskId)
            .CountAsync();
    }

    /// <inheritdoc/>
    public async Task<long> CountByExpressionIdAsync(string expressionId)
    {
        return await _db.Queryable<CollectionResultEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .CountAsync();
    }

    /// <inheritdoc/>
    public async Task<List<CollectionResultEntity>> QueryAsync(string? taskId, string? stepId, string? agentId, string? keyword, DateTime? startTime, DateTime? endTime, bool? isDuplicate, int pageIndex, int pageSize)
    {
        var query = BuildFilteredQuery(taskId, stepId, agentId, keyword, startTime, endTime, isDuplicate);

        return await query
            .OrderByDescending(x => x.CollectedAt)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<long> CountAsync(string? taskId, string? stepId, string? agentId, string? keyword, DateTime? startTime, DateTime? endTime, bool? isDuplicate)
    {
        var query = BuildFilteredQuery(taskId, stepId, agentId, keyword, startTime, endTime, isDuplicate);
        return await query.CountAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsByFingerprintAsync(string taskId, string agentId, string fingerprint)
    {
        return await _db.Queryable<CollectionResultEntity>()
            .AnyAsync(x => x.TaskId == taskId && x.AgentId == agentId && x.DataFingerprint == fingerprint);
    }

    /// <inheritdoc/>
    public async Task AddAsync(CollectionResultEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(List<CollectionResultEntity> entities)
    {
        await _db.Insertable(entities).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteByTaskIdAsync(string taskId)
    {
        await _db.Deleteable<CollectionResultEntity>()
            .Where(x => x.TaskId == taskId)
            .ExecuteCommandAsync();
    }

    private ISugarQueryable<CollectionResultEntity> BuildFilteredQuery(string? taskId, string? stepId, string? agentId, string? keyword, DateTime? startTime, DateTime? endTime, bool? isDuplicate)
    {
        var query = _db.Queryable<CollectionResultEntity>();

        if (!string.IsNullOrWhiteSpace(taskId))
        {
            query = query.Where(x => x.TaskId == taskId);
        }

        if (!string.IsNullOrWhiteSpace(stepId))
        {
            query = query.Where(x => x.StepId == stepId);
        }

        if (!string.IsNullOrWhiteSpace(agentId))
        {
            query = query.Where(x => x.AgentId == agentId);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => (x.SourceUrl != null && x.SourceUrl.Contains(keyword))
                || (x.TaskName != null && x.TaskName.Contains(keyword))
                || x.Fields.Contains(keyword));
        }

        if (startTime.HasValue)
        {
            query = query.Where(x => x.CollectedAt >= startTime.Value || x.CreatedAt >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(x => x.CollectedAt <= endTime.Value || x.CreatedAt <= endTime.Value);
        }

        if (isDuplicate.HasValue)
        {
            query = query.Where(x => x.IsDuplicate == isDuplicate.Value);
        }

        return query;
    }
}
