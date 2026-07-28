using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Entities;

/// <summary>
/// 统计实体，映射数据库 statistics 表
/// 按日期和代理节点维度记录请求统计数据，用于性能监控和趋势分析
/// </summary>
[SugarTable("statistics")]
public class StatisticEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 关联的代理节点 ID，标识该统计数据属于哪个代理节点
    /// </summary>
    [SugarColumn(Length = 64)]
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// 统计日期，按天聚合统计数据
    /// </summary>
    public DateTime StatDate { get; set; }

    /// <summary>
    /// 总请求数，当天该代理节点发起的请求总量
    /// </summary>
    public int TotalRequests { get; set; }

    /// <summary>
    /// 成功请求数，HTTP 状态码为 2xx 的请求量
    /// </summary>
    public int SuccessRequests { get; set; }

    /// <summary>
    /// 失败请求数，HTTP 状态码非 2xx 或发生异常的请求量
    /// </summary>
    public int FailedRequests { get; set; }

    /// <summary>
    /// 平均请求耗时（毫秒），反映代理节点的响应速度
    /// </summary>
    public decimal? AvgDuration { get; set; }

    /// <summary>
    /// 数据采集量（字节），当天采集的数据总量
    /// </summary>
    public long DataVolume { get; set; }

    /// <summary>
    /// 统计记录创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
