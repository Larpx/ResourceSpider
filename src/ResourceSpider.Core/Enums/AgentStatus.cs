namespace ResourceSpider.Core.Enums;

/// <summary>
/// 代理节点在线状态枚举
/// </summary>
public enum AgentStatus
{
    /// <summary>
    /// 离线状态，代理节点未连接
    /// </summary>
    Offline = 0,

    /// <summary>
    /// 在线状态，代理节点已连接且空闲
    /// </summary>
    Online = 1,

    /// <summary>
    /// 忙碌状态，代理节点正在执行任务
    /// </summary>
    Busy = 2
}
