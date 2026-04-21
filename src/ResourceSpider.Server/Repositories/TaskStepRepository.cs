using SqlSugar;
using ResourceSpider.Server.Entities;

namespace ResourceSpider.Server.Repositories;

public interface ITaskStepRepository
{
    Task<List<TaskStepEntity>> GetByTaskIdAsync(string taskId);
    Task<TaskStepEntity?> GetByIdAsync(string stepId);
    Task AddAsync(TaskStepEntity entity);
    Task AddRangeAsync(List<TaskStepEntity> entities);
    Task UpdateAsync(TaskStepEntity entity);
    Task DeleteByTaskIdAsync(string taskId);
    Task DeleteAsync(string stepId);
}

public class TaskStepRepository : ITaskStepRepository
{
    private readonly ISqlSugarClient _db;

    public TaskStepRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<List<TaskStepEntity>> GetByTaskIdAsync(string taskId)
    {
        return await _db.Queryable<TaskStepEntity>()
            .Where(s => s.TaskId == taskId)
            .OrderBy(s => s.StepOrder)
            .ToListAsync();
    }

    public async Task<TaskStepEntity?> GetByIdAsync(string stepId)
    {
        return await _db.Queryable<TaskStepEntity>()
            .FirstAsync(s => s.StepId == stepId);
    }

    public async Task AddAsync(TaskStepEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    public async Task AddRangeAsync(List<TaskStepEntity> entities)
    {
        await _db.Insertable(entities).ExecuteCommandAsync();
    }

    public async Task UpdateAsync(TaskStepEntity entity)
    {
        await _db.Updateable(entity)
            .IgnoreColumns(s => s.CreatedAt)
            .ExecuteCommandAsync();
    }

    public async Task DeleteByTaskIdAsync(string taskId)
    {
        await _db.Deleteable<TaskStepEntity>()
            .Where(s => s.TaskId == taskId)
            .ExecuteCommandAsync();
    }

    public async Task DeleteAsync(string stepId)
    {
        await _db.Deleteable<TaskStepEntity>()
            .Where(s => s.StepId == stepId)
            .ExecuteCommandAsync();
    }
}
