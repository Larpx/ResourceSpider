using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

/// <summary>
/// 分页配置模型，定义爬虫任务的翻页采集策略
/// </summary>
public class PaginationConfig
{
    /// <summary>
    /// 分页类型，如页码翻页、偏移量翻页、下一页链接等
    /// </summary>
    public PaginationType PaginationType { get; set; }

    /// <summary>
    /// 页码参数名称，如 "page"、"pageNum" 等
    /// </summary>
    public string? PageParamName { get; set; }

    /// <summary>
    /// 起始页码，默认为 1
    /// </summary>
    public int StartPage { get; set; } = 1;

    /// <summary>
    /// 结束页码，为 null 时不限制
    /// </summary>
    public int? EndPage { get; set; }

    /// <summary>
    /// 页码递增量，默认为 1
    /// </summary>
    public int PageIncrement { get; set; } = 1;

    /// <summary>
    /// 偏移量递增值，用于偏移量分页模式
    /// </summary>
    public int? OffsetIncrement { get; set; }

    /// <summary>
    /// 下一页链接的 CSS 选择器，用于链接翻页模式
    /// </summary>
    public string? NextPageSelector { get; set; }

    /// <summary>
    /// 滚动等待时间（毫秒），用于无限滚动加载模式，默认 2000ms
    /// </summary>
    public int ScrollWaitTime { get; set; } = 2000;

    /// <summary>
    /// 最大翻页数，为 null 时不限制
    /// </summary>
    public int? MaxPages { get; set; }

    /// <summary>
    /// URL 模板，支持 {page}、{offset} 等占位符
    /// </summary>
    public string? UrlPattern { get; set; }
}
