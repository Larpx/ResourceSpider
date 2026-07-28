using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Entities;

/// <summary>
/// 采集结果实体，映射数据库 collection_results 表
/// 存储通过表达式采集的结构化数据，每条记录对应一次字段级别的数据采集结果
/// </summary>
[SugarTable("collection_results")]
public class CollectionResultEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 结果唯一标识符，业务主键
    /// </summary>
    [SugarColumn(Length = 64)]
    public string ResultId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的任务 ID，标识该采集结果属于哪个任务
    /// </summary>
    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 步骤 ID，标识该采集结果属于哪个步骤
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? StepId { get; set; }

    /// <summary>
    /// 任务名称快照，便于结果浏览与导出。
    /// </summary>
    [SugarColumn(Length = 256, IsNullable = true)]
    public string? TaskName { get; set; }

    /// <summary>
    /// 任务状态快照。
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? TaskStatus { get; set; }

    /// <summary>
    /// 关联的表达式 ID，标识该结果使用的采集表达式
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? ExpressionId { get; set; }

    /// <summary>
    /// 执行采集的代理节点 ID
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? AgentId { get; set; }

    /// <summary>
    /// 数据来源 URL，标识该条数据从哪个页面采集
    /// </summary>
    [SugarColumn(Length = 2048, IsNullable = true)]
    public string? SourceUrl { get; set; }

    /// <summary>
    /// 采集字段数据 JSON，键为字段名，值为采集到的数据
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string Fields { get; set; } = "{}";

    /// <summary>
    /// 字段与表达式的映射关系 JSON，记录每个字段值对应的提取表达式
    /// </summary>
    [SugarColumn(ColumnDataType = "json", IsNullable = true)]
    public string? FieldExpressionMap { get; set; }

    /// <summary>
    /// 数据指纹，用于去重判断。
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = true)]
    public string? DataFingerprint { get; set; }

    /// <summary>
    /// 是否被判定为重复数据。
    /// </summary>
    public bool IsDuplicate { get; set; }

    /// <summary>
    /// 存储引擎信息，标识结果数据的存储方式
    /// </summary>
    [SugarColumn(Length = 32, IsNullable = true)]
    public string? StorageEngine { get; set; }

    /// <summary>
    /// 数据采集时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? CollectedAt { get; set; }

    /// <summary>
    /// 采集结果创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
