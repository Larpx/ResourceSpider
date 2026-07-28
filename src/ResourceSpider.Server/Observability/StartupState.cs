namespace Larpx.PersonalTools.ResourceSpider.Server.Observability;

/// <summary>
/// 启动阶段状态快照，用于健康检查和运维排障
/// </summary>
public class StartupState
{
    public bool DatabaseInitializationSucceeded { get; private set; } = true;

    public string? DatabaseInitializationError { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;

    public void MarkDatabaseReady()
    {
        DatabaseInitializationSucceeded = true;
        DatabaseInitializationError = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkDatabaseFailed(Exception exception)
    {
        DatabaseInitializationSucceeded = false;
        DatabaseInitializationError = exception.Message;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
