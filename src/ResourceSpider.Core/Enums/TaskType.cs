namespace ResourceSpider.Core.Enums;

/// <summary>
/// 任务类型枚举，定义不同的爬取策略
/// </summary>
public enum TaskType
{
    /// <summary>
    /// 单页面模式，仅爬取指定 URL 的单个页面
    /// </summary>
    SinglePage = 0,

    /// <summary>
    /// 分页模式，自动翻页爬取多页数据
    /// </summary>
    Paginated = 1,

    /// <summary>
    /// 多阶段模式，按步骤依次执行多个爬取阶段
    /// </summary>
    MultiStage = 2
}
