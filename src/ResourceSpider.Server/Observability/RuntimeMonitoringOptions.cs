namespace Larpx.PersonalTools.ResourceSpider.Server.Observability;

/// <summary>
/// 运行监控实时推送配置。
/// </summary>
public class RuntimeMonitoringOptions
{
    /// <summary>
    /// 默认快照推送策略（秒）。
    /// </summary>
    public int DefaultSnapshotPushIntervalSeconds { get; set; } = 1;

    /// <summary>
    /// 允许的快照推送策略（秒）。
    /// </summary>
    public List<int> SnapshotPushIntervalsSeconds { get; set; } = [1, 2, 5];
}
