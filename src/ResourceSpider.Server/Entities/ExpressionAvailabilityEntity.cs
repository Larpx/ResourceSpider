using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Entities;

/// <summary>
/// 表达式可用性实体，映射数据库 expression_availability 表
/// 记录表达式在不同代理节点上的可用性状态，用于表达式调度和故障检测
/// </summary>
[SugarTable("expression_availability")]
public class ExpressionAvailabilityEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 关联的表达式 ID，标识该可用性记录属于哪个表达式
    /// </summary>
    [SugarColumn(Length = 64)]
    public string ExpressionId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的代理节点 ID，标识该可用性记录对应的代理节点
    /// </summary>
    [SugarColumn(Length = 64)]
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// 表达式是否可用，true 表示可用，false 表示不可用
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// 不可用原因描述，如页面结构变更、选择器失效等
    /// </summary>
    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? FailureReason { get; set; }

    /// <summary>
    /// 最后一次检查时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? LastCheckedAt { get; set; }

    /// <summary>
    /// 最后一次成功执行时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? LastSuccessAt { get; set; }

    /// <summary>
    /// 最后一次失败执行时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? LastFailureAt { get; set; }

    /// <summary>
    /// 连续失败次数，用于判断表达式是否需要标记为不可用
    /// </summary>
    public int ConsecutiveFailures { get; set; }

    /// <summary>
    /// 可用性记录创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 可用性记录最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
