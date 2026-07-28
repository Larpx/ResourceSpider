using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Repositories;

/// <summary>
/// 任务数据仓储接口，定义任务实体的数据访问操作
/// 支持任务的增删改查、分页查询和待执行任务获取
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// 根据任务 ID 获取任务实体
    /// </summary>
    /// <param name="taskId">任务唯一标识符</param>
    /// <returns>任务实体，未找到时返回 null</returns>
    Task<TaskEntity?> GetByIdAsync(string taskId);

    /// <summary>
    /// 分页获取任务列表，支持按状态筛选，按优先级和创建时间倒序排列
    /// </summary>
    /// <param name="pageIndex">页码，从 1 开始</param>
    /// <param name="pageSize">每页记录数</param>
    /// <param name="status">状态筛选，为 null 时不筛选</param>
    /// <param name="keyword">关键字筛选，为 null 时不筛选</param>
    /// <returns>任务列表</returns>
    Task<List<TaskEntity>> GetAllAsync(int pageIndex, int pageSize, int? status = null, string? keyword = null);

    /// <summary>
    /// 统计任务数量，支持按状态筛选
    /// </summary>
    /// <param name="status">状态筛选，为 null 时不筛选</param>
    /// <param name="keyword">关键字筛选，为 null 时不筛选</param>
    /// <returns>任务总数</returns>
    Task<long> CountAsync(int? status = null, string? keyword = null);

    /// <summary>
    /// 新增任务实体
    /// </summary>
    /// <param name="entity">任务实体</param>
    Task AddAsync(TaskEntity entity);

    /// <summary>
    /// 更新任务实体，忽略 CreatedAt 字段
    /// </summary>
    /// <param name="entity">任务实体</param>
    Task UpdateAsync(TaskEntity entity);

    /// <summary>
    /// 根据任务 ID 删除任务
    /// </summary>
    /// <param name="taskId">任务唯一标识符</param>
    Task DeleteAsync(string taskId);

    /// <summary>
    /// 获取待执行的任务列表，按优先级倒序排列
    /// 包括状态为待执行（0）和执行中（1）的任务
    /// </summary>
    /// <param name="count">获取数量上限</param>
    /// <returns>待执行任务列表</returns>
    Task<List<TaskEntity>> GetPendingTasksAsync(int count);
}

/// <summary>
/// 任务数据仓储实现，基于 SQLSugar 提供任务的增删改查操作
/// </summary>
public class TaskRepository : ITaskRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly SqlSugarClient _db;

    /// <summary>
    /// 初始化任务仓储实例
    /// </summary>
    /// <param name="sqlSugarClient">SQLSugar 数据库客户端</param>
    public TaskRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    /// <inheritdoc/>
    public async Task<TaskEntity?> GetByIdAsync(string taskId)
    {
        return await _db.Queryable<TaskEntity>()
            .FirstAsync(x => x.TaskId == taskId);
    }

    /// <inheritdoc/>
    public async Task<List<TaskEntity>> GetAllAsync(int pageIndex, int pageSize, int? status = null, string? keyword = null)
    {
        var query = _db.Queryable<TaskEntity>();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.TaskName.Contains(keyword) || x.TaskType.Contains(keyword) || x.TaskId.Contains(keyword));
        }

        return await query
            .OrderByDescending(x => x.Priority)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<long> CountAsync(int? status = null, string? keyword = null)
    {
        var query = _db.Queryable<TaskEntity>();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.TaskName.Contains(keyword) || x.TaskType.Contains(keyword) || x.TaskId.Contains(keyword));
        }

        return await query.CountAsync();
    }

    /// <inheritdoc/>
    public async Task AddAsync(TaskEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(TaskEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.Updateable(entity)
            .IgnoreColumns(x => x.CreatedAt)
            .ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string taskId)
    {
        await _db.Deleteable<TaskEntity>()
            .Where(x => x.TaskId == taskId)
            .ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task<List<TaskEntity>> GetPendingTasksAsync(int count)
    {
        return await _db.Queryable<TaskEntity>()
            .Where(x => x.Status == 0 || x.Status == 1)
            .OrderByDescending(x => x.Priority)
            .Take(count)
            .ToListAsync();
    }
}
