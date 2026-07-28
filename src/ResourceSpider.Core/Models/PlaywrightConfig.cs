namespace Larpx.PersonalTools.ResourceSpider.Core.Models;

/// <summary>
/// Playwright 浏览器配置模型，定义浏览器自动化采集的详细参数
/// </summary>
public class PlaywrightConfig
{
    /// <summary>
    /// 浏览器类型，如 "Chromium"、"Firefox"、"WebKit"，默认 "Chromium"
    /// </summary>
    public string BrowserType { get; set; } = "Chromium";

    /// <summary>
    /// 是否以无头模式运行浏览器，默认启用
    /// </summary>
    public bool Headless { get; set; } = true;

    /// <summary>
    /// 等待指定选择器的元素出现
    /// </summary>
    public string? WaitForSelector { get; set; }

    /// <summary>
    /// 是否等待网络空闲，默认启用
    /// </summary>
    public bool WaitForNetworkIdle { get; set; } = true;

    /// <summary>
    /// 等待网络空闲的超时时间（毫秒），默认 30000ms
    /// </summary>
    public int WaitForNetworkIdleTimeout { get; set; } = 30000;

    /// <summary>
    /// 页面加载后执行的浏览器动作列表
    /// </summary>
    public List<BrowserAction>? Actions { get; set; }

    /// <summary>
    /// 页面加载后执行的 JavaScript 脚本列表
    /// </summary>
    public List<string>? Scripts { get; set; }

    /// <summary>
    /// 浏览器视口宽度（像素），默认 1920
    /// </summary>
    public int ViewportWidth { get; set; } = 1920;

    /// <summary>
    /// 浏览器视口高度（像素），默认 1080
    /// </summary>
    public int ViewportHeight { get; set; } = 1080;

    /// <summary>
    /// 是否禁用图片加载，默认不禁用
    /// </summary>
    public bool DisableImages { get; set; }

    /// <summary>
    /// 是否禁用 CSS 加载，默认不禁用
    /// </summary>
    public bool DisableCss { get; set; }

    /// <summary>
    /// 是否禁用字体加载，默认不禁用
    /// </summary>
    public bool DisableFonts { get; set; }

    /// <summary>
    /// 自定义 User-Agent 字符串
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 浏览器请求使用的代理配置
    /// </summary>
    public StepProxyConfig? ProxyConfig { get; set; }
}
