namespace ResourceSpider.Server.Services;

/// <summary>
/// PostgreSQL 结果存储开关服务接口。
/// </summary>
public interface IPostgreSqlResultStorageFeatureService
{
    bool IsEnabled { get; }
    bool IsConfigured { get; }
    bool IsConnected { get; }
    string? LastError { get; }
    string? LastConfigWriteError { get; }
    string? EffectiveConfigFile { get; }
    void SetEnabled(bool enabled);
    void UpdateState(bool enabled, bool configured, bool connected, string? lastError = null, string? lastConfigWriteError = null, string? effectiveConfigFile = null);
}

/// <summary>
/// PostgreSQL 结果存储开关服务实现。
/// </summary>
public sealed class PostgreSqlResultStorageFeatureService : IPostgreSqlResultStorageFeatureService
{
    private volatile bool _enabled;
    private volatile bool _configured;
    private volatile bool _connected;
    private string? _lastError;
    private string? _lastConfigWriteError;
    private string? _effectiveConfigFile;

    public PostgreSqlResultStorageFeatureService(bool enabled, bool configured, bool connected)
    {
        _enabled = enabled;
        _configured = configured;
        _connected = connected;
    }

    public bool IsEnabled => _enabled;

    public bool IsConfigured => _configured;

    public bool IsConnected => _connected;

    public string? LastError => _lastError;

    public string? LastConfigWriteError => _lastConfigWriteError;

    public string? EffectiveConfigFile => _effectiveConfigFile;

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
    }

    public void UpdateState(bool enabled, bool configured, bool connected, string? lastError = null, string? lastConfigWriteError = null, string? effectiveConfigFile = null)
    {
        _enabled = enabled;
        _configured = configured;
        _connected = connected;
        _lastError = lastError;
        _lastConfigWriteError = lastConfigWriteError;
        _effectiveConfigFile = effectiveConfigFile;
    }
}
