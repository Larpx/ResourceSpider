namespace ResourceSpider.Core.Enums;

/// <summary>
/// 数据采集模式枚举，定义不同的页面获取方式
/// </summary>
public enum CollectionMode
{
    /// <summary>
    /// 使用 HttpClient 直接请求页面，适用于静态页面
    /// </summary>
    HttpClient = 0,

    /// <summary>
    /// 使用 Playwright 浏览器引擎渲染页面，适用于动态页面
    /// </summary>
    Playwright = 1,

    /// <summary>
    /// 浏览器自动化模式，支持复杂的交互操作
    /// </summary>
    BrowserAutomation = 2
}
