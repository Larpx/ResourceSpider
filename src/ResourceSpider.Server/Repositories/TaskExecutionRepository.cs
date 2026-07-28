using SqlSugar;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;

namespace Larpx.PersonalTools.ResourceSpider.Server.Repositories;

/// <summary>
/// 任务执行记录数据仓储接口，定义任务执行记录的数据访问操作
/// </summary>
public interface ITaskExecutionRepository
{
    /// <summary>
    /// 根据执行记录 ID 获取任务执行实体
    /// </summary>
    /// <param name="executionId">执行记录唯一标识符</param>
    /// <returns>任务执行实体，未找到时返回 null</returns>
    Task<TaskExecutionEntity?> GetByIdAsync(string executionId);

    /// <summary>
    /// 根据任务 ID 分页获取执行记录列表
    /// </summary>
    /// <param name="taskId">任务唯一标识符</param>
    /// <param name="pageIndex">页码，从 1 开始</param>
    /// <param name="pageSize">每页记录数</param>
    /// <returns>执行记录列表，按创建时间倒序排列</returns>
    Task<List<TaskExecutionEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize);

    /// <summary>
    /// 统计指定任务的执行记录数量
    /// </summary>
    /// <param name="taskId">任务唯一标识符</param>
    /// <returns>执行记录总数</returns>
    Task<long> CountByTaskIdAsync(string taskId);

    /// <summary>
    /// 新增任务执行记录
    /// </summary>
    /// <param name="entity">任务执行实体</param>
    Task AddAsync(TaskExecutionEntity entity);

    /// <summary>
    /// 更新任务执行记录
    /// </summary>
    /// <param name="entity">任务执行实体</param>
    Task UpdateAsync(TaskExecutionEntity entity);
}

/// <summary>
/// 任务执行记录数据仓储实现，基于 SQLSugar 提供任务执行记录的增删改查操作
/// </summary>
public class TaskExecutionRepository : ITaskExecutionRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 初始化任务执行记录仓储实例
    /// </summary>
    /// <param name="db">SQLSugar 数据库客户端</param>
    public TaskExecutionRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<TaskExecutionEntity?> GetByIdAsync(string executionId)
    {
        return await _db.Queryable<TaskExecutionEntity>()
            .FirstAsync(e => e.ExecutionId == executionId);
    }

    /// <inheritdoc/>
    public async Task<List<TaskExecutionEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize)
    {
        return await _db.Queryable<TaskExecutionEntity>()
            .Where(e => e.TaskId == taskId)
            .OrderByDescending(e => e.CreatedAt)
            .ToPageListAsync(pageIndex, pageSize);
    }

    /// <inheritdoc/>
    public async Task<long> CountByTaskIdAsync(string taskId)
    {
        return await _db.Queryable<TaskExecutionEntity>()
            .Where(e => e.TaskId == taskId)
            .CountAsync();
    }

    /// <inheritdoc/>
    public async Task AddAsync(TaskExecutionEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(TaskExecutionEntity entity)
    {
        await _db.Updateable(entity).ExecuteCommandAsync();
    }
}
