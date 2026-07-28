namespace Larpx.PersonalTools.ResourceSpider.Core.Enums;

/// <summary>
/// 步骤执行状态枚举，定义任务步骤的生命周期状态
/// </summary>
public enum StepState
{
    /// <summary>
    /// 等待中，步骤尚未开始执行
    /// </summary>
    Waiting = 0,

    /// <summary>
    /// 就绪，步骤满足开始条件，等待执行
    /// </summary>
    Ready = 1,

    /// <summary>
    /// 运行中，步骤正在执行
    /// </summary>
    Running = 2,

    /// <summary>
    /// 已完成，步骤执行成功
    /// </summary>
    Completed = 3,

    /// <summary>
    /// 已失败，步骤执行失败
    /// </summary>
    Failed = 4,

    /// <summary>
    /// 已跳过，步骤因条件不满足而被跳过
    /// </summary>
    Skipped = 5
}

/// <summary>
/// 条件类型枚举，定义步骤开始/结束条件的类型
/// </summary>
public enum ConditionType
{
    /// <summary>
    /// 手动触发
    /// </summary>
    Manual = 0,

    /// <summary>
    /// 步骤依赖条件，依赖其他步骤完成后触发
    /// </summary>
    StepDependency = 1,

    /// <summary>
    /// 资源阈值条件，当资源数量达到阈值时触发
    /// </summary>
    ResourceThreshold = 2,

    /// <summary>
    /// 表达式条件，通过自定义表达式判断
    /// </summary>
    Expression = 3
}

/// <summary>
/// 条件运算符枚举，定义条件比较的运算方式
/// </summary>
public enum ConditionOperator
{
    /// <summary>
    /// 大于
    /// </summary>
    GreaterThan = 0,

    /// <summary>
    /// 大于等于
    /// </summary>
    GreaterThanOrEqual = 1,

    /// <summary>
    /// 小于
    /// </summary>
    LessThan = 2,

    /// <summary>
    /// 小于等于
    /// </summary>
    LessThanOrEqual = 3,

    /// <summary>
    /// 等于
    /// </summary>
    Equal = 4,

    /// <summary>
    /// 不等于
    /// </summary>
    NotEqual = 5
}

/// <summary>
/// 存储引擎枚举，定义采集结果的存储后端类型
/// </summary>
public enum StorageEngine
{
    /// <summary>
    /// MySQL 存储引擎
    /// </summary>
    MySQL = 0,

    /// <summary>
    /// PostgreSQL 存储引擎
    /// </summary>
    PostgreSQL = 1
}
