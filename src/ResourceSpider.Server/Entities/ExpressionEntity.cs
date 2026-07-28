using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Entities;

/// <summary>
/// 表达式实体，映射数据库 expressions 表
/// 定义数据提取表达式的元信息和状态，包含容器选择器和健康度统计
/// 一个表达式由多个 ExpressionFieldEntity 组成，定义完整的数据提取规则
/// </summary>
[SugarTable("expressions")]
public class ExpressionEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 表达式唯一标识符，业务主键
    /// </summary>
    [SugarColumn(Length = 64)]
    public string ExpressionId { get; set; } = string.Empty;

    /// <summary>
    /// 表达式名称，用于展示和识别表达式
    /// </summary>
    [SugarColumn(Length = 128)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 表达式描述，说明表达式的用途和适用场景
    /// </summary>
    [SugarColumn(Length = 512, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 默认选择器类型：XPath、Css、JsonPath、Regex
    /// </summary>
    [SugarColumn(Length = 32)]
    public string SelectorType { get; set; } = "XPath";

    /// <summary>
    /// 容器选择器表达式，用于定位数据列表的容器元素，字段提取在容器内进行
    /// </summary>
    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? ContainerExpression { get; set; }

    /// <summary>
    /// 表达式状态：0-禁用，1-启用
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// 累计成功执行次数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 累计失败执行次数
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// 连续失败次数，用于判断表达式是否需要自动禁用
    /// </summary>
    public int ConsecutiveFailures { get; set; }

    /// <summary>
    /// 最后一次验证时间，记录表达式有效性验证的时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? LastValidatedAt { get; set; }

    /// <summary>
    /// 最后一次使用时间，记录表达式最近一次被任务引用的时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// 表达式过期时间，过期后不再被任务调度使用
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? ExpiredAt { get; set; }

    /// <summary>
    /// 表达式创建者用户 ID
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 表达式创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 表达式信息最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
