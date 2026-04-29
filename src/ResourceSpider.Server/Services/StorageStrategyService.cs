using ResourceSpider.Core.Enums;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;
using SqlSugar;

namespace ResourceSpider.Server.Services;

public interface IStorageStrategyService
{
    Task StoreResultAsync(CollectionResultEntity entity);

    Task StoreResultsAsync(List<CollectionResultEntity> entities);

    StorageEngine GetCurrentEngine();

    Task<bool> SwitchEngineAsync(string newEngine);

    Task<bool> TestConnectionAsync(string engine, string connectionString);
}

public class StorageStrategyService : IStorageStrategyService
{
    private readonly ICollectionResultRepository _mysqlResultRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StorageStrategyService> _logger;
    private StorageEngine _currentEngine;
    private readonly object _engineLock = new();
    private ISqlSugarClient? _postgreSqlClient;

    public StorageStrategyService(
        ICollectionResultRepository mysqlResultRepository,
        IConfiguration configuration,
        ILogger<StorageStrategyService> logger)
    {
        _mysqlResultRepository = mysqlResultRepository;
        _configuration = configuration;
        _logger = logger;

        var engineStr = configuration["Storage:Engine"] ?? "MySQL";
        _currentEngine = engineStr.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
            ? StorageEngine.PostgreSQL
            : StorageEngine.MySQL;
    }

    public StorageEngine GetCurrentEngine()
    {
        lock (_engineLock) return _currentEngine;
    }

    public async Task StoreResultAsync(CollectionResultEntity entity)
    {
        var engine = GetCurrentEngine();
        entity.StorageEngine = engine.ToString();

        if (engine == StorageEngine.PostgreSQL)
        {
            await StoreToPostgreSqlAsync(entity);
        }
        else
        {
            await _mysqlResultRepository.AddAsync(entity);
        }

        _logger.LogDebug("存储采集结果到 {Engine}，结果 ID: {ResultId}", engine, entity.ResultId);
    }

    public async Task StoreResultsAsync(List<CollectionResultEntity> entities)
    {
        var engine = GetCurrentEngine();

        foreach (var entity in entities)
        {
            entity.StorageEngine = engine.ToString();
        }

        if (engine == StorageEngine.PostgreSQL)
        {
            await StoreBatchToPostgreSqlAsync(entities);
        }
        else
        {
            await _mysqlResultRepository.AddRangeAsync(entities);
        }

        _logger.LogInformation("批量存储 {Count} 条采集结果到 {Engine}", entities.Count, engine);
    }

    public async Task<bool> SwitchEngineAsync(string newEngine)
    {
        var targetEngine = newEngine.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
            ? StorageEngine.PostgreSQL
            : StorageEngine.MySQL;

        var connectionString = targetEngine == StorageEngine.PostgreSQL
            ? _configuration.GetConnectionString("PostgreSQL")
            : _configuration.GetConnectionString("Default");

        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogError("切换存储引擎失败：{Engine} 连接字符串未配置", targetEngine);
            return false;
        }

        var testResult = await TestConnectionAsync(newEngine, connectionString);
        if (!testResult)
        {
            _logger.LogError("切换存储引擎失败：{Engine} 连接测试不通过", targetEngine);
            return false;
        }

        lock (_engineLock)
        {
            _currentEngine = targetEngine;
        }

        _logger.LogWarning("存储引擎已切换为 {Engine}", targetEngine);
        return true;
    }

    public async Task<bool> TestConnectionAsync(string engine, string connectionString)
    {
        try
        {
            var dbType = engine.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
                ? DbType.PostgreSQL
                : DbType.MySql;

            var client = new SqlSugarScope(new ConnectionConfig
            {
                DbType = dbType,
                ConnectionString = connectionString,
                IsAutoCloseConnection = true
            });

            var result = await client.Ado.GetIntAsync("SELECT 1");
            return result == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "测试 {Engine} 连接失败", engine);
            return false;
        }
    }

    private ISqlSugarClient GetPostgreSqlClient()
    {
        if (_postgreSqlClient != null) return _postgreSqlClient;

        var connectionString = _configuration.GetConnectionString("PostgreSQL");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("PostgreSQL 连接字符串未配置");
        }

        _postgreSqlClient = new SqlSugarScope(new ConnectionConfig
        {
            DbType = DbType.PostgreSQL,
            ConnectionString = connectionString,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });

        return _postgreSqlClient;
    }

    private async Task StoreToPostgreSqlAsync(CollectionResultEntity entity)
    {
        var client = GetPostgreSqlClient();
        await client.Insertable(entity).ExecuteCommandAsync();
    }

    private async Task StoreBatchToPostgreSqlAsync(List<CollectionResultEntity> entities)
    {
        var client = GetPostgreSqlClient();
        await client.Insertable(entities).ExecuteCommandAsync();
    }
}
