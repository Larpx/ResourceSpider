using SqlSugar;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;

namespace Larpx.PersonalTools.ResourceSpider.Server.Repositories;

/// <summary>
/// 任务步骤数据仓储接口，定义任务步骤实体的数据访问操作
/// 支持按任务查询步骤列表、批量写入和删除
/// </summary>
public interface ITaskStepRepository
{
    /// <summary>
    /// 根据任务 ID 获取步骤列表，按执行顺序排列
    /// </summary>
    /// <param name="taskId">任务唯一标识符</param>
    /// <returns>任务步骤列表</returns>
    Task<List<TaskStepEntity>> GetByTaskIdAsync(string taskId);

    /// <summary>
    /// 根据步骤 ID 获取任务步骤实体
    /// </summary>
    /// <param name="stepId">步骤唯一标识符</param>
    /// <returns>任务步骤实体，未找到时返回 null</returns>
    Task<TaskStepEntity?> GetByIdAsync(string stepId);

    /// <summary>
    /// 新增单条任务步骤
    /// </summary>
    /// <param name="entity">任务步骤实体</param>
    Task AddAsync(TaskStepEntity entity);

    /// <summary>
    /// 批量新增任务步骤
    /// </summary>
    /// <param name="entities">任务步骤实体列表</param>
    Task AddRangeAsync(List<TaskStepEntity> entities);

    /// <summary>
    /// 更新任务步骤，忽略 CreatedAt 字段
    /// </summary>
    /// <param name="entity">任务步骤实体</param>
    Task UpdateAsync(TaskStepEntity entity);

    /// <summary>
    /// 根据任务 ID 删除所有步骤
    /// </summary>
    /// <param name="taskId">任务唯一标识符</param>
    Task DeleteByTaskIdAsync(string taskId);

    /// <summary>
    /// 根据步骤 ID 删除单个步骤
    /// </summary>
    /// <param name="stepId">步骤唯一标识符</param>
    Task DeleteAsync(string stepId);
}

/// <summary>
/// 任务步骤数据仓储实现，基于 SQLSugar 提供任务步骤的增删改查操作
/// </summary>
public class TaskStepRepository : ITaskStepRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 初始化任务步骤仓储实例
    /// </summary>
    /// <param name="db">SQLSugar 数据库客户端</param>
    public TaskStepRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<List<TaskStepEntity>> GetByTaskIdAsync(string taskId)
    {
        return await _db.Queryable<TaskStepEntity>()
            .Where(s => s.TaskId == taskId)
            .OrderBy(s => s.StepOrder)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<TaskStepEntity?> GetByIdAsync(string stepId)
    {
        return await _db.Queryable<TaskStepEntity>()
            .FirstAsync(s => s.StepId == stepId);
    }

    /// <inheritdoc/>
    public async Task AddAsync(TaskStepEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(List<TaskStepEntity> entities)
    {
        await _db.Insertable(entities).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(TaskStepEntity entity)
    {
        await _db.Updateable(entity)
            .IgnoreColumns(s => s.CreatedAt)
            .ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteByTaskIdAsync(string taskId)
    {
        await _db.Deleteable<TaskStepEntity>()
            .Where(s => s.TaskId == taskId)
            .ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string stepId)
    {
        await _db.Deleteable<TaskStepEntity>()
            .Where(s => s.StepId == stepId)
            .ExecuteCommandAsync();
    }
}
