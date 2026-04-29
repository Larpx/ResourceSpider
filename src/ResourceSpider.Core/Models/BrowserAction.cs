namespace ResourceSpider.Core.Models;

/// <summary>
/// 浏览器动作模型，定义 Playwright 浏览器自动化中的页面交互操作
/// </summary>
public class BrowserAction
{
    /// <summary>
    /// 动作类型，如 "Click"（点击）、"Fill"（填充）、"Scroll"（滚动）、"Wait"（等待）、"Evaluate"（执行脚本）等
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// 目标元素的 CSS 选择器或 XPath
    /// </summary>
    public string? Selector { get; set; }

    /// <summary>
    /// 填充或输入的值
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// 动作执行前的延迟时间（毫秒）
    /// </summary>
    public int? DelayMs { get; set; }

    /// <summary>
    /// 滚动像素数，用于 Scroll 动作
    /// </summary>
    public int? ScrollPixels { get; set; }

    /// <summary>
    /// 下拉框选项值，用于 Select 动作
    /// </summary>
    public string? OptionValue { get; set; }

    /// <summary>
    /// 鼠标点击的 X 坐标，用于坐标点击
    /// </summary>
    public int? X { get; set; }

    /// <summary>
    /// 鼠标点击的 Y 坐标，用于坐标点击
    /// </summary>
    public int? Y { get; set; }

    /// <summary>
    /// 要执行的 JavaScript 脚本，用于 Evaluate 动作
    /// </summary>
    public string? Script { get; set; }

    /// <summary>
    /// 动作执行后的等待时间（毫秒）
    /// </summary>
    public int? WaitAfterMs { get; set; }
}
