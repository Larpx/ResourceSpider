using ResourceSpider.Core.Enums;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface IStorageStrategyService
{
    Task StoreResultAsync(CollectionResultEntity entity);

    Task StoreResultsAsync(List<CollectionResultEntity> entities);

    StorageEngine GetCurrentEngine();
}

public class StorageStrategyService : IStorageStrategyService
{
    private readonly ICollectionResultRepository _mysqlResultRepository;
    private readonly ILogger<StorageStrategyService> _logger;
    private readonly StorageEngine _currentEngine;

    public StorageStrategyService(
        ICollectionResultRepository mysqlResultRepository,
        IConfiguration configuration,
        ILogger<StorageStrategyService> logger)
    {
        _mysqlResultRepository = mysqlResultRepository;
        _logger = logger;

        var engineStr = configuration["Storage:Engine"] ?? "MySQL";
        _currentEngine = engineStr.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
            ? StorageEngine.PostgreSQL
            : StorageEngine.MySQL;
    }

    public StorageEngine GetCurrentEngine() => _currentEngine;

    public async Task StoreResultAsync(CollectionResultEntity entity)
    {
        entity.StorageEngine = _currentEngine.ToString();
        await _mysqlResultRepository.AddAsync(entity);
        _logger.LogDebug("存储采集结果到 {Engine}，结果 ID: {ResultId}", _currentEngine, entity.ResultId);
    }

    public async Task StoreResultsAsync(List<CollectionResultEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.StorageEngine = _currentEngine.ToString();
        }

        await _mysqlResultRepository.AddRangeAsync(entities);
        _logger.LogInformation("批量存储 {Count} 条采集结果到 {Engine}", entities.Count, _currentEngine);
    }
}
