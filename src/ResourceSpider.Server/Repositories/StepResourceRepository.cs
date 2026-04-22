using ResourceSpider.Server.Entities;
using SqlSugar;

namespace ResourceSpider.Server.Repositories;

public interface IStepResourceRepository
{
    Task AddRangeAsync(List<StepResourceEntity> entities);
    Task<List<StepResourceEntity>> GetAvailableByStepIdsAsync(string taskId, List<string> stepIds, int take);
    Task MarkConsumedAsync(List<string> resourceIds);
    Task<int> CountAvailableByStepIdAsync(string taskId, string stepId);
    Task DeleteByTaskIdAsync(string taskId);
}

public class StepResourceRepository : IStepResourceRepository
{
    private readonly ISqlSugarClient _db;

    public StepResourceRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task AddRangeAsync(List<StepResourceEntity> entities)
    {
        if (entities.Count == 0) return;
        await _db.Insertable(entities).ExecuteCommandAsync();
    }

    public async Task<List<StepResourceEntity>> GetAvailableByStepIdsAsync(string taskId, List<string> stepIds, int take)
    {
        if (stepIds.Count == 0 || take <= 0)
        {
            return [];
        }

        return await _db.Queryable<StepResourceEntity>()
            .Where(x => x.TaskId == taskId && x.Status == 0 && stepIds.Contains(x.StepId))
            .OrderBy(x => x.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task MarkConsumedAsync(List<string> resourceIds)
    {
        if (resourceIds.Count == 0) return;
        await _db.Updateable<StepResourceEntity>()
            .SetColumns(x => x.Status == 1)
            .Where(x => resourceIds.Contains(x.ResourceId))
            .ExecuteCommandAsync();
    }

    public async Task<int> CountAvailableByStepIdAsync(string taskId, string stepId)
    {
        return (int)await _db.Queryable<StepResourceEntity>()
            .Where(x => x.TaskId == taskId && x.StepId == stepId && x.Status == 0)
            .CountAsync();
    }

    public async Task DeleteByTaskIdAsync(string taskId)
    {
        await _db.Deleteable<StepResourceEntity>()
            .Where(x => x.TaskId == taskId)
            .ExecuteCommandAsync();
    }
}
