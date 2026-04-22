namespace ResourceSpider.Core.Enums;

/// <summary>
/// 分页类型枚举，定义不同的分页获取策略
/// </summary>
public enum PaginationType
{
    /// <summary>
    /// 页码模式，通过递增页码参数实现分页
    /// </summary>
    PageNumber = 0,

    /// <summary>
    /// 偏移量模式，通过递增偏移量参数实现分页
    /// </summary>
    Offset = 1,

    /// <summary>
    /// 下一页链接模式，从页面中提取下一页的 URL
    /// </summary>
    NextPageUrl = 2,

    /// <summary>
    /// 点击下一页模式，模拟点击操作实现翻页
    /// </summary>
    ClickNext = 3,

    /// <summary>
    /// 无限滚动模式，模拟页面滚动加载更多内容
    /// </summary>
    InfiniteScroll = 4
}
