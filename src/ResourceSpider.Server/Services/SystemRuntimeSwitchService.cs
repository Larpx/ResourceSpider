using System.Text.Json.Nodes;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;
using Serilog;
using SqlSugar;
using StackExchange.Redis;

namespace ResourceSpider.Server.Services;

public interface ISystemRuntimeSwitchService
{
    Task<RedisFeatureStatusDto> UpdateRedisEnabledAsync(bool enabled, CancellationToken cancellationToken = default);
    Task<PostgreSqlResultStorageStatusDto> UpdatePostgreSqlResultStorageEnabledAsync(bool enabled, CancellationToken cancellationToken = default);
}

public sealed class SystemRuntimeSwitchService : ISystemRuntimeSwitchService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly IRedisFeatureService _redisFeatureService;
    private readonly IPostgreSqlResultStorageFeatureService _postgreFeatureService;
    private readonly IRuntimeRedisConnectionAccessor _redisConnectionAccessor;
    private readonly IRuntimePostgreSqlResultDbAccessor _postgreAccessor;
    private readonly ILogger<SystemRuntimeSwitchService> _logger;
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    public SystemRuntimeSwitchService(
        IWebHostEnvironment environment,
        IConfiguration configuration,
        IRedisFeatureService redisFeatureService,
        IPostgreSqlResultStorageFeatureService postgreFeatureService,
        IRuntimeRedisConnectionAccessor redisConnectionAccessor,
        IRuntimePostgreSqlResultDbAccessor postgreAccessor,
        ILogger<SystemRuntimeSwitchService> logger)
    {
        _environment = environment;
        _configuration = configuration;
        _redisFeatureService = redisFeatureService;
        _postgreFeatureService = postgreFeatureService;
        _redisConnectionAccessor = redisConnectionAccessor;
        _postgreAccessor = postgreAccessor;
        _logger = logger;
    }

    public async Task<RedisFeatureStatusDto> UpdateRedisEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        await _syncLock.WaitAsync(cancellationToken);
        try
        {
            var (effectiveConfigFile, configWriteError) = await UpdateJsonConfigFilesAsync("Redis", "Enabled", enabled, cancellationToken);

            var connectionString = _configuration.GetConnectionString("Redis");
            var configured = !string.IsNullOrWhiteSpace(connectionString);
            var connected = false;
            string? lastError = null;

            if (enabled && configured)
            {
                try
                {
                    var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionString!);
                    _redisConnectionAccessor.SetConnection(multiplexer);
                    connected = multiplexer.IsConnected;
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                    _logger.LogWarning(ex, "运行时启用 Redis 失败，将保持未连接状态");
                    _redisConnectionAccessor.SetConnection(null);
                }
            }
            else
            {
                _redisConnectionAccessor.SetConnection(null);
            }

            _redisFeatureService.UpdateState(enabled, configured, connected, lastError, configWriteError, effectiveConfigFile);

            return new RedisFeatureStatusDto(
                Enabled: _redisFeatureService.IsEnabled,
                Configured: _redisFeatureService.IsConfigured,
                Connected: _redisFeatureService.IsConnected,
                TaskContentTtlSeconds: _redisFeatureService.TaskContentTtlSeconds,
                Status: !_redisFeatureService.IsConfigured
                    ? "NotConfigured"
                    : !_redisFeatureService.IsEnabled
                        ? "Disabled"
                        : _redisFeatureService.IsConnected
                            ? "Connected"
                            : "Unavailable",
                LastError: _redisFeatureService.LastError,
                LastConfigWriteError: _redisFeatureService.LastConfigWriteError,
                EffectiveConfigFile: _redisFeatureService.EffectiveConfigFile);
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public async Task<PostgreSqlResultStorageStatusDto> UpdatePostgreSqlResultStorageEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        await _syncLock.WaitAsync(cancellationToken);
        try
        {
            var (effectiveConfigFile, configWriteError) = await UpdateJsonConfigFilesAsync("PostgreSqlResults", "Enabled", enabled, cancellationToken);

            var connectionString = _configuration.GetConnectionString("PostgreSqlResults");
            var configured = !string.IsNullOrWhiteSpace(connectionString);
            var connected = false;
            string? lastError = null;

            if (enabled && configured)
            {
                try
                {
                    var client = new SqlSugarClient(new ConnectionConfig
                    {
                        ConnectionString = connectionString!,
                        DbType = DbType.PostgreSQL,
                        IsAutoCloseConnection = true,
                        InitKeyType = InitKeyType.Attribute
                    });

                    client.Ado.GetInt("SELECT 1");
                    if (!client.DbMaintenance.IsAnyTable("collection_results", false))
                    {
                        client.CodeFirst.InitTables(typeof(CollectionResultEntity));
                    }

                    _postgreAccessor.SetClient(client);
                    connected = true;
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                    _logger.LogWarning(ex, "运行时启用 PostgreSQL 结果存储失败，将保持未连接状态");
                    _postgreAccessor.SetClient(null);
                }
            }
            else
            {
                _postgreAccessor.SetClient(null);
            }

            _postgreFeatureService.UpdateState(enabled, configured, connected, lastError, configWriteError, effectiveConfigFile);

            return new PostgreSqlResultStorageStatusDto(
                Enabled: _postgreFeatureService.IsEnabled,
                Configured: _postgreFeatureService.IsConfigured,
                Connected: _postgreFeatureService.IsConnected,
                Status: !_postgreFeatureService.IsConfigured
                    ? "NotConfigured"
                    : !_postgreFeatureService.IsEnabled
                        ? "Disabled"
                        : _postgreFeatureService.IsConnected
                            ? "Connected"
                            : "Unavailable",
                LastError: _postgreFeatureService.LastError,
                LastConfigWriteError: _postgreFeatureService.LastConfigWriteError,
                EffectiveConfigFile: _postgreFeatureService.EffectiveConfigFile);
        }
        finally
        {
            _syncLock.Release();
        }
    }

    private async Task<(string? EffectiveConfigFile, string? Error)> UpdateJsonConfigFilesAsync(string sectionName, string propertyName, bool value, CancellationToken cancellationToken)
    {
        var targetFiles = new List<string>
        {
            Path.Combine(_environment.ContentRootPath, "appsettings.json")
        };

        if (!string.IsNullOrWhiteSpace(_environment.EnvironmentName))
        {
            targetFiles.Add(Path.Combine(_environment.ContentRootPath, $"appsettings.{_environment.EnvironmentName}.json"));
        }

        var updatedFiles = new List<string>();
        var errors = new List<string>();

        foreach (var file in targetFiles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var rootNode = await LoadOrCreateJsonRootAsync(file, cancellationToken);
                var sectionNode = rootNode[sectionName] as JsonObject ?? new JsonObject();
                sectionNode[propertyName] = value;
                rootNode[sectionName] = sectionNode;

                var json = rootNode.ToJsonString(new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(file, json, cancellationToken);
                updatedFiles.Add(Path.GetFileName(file));
            }
            catch (Exception ex)
            {
                errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
                _logger.LogWarning(ex, "更新配置文件 {File} 失败", file);
            }
        }

        var effectiveConfigFile = updatedFiles.Count == 0 ? null : string.Join(", ", updatedFiles);
        var error = errors.Count == 0 ? null : string.Join(" | ", errors);

        if (effectiveConfigFile != null)
        {
            Log.Information("已更新配置文件 {Files} 中的 {Section}:{Property} 为 {Value}", effectiveConfigFile, sectionName, propertyName, value);
        }

        return (effectiveConfigFile, error);
    }

    private static async Task<JsonObject> LoadOrCreateJsonRootAsync(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return new JsonObject();
        }

        var jsonText = await File.ReadAllTextAsync(filePath, cancellationToken);
        return JsonNode.Parse(jsonText)?.AsObject() ?? new JsonObject();
    }
}

public interface IRuntimeRedisConnectionAccessor
{
    IConnectionMultiplexer? Connection { get; }
    void SetConnection(IConnectionMultiplexer? connection);
}

public sealed class RuntimeRedisConnectionAccessor : IRuntimeRedisConnectionAccessor
{
    private IConnectionMultiplexer? _connection;

    public IConnectionMultiplexer? Connection => _connection;

    public void SetConnection(IConnectionMultiplexer? connection)
    {
        var old = Interlocked.Exchange(ref _connection, connection);
        try
        {
            old?.Dispose();
        }
        catch
        {
        }
    }
}

public interface IRuntimePostgreSqlResultDbAccessor
{
    SqlSugarClient? Client { get; }
    void SetClient(SqlSugarClient? client);
}

public sealed class RuntimePostgreSqlResultDbAccessor : IRuntimePostgreSqlResultDbAccessor
{
    private SqlSugarClient? _client;

    public SqlSugarClient? Client => _client;

    public void SetClient(SqlSugarClient? client)
    {
        _client = client;
    }
}
