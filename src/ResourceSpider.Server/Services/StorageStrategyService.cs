using ResourceSpider.Core.Enums;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface IStorageStrategyService
{
    Task StoreResultAsync(CollectionResultEntity entity);

    Task StoreResultsAsync(List<CollectionResultEntity> entities);

    string GetCurrentEngineName();
}

public class StorageStrategyService : IStorageStrategyService
{
    private const string MySqlEngineName = "MySQL";
    private const string PostgreSqlEngineName = "PostgreSQL";

    private readonly ICollectionResultRepository _mysqlResultRepository;
    private readonly IPostgreCollectionResultRepository _postgreResultRepository;
    private readonly IPostgreSqlResultStorageFeatureService _postgreFeatureService;
    private readonly ILogger<StorageStrategyService> _logger;

    public StorageStrategyService(
        ICollectionResultRepository mysqlResultRepository,
        IPostgreCollectionResultRepository postgreResultRepository,
        IPostgreSqlResultStorageFeatureService postgreFeatureService,
        ILogger<StorageStrategyService> logger)
    {
        _mysqlResultRepository = mysqlResultRepository;
        _postgreResultRepository = postgreResultRepository;
        _postgreFeatureService = postgreFeatureService;
        _logger = logger;
    }

    public string GetCurrentEngineName()
    {
        return _postgreFeatureService.IsEnabled
               && _postgreFeatureService.IsConfigured
               && _postgreFeatureService.IsConnected
            ? PostgreSqlEngineName
            : MySqlEngineName;
    }

    public async Task StoreResultAsync(CollectionResultEntity entity)
    {
        var engine = GetCurrentEngineName();
        entity.StorageEngine = engine;

        if (engine == PostgreSqlEngineName)
        {
            await _postgreResultRepository.AddAsync(entity);
        }
        else
        {
            await _mysqlResultRepository.AddAsync(entity);
        }

        _logger.LogDebug("存储采集结果到 {Engine}，结果 ID: {ResultId}", engine, entity.ResultId);
    }

    public async Task StoreResultsAsync(List<CollectionResultEntity> entities)
    {
        if (entities.Count == 0)
        {
            return;
        }

        var engine = GetCurrentEngineName();
        foreach (var entity in entities)
        {
            entity.StorageEngine = engine;
        }

        if (engine == PostgreSqlEngineName)
        {
            await _postgreResultRepository.AddRangeAsync(entities);
        }
        else
        {
            await _mysqlResultRepository.AddRangeAsync(entities);
        }

        _logger.LogInformation("批量存储 {Count} 条采集结果到 {Engine}", entities.Count, engine);
    }
}
