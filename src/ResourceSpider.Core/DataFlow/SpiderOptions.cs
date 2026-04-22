namespace ResourceSpider.Core.DataFlow;

/// <summary>
/// 爬虫配置选项，控制爬虫的运行行为和调度策略
/// </summary>
public class SpiderOptions
{
    /// <summary>
    /// 请求队列容量，控制调度器中待处理请求的最大数量
    /// </summary>
    public int RequestedQueueCount { get; set; } = 1000;

    /// <summary>
    /// 爬取深度，0 表示不限制深度
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// 请求失败时的最大重试次数
    /// </summary>
    public int RetriedTimes { get; set; } = 3;

    /// <summary>
    /// 队列为空时的休眠时间（秒）
    /// </summary>
    public int EmptySleepTime { get; set; } = 60;

    /// <summary>
    /// 爬取速度倍率，1.0 为正常速度
    /// </summary>
    public double Speed { get; set; } = 1;

    /// <summary>
    /// 每批处理的请求数量
    /// </summary>
    public uint Batch { get; set; } = 4;

    /// <summary>
    /// 是否移除站外链接，仅保留同一域名下的请求
    /// </summary>
    public bool RemoveOutboundLinks { get; set; }

    /// <summary>
    /// 存储类型标识，决定数据持久化的方式
    /// </summary>
    public string StorageType { get; set; } = string.Empty;

    /// <summary>
    /// 代理池刷新间隔（秒）
    /// </summary>
    public int RefreshProxy { get; set; } = 30;
}
