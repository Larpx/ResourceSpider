using ResourceSpider.Server.Entities;
using SqlSugar;

namespace ResourceSpider.Server.Repositories;

public interface ITaskRepository
{
    Task<TaskEntity?> GetByIdAsync(string taskId);
    Task<List<TaskEntity>> GetAllAsync(int pageIndex, int pageSize, int? status = null);
    Task<long> CountAsync(int? status = null);
    Task AddAsync(TaskEntity entity);
    Task UpdateAsync(TaskEntity entity);
    Task DeleteAsync(string taskId);
    Task<List<TaskEntity>> GetPendingTasksAsync(int count);
}

public class TaskRepository : ITaskRepository
{
    private readonly SqlSugarClient _db;

    public TaskRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    public async Task<TaskEntity?> GetByIdAsync(string taskId)
    {
        return await _db.Queryable<TaskEntity>()
            .FirstAsync(x => x.TaskId == taskId);
    }

    public async Task<List<TaskEntity>> GetAllAsync(int pageIndex, int pageSize, int? status = null)
    {
        var query = _db.Queryable<TaskEntity>();
        
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return await query
            .OrderByDescending(x => x.Priority)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountAsync(int? status = null)
    {
        var query = _db.Queryable<TaskEntity>();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }
        return await query.CountAsync();
    }

    public async Task AddAsync(TaskEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    public async Task UpdateAsync(TaskEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.Updateable(entity)
            .IgnoreColumns(x => x.CreatedAt)
            .ExecuteCommandAsync();
    }

    public async Task DeleteAsync(string taskId)
    {
        await _db.Deleteable<TaskEntity>()
            .Where(x => x.TaskId == taskId)
            .ExecuteCommandAsync();
    }

    public async Task<List<TaskEntity>> GetPendingTasksAsync(int count)
    {
        return await _db.Queryable<TaskEntity>()
            .Where(x => x.Status == 0 || x.Status == 1)
            .OrderByDescending(x => x.Priority)
            .Take(count)
            .ToListAsync();
    }
}
