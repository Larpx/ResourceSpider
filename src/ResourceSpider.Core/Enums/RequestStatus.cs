namespace Larpx.PersonalTools.ResourceSpider.Core.Enums;

/// <summary>
/// 请求处理状态枚举，描述单个请求的生命周期
/// </summary>
public enum RequestStatus
{
    /// <summary>
    /// 等待中，请求已入队但尚未开始处理
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 运行中，请求正在被处理
    /// </summary>
    Running = 1,

    /// <summary>
    /// 已成功，请求处理完成且结果正常
    /// </summary>
    Success = 2,

    /// <summary>
    /// 已失败，请求处理过程中出现错误
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 已超时，请求在规定时间内未完成
    /// </summary>
    Timeout = 4,

    /// <summary>
    /// 已跳过，请求因去重或其他策略被跳过
    /// </summary>
    Skipped = 5
}
