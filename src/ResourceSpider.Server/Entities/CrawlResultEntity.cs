using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Entities;

/// <summary>
/// 爬取结果实体，映射数据库 crawl_results 表
/// 存储任务执行过程中提取的结构化数据，每条记录对应一次数据提取的结果
/// </summary>
[SugarTable("crawl_results")]
public class CrawlResultEntity
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
    /// 关联的任务执行记录 ID，标识该结果属于哪次任务执行
    /// </summary>
    [SugarColumn(Length = 64)]
    public string ExecutionId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的任务 ID，标识该结果属于哪个任务
    /// </summary>
    [SugarColumn(Length = 64)]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的任务步骤 ID，标识该结果由哪个步骤产生
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? StepId { get; set; }

    /// <summary>
    /// 提取的结构化数据 JSON，包含根据提取规则获取的所有字段数据
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string ExtractedData { get; set; } = "{}";

    /// <summary>
    /// 数据来源 URL，标识该条数据从哪个页面提取
    /// </summary>
    [SugarColumn(Length = 2000, IsNullable = true)]
    public string? SourceUrl { get; set; }

    /// <summary>
    /// 结果创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
