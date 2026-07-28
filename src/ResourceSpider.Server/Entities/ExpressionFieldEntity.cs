using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Entities;

/// <summary>
/// 表达式字段实体，映射数据库 expression_fields 表
/// 定义表达式中每个字段的提取规则，包括选择器类型、表达式、格式化器等
/// 一个表达式可包含多个字段，按 Order 排序
/// </summary>
[SugarTable("expression_fields")]
public class ExpressionFieldEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 字段唯一标识符，业务主键
    /// </summary>
    [SugarColumn(Length = 64)]
    public string FieldId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的表达式 ID，标识该字段属于哪个表达式
    /// </summary>
    [SugarColumn(Length = 64)]
    public string ExpressionId { get; set; } = string.Empty;

    /// <summary>
    /// 字段名称，用于标识和引用采集结果中的字段
    /// </summary>
    [SugarColumn(Length = 128)]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// 选择器类型：XPath、Css、JsonPath、Regex
    /// </summary>
    [SugarColumn(Length = 32)]
    public string SelectorType { get; set; } = "XPath";

    /// <summary>
    /// 提取表达式，根据 SelectorType 使用对应的语法
    /// </summary>
    [SugarColumn(Length = 1024)]
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// 要提取的 HTML 元素属性名，为空时提取元素的文本内容
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = true)]
    public string? AttributeName { get; set; }

    /// <summary>
    /// 字段是否必填，必填字段提取失败时标记整条记录为异常
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// 字段默认值，当提取结果为空时使用此值
    /// </summary>
    [SugarColumn(Length = 256, IsNullable = true)]
    public string? DefaultValue { get; set; }

    /// <summary>
    /// 格式化器名称，如 Trim、Regex、Replace 等，对提取结果进行后处理
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? Formatter { get; set; }

    /// <summary>
    /// 格式化器参数 JSON，传递给格式化器的配置参数
    /// </summary>
    [SugarColumn(Length = 512, IsNullable = true)]
    public string? FormatterArgs { get; set; }

    /// <summary>
    /// 字段排序序号，数值越小越靠前
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 字段创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 字段信息最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
