using SqlSugar;

namespace ResourceSpider.Server.Entities;

/// <summary>
/// 代理节点实体，映射数据库 agents 表
/// 表示一个可执行爬虫任务的代理节点，包含节点连接信息、运行状态和资源使用情况
/// </summary>
[SugarTable("agents")]
public class AgentEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 代理节点唯一标识符，业务主键
    /// </summary>
    [SugarColumn(Length = 64)]
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// 代理节点名称，用于展示和识别节点
    /// </summary>
    [SugarColumn(Length = 128)]
    public string AgentName { get; set; } = string.Empty;

    /// <summary>
    /// 代理节点认证令牌，用于节点与服务器之间的身份验证
    /// </summary>
    [SugarColumn(Length = 256)]
    public string AgentToken { get; set; } = string.Empty;

    /// <summary>
    /// 代理节点 IP 地址
    /// </summary>
    [SugarColumn(Length = 45)]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 代理节点通信端口
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 代理节点能力 JSON，描述节点支持的爬虫类型、浏览器引擎等功能特性
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? Capabilities { get; set; }

    /// <summary>
    /// 代理节点状态：0-离线，1-在线，2-忙碌，3-异常
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// CPU 使用率百分比
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public decimal? CpuUsage { get; set; }

    /// <summary>
    /// 内存使用率百分比
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public decimal? MemoryUsage { get; set; }

    /// <summary>
    /// 当前正在执行的任务数量
    /// </summary>
    public int TaskCount { get; set; }

    /// <summary>
    /// 最后一次心跳时间，用于判断节点是否在线
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? LastHeartbeat { get; set; }

    /// <summary>
    /// 代理节点标签 JSON 数组，用于节点分类和筛选
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? Tags { get; set; }

    /// <summary>
    /// 代理节点所属分组 ID
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? GroupId { get; set; }

    /// <summary>
    /// 代理节点操作系统信息
    /// </summary>
    [SugarColumn(Length = 100, IsNullable = true)]
    public string? OS { get; set; }

    /// <summary>
    /// 代理节点程序版本号
    /// </summary>
    [SugarColumn(Length = 50, IsNullable = true)]
    public string? Version { get; set; }

    /// <summary>
    /// 代理节点配置 JSON，包含并发数、超时等运行时配置
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? Config { get; set; }

    /// <summary>
    /// 代理节点注册时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 代理节点信息最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
