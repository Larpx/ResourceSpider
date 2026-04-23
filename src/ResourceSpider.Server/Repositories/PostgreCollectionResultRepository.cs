using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Services;
using SqlSugar;

namespace ResourceSpider.Server.Repositories;

public interface IPostgreResultDbClient
{
    SqlSugarClient? Client { get; }
}

public sealed class PostgreResultDbClient : IPostgreResultDbClient
{
    private readonly IRuntimePostgreSqlResultDbAccessor _accessor;

    public PostgreResultDbClient(IRuntimePostgreSqlResultDbAccessor accessor)
    {
        _accessor = accessor;
    }

    public SqlSugarClient? Client => _accessor.Client;
}

public interface IPostgreCollectionResultRepository
{
    Task<CollectionResultEntity?> GetByIdAsync(string resultId);
    Task<List<CollectionResultEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize);
    Task<List<CollectionResultEntity>> GetAllByTaskIdAsync(string taskId);
    Task<List<CollectionResultEntity>> GetByExpressionIdAsync(string expressionId, int pageIndex, int pageSize);
    Task<long> CountByTaskIdAsync(string taskId);
    Task<long> CountByExpressionIdAsync(string expressionId);
    Task AddAsync(CollectionResultEntity entity);
    Task AddRangeAsync(List<CollectionResultEntity> entities);
    Task DeleteByTaskIdAsync(string taskId);
}

public class PostgreCollectionResultRepository : IPostgreCollectionResultRepository
{
    private readonly IPostgreResultDbClient _postgreResultDbClient;

    public PostgreCollectionResultRepository(IPostgreResultDbClient postgreResultDbClient)
    {
        _postgreResultDbClient = postgreResultDbClient;
    }

    private SqlSugarClient? Db => _postgreResultDbClient.Client;

    public async Task<CollectionResultEntity?> GetByIdAsync(string resultId)
    {
        if (Db == null)
        {
            return null;
        }

        return await Db.Queryable<CollectionResultEntity>()
            .FirstAsync(x => x.ResultId == resultId);
    }

    public async Task<List<CollectionResultEntity>> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize)
    {
        if (Db == null)
        {
            return [];
        }

        return await Db.Queryable<CollectionResultEntity>()
            .Where(x => x.TaskId == taskId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<CollectionResultEntity>> GetAllByTaskIdAsync(string taskId)
    {
        if (Db == null)
        {
            return [];
        }

        return await Db.Queryable<CollectionResultEntity>()
            .Where(x => x.TaskId == taskId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<CollectionResultEntity>> GetByExpressionIdAsync(string expressionId, int pageIndex, int pageSize)
    {
        if (Db == null)
        {
            return [];
        }

        return await Db.Queryable<CollectionResultEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountByTaskIdAsync(string taskId)
    {
        if (Db == null)
        {
            return 0;
        }

        return await Db.Queryable<CollectionResultEntity>()
            .Where(x => x.TaskId == taskId)
            .CountAsync();
    }

    public async Task<long> CountByExpressionIdAsync(string expressionId)
    {
        if (Db == null)
        {
            return 0;
        }

        return await Db.Queryable<CollectionResultEntity>()
            .Where(x => x.ExpressionId == expressionId)
            .CountAsync();
    }

    public async Task AddAsync(CollectionResultEntity entity)
    {
        if (Db == null)
        {
            return;
        }

        await Db.Insertable(entity).ExecuteCommandAsync();
    }

    public async Task AddRangeAsync(List<CollectionResultEntity> entities)
    {
        if (Db == null || entities.Count == 0)
        {
            return;
        }

        await Db.Insertable(entities).ExecuteCommandAsync();
    }

    public async Task DeleteByTaskIdAsync(string taskId)
    {
        if (Db == null)
        {
            return;
        }

        await Db.Deleteable<CollectionResultEntity>()
            .Where(x => x.TaskId == taskId)
            .ExecuteCommandAsync();
    }
}
