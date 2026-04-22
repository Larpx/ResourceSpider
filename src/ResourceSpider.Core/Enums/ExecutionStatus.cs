namespace ResourceSpider.Core.Enums;

/// <summary>
/// 任务执行状态枚举，描述任务执行的生命周期
/// </summary>
public enum ExecutionStatus
{
    /// <summary>
    /// 等待中，任务已创建但尚未开始执行
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 运行中，任务正在执行
    /// </summary>
    Running = 1,

    /// <summary>
    /// 已完成，任务执行成功
    /// </summary>
    Completed = 2,

    /// <summary>
    /// 已失败，任务执行过程中出现错误
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 已取消，任务被用户手动取消
    /// </summary>
    Cancelled = 4
}
