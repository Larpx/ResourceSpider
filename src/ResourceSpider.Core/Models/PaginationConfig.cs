using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

/// <summary>
/// 分页配置模型，定义多页数据的获取策略
/// </summary>
public class PaginationConfig
{
    /// <summary>
    /// 分页类型
    /// </summary>
    public PaginationType PaginationType { get; set; }

    /// <summary>
    /// 页码参数名，用于页码和偏移量模式
    /// </summary>
    public string? PageParamName { get; set; }

    /// <summary>
    /// 起始页码
    /// </summary>
    public int StartPage { get; set; } = 1;

    /// <summary>
    /// 结束页码，为空时不限制
    /// </summary>
    public int? EndPage { get; set; }

    /// <summary>
    /// 页码递增量
    /// </summary>
    public int PageIncrement { get; set; } = 1;

    /// <summary>
    /// 下一页选择器，用于 NextPageUrl 和 ClickNext 模式
    /// </summary>
    public string? NextPageSelector { get; set; }

    /// <summary>
    /// 滚动等待时间（毫秒），用于 InfiniteScroll 模式
    /// </summary>
    public int ScrollWaitTime { get; set; } = 2000;

    /// <summary>
    /// 最大爬取页数限制
    /// </summary>
    public int? MaxPages { get; set; }

    /// <summary>
    /// URL 模式，包含 {page} 占位符用于生成翻页 URL
    /// </summary>
    public string? UrlPattern { get; set; }
}
