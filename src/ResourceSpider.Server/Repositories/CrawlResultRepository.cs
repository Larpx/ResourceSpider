using SqlSugar;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;

namespace Larpx.PersonalTools.ResourceSpider.Server.Repositories;

/// <summary>
/// 爬取结果数据仓储接口，定义爬取结果实体的数据访问操作
/// 支持按任务和执行记录查询爬取结果，以及批量写入和删除
/// </summary>
public interface ICrawlResultRepository
{
    /// <summary>
    /// 根据结果 ID 获取爬取结果实体
    /// </summary>
    /// <param name="resultId">结果唯一标识符</param>
    /// <returns>爬取结果实体，未找到时返回 null</returns>
    Task<CrawlResultEntity?> GetByIdAsync(string resultId);

    /// <summary>
    /// 根据任务 ID 分页获取爬取结果列表
    /// </summary>
    /// <param name="taskId">任务唯一标识符</param>
    /// <param name="pageIndex">页码，从 1 开始</param>
    /// <param name="pageSize">每页记录数</param>
    /// <returns>爬取结果列表，按创建时间倒序排列</returns>
    Task<List<CrawlResultEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize);

    /// <summary>
    /// 根据执行记录 ID 分页获取爬取结果列表
    /// </summary>
    /// <param name="executionId">执行记录唯一标识符</param>
    /// <param name="pageIndex">页码，从 1 开始</param>
    /// <param name="pageSize">每页记录数</param>
    /// <returns>爬取结果列表，按创建时间倒序排列</returns>
    Task<List<CrawlResultEntity>> GetByExecutionIdAsync(string executionId, int pageIndex, int pageSize);

    /// <summary>
    /// 统计指定任务的爬取结果数量
    /// </summary>
    /// <param name="taskId">任务唯一标识符</param>
    /// <returns>爬取结果总数</returns>
    Task<long> CountByTaskIdAsync(string taskId);

    /// <summary>
    /// 新增单条爬取结果
    /// </summary>
    /// <param name="entity">爬取结果实体</param>
    Task AddAsync(CrawlResultEntity entity);

    /// <summary>
    /// 批量新增爬取结果
    /// </summary>
    /// <param name="entities">爬取结果实体列表</param>
    Task AddRangeAsync(List<CrawlResultEntity> entities);

    /// <summary>
    /// 根据任务 ID 删除所有爬取结果
    /// </summary>
    /// <param name="taskId">任务唯一标识符</param>
    Task DeleteByTaskIdAsync(string taskId);
}

/// <summary>
/// 爬取结果数据仓储实现，基于 SQLSugar 提供爬取结果的增删改查操作
/// </summary>
public class CrawlResultRepository : ICrawlResultRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 初始化爬取结果仓储实例
    /// </summary>
    /// <param name="db">SQLSugar 数据库客户端</param>
    public CrawlResultRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<CrawlResultEntity?> GetByIdAsync(string resultId)
    {
        return await _db.Queryable<CrawlResultEntity>()
            .FirstAsync(r => r.ResultId == resultId);
    }

    /// <inheritdoc/>
    public async Task<List<CrawlResultEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize)
    {
        return await _db.Queryable<CrawlResultEntity>()
            .Where(r => r.TaskId == taskId)
            .OrderByDescending(r => r.CreatedAt)
            .ToPageListAsync(pageIndex, pageSize);
    }

    /// <inheritdoc/>
    public async Task<List<CrawlResultEntity>> GetByExecutionIdAsync(string executionId, int pageIndex, int pageSize)
    {
        return await _db.Queryable<CrawlResultEntity>()
            .Where(r => r.ExecutionId == executionId)
            .OrderByDescending(r => r.CreatedAt)
            .ToPageListAsync(pageIndex, pageSize);
    }

    /// <inheritdoc/>
    public async Task<long> CountByTaskIdAsync(string taskId)
    {
        return await _db.Queryable<CrawlResultEntity>()
            .Where(r => r.TaskId == taskId)
            .CountAsync();
    }

    /// <inheritdoc/>
    public async Task AddAsync(CrawlResultEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(List<CrawlResultEntity> entities)
    {
        await _db.Insertable(entities).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteByTaskIdAsync(string taskId)
    {
        await _db.Deleteable<CrawlResultEntity>()
            .Where(r => r.TaskId == taskId)
            .ExecuteCommandAsync();
    }
}
