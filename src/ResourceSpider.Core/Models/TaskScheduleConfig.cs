namespace Larpx.PersonalTools.ResourceSpider.Core.Models;

/// <summary>
/// 任务调度配置模型，定义任务的定时执行策略，支持 Cron 表达式、固定间隔和一次性执行
/// </summary>
public class TaskScheduleConfig
{
    /// <summary>
    /// 调度类型，可选值为 "Once"（一次性）、"Cron"（Cron 表达式）、"Interval"（固定间隔），默认 "Once"
    /// </summary>
    public string ScheduleType { get; set; } = "Once";

    /// <summary>
    /// Cron 表达式，当 ScheduleType 为 "Cron" 时使用
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// 固定间隔时间（秒），当 ScheduleType 为 "Interval" 时使用
    /// </summary>
    public int? IntervalSeconds { get; set; }

    /// <summary>
    /// 调度开始时间，为 null 时立即开始
    /// </summary>
    public DateTime? StartAt { get; set; }

    /// <summary>
    /// 调度结束时间，为 null 时不限制
    /// </summary>
    public DateTime? EndAt { get; set; }

    /// <summary>
    /// 是否启用调度，默认启用
    /// </summary>
    public bool Enabled { get; set; } = true;
}
