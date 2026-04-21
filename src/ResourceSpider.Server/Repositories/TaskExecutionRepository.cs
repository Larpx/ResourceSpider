using SqlSugar;
using ResourceSpider.Server.Entities;

namespace ResourceSpider.Server.Repositories;

public interface ITaskExecutionRepository
{
    Task<TaskExecutionEntity?> GetByIdAsync(string executionId);
    Task<List<TaskExecutionEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize);
    Task<long> CountByTaskIdAsync(string taskId);
    Task AddAsync(TaskExecutionEntity entity);
    Task UpdateAsync(TaskExecutionEntity entity);
}

public class TaskExecutionRepository : ITaskExecutionRepository
{
    private readonly ISqlSugarClient _db;

    public TaskExecutionRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<TaskExecutionEntity?> GetByIdAsync(string executionId)
    {
        return await _db.Queryable<TaskExecutionEntity>()
            .FirstAsync(e => e.ExecutionId == executionId);
    }

    public async Task<List<TaskExecutionEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize)
    {
        return await _db.Queryable<TaskExecutionEntity>()
            .Where(e => e.TaskId == taskId)
            .OrderByDescending(e => e.CreatedAt)
            .ToPageListAsync(pageIndex, pageSize);
    }

    public async Task<long> CountByTaskIdAsync(string taskId)
    {
        return await _db.Queryable<TaskExecutionEntity>()
            .Where(e => e.TaskId == taskId)
            .CountAsync();
    }

    public async Task AddAsync(TaskExecutionEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    public async Task UpdateAsync(TaskExecutionEntity entity)
    {
        await _db.Updateable(entity).ExecuteCommandAsync();
    }
}
