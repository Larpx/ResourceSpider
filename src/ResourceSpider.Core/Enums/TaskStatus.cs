namespace ResourceSpider.Core.Enums;

/// <summary>
/// 任务状态枚举，描述爬虫任务从创建到结束的完整生命周期
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// 等待中，任务已创建等待调度
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 运行中，任务正在被代理节点执行
    /// </summary>
    Running = 1,

    /// <summary>
    /// 已完成，任务执行成功
    /// </summary>
    Completed = 2,

    /// <summary>
    /// 已失败，任务执行过程中出现不可恢复的错误
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 已暂停，任务被用户手动暂停
    /// </summary>
    Paused = 4,

    /// <summary>
    /// 等待恢复，任务暂停后等待重新启动
    /// </summary>
    WaitingRecovery = 5,

    /// <summary>
    /// 已取消，任务被用户手动取消
    /// </summary>
    Cancelled = 6
}
