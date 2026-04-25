namespace ResourceSpider.Server.DTOs;

/// <summary>
/// 系统健康状态数据传输对象
/// </summary>
/// <param name="Status">系统状态（Healthy/Degraded/Unhealthy）</param>
/// <param name="Version">系统版本号</param>
/// <param name="Uptime">系统运行时长</param>
/// <param name="Components">依赖组件状态字典</param>
/// <param name="Load">当前服务负载快照</param>
/// <param name="StartedAtUtc">服务启动时间（UTC）</param>
/// <param name="TimestampUtc">响应时间戳（UTC）</param>
/// <param name="Environment">运行环境名称</param>
public record SystemHealthDto(
    string Status,
    string Version,
    TimeSpan Uptime,
    Dictionary<string, string> Components,
    SystemLoadSnapshotDto? Load = null,
    DateTime? StartedAtUtc = null,
    DateTime? TimestampUtc = null,
    string? Environment = null
);

/// <summary>
/// 系统负载快照
/// </summary>
/// <param name="CpuLoadPercent">进程平均 CPU 负载百分比（基于运行期估算）</param>
/// <param name="WorkingSetMb">进程工作集内存（MB）</param>
/// <param name="GcHeapMb">GC 托管堆内存（MB）</param>
/// <param name="ThreadPoolAvailableWorkers">线程池可用 Worker 线程数</param>
/// <param name="ThreadPoolMaxWorkers">线程池最大 Worker 线程数</param>
/// <param name="PendingWorkItems">线程池待处理工作项数</param>
public record SystemLoadSnapshotDto(
    double CpuLoadPercent,
    double WorkingSetMb,
    double GcHeapMb,
    int ThreadPoolAvailableWorkers,
    int ThreadPoolMaxWorkers,
    long PendingWorkItems
);

/// <summary>
/// Agent 负载聚合快照
/// </summary>
/// <param name="TotalAgents">Agent 总数</param>
/// <param name="OnlineAgents">在线 Agent 数</param>
/// <param name="BusyAgents">忙碌 Agent 数</param>
/// <param name="TotalRunningTasks">Agent 正在执行任务总数</param>
/// <param name="AverageCpuUsage">在线 Agent 平均 CPU 使用率</param>
/// <param name="AverageMemoryUsage">在线 Agent 平均内存使用率</param>
public record AgentLoadSnapshotDto(
    int TotalAgents,
    int OnlineAgents,
    int BusyAgents,
    int TotalRunningTasks,
    decimal AverageCpuUsage,
    decimal AverageMemoryUsage
);

/// <summary>
/// 运行时 Agent 状态项
/// </summary>
/// <param name="AgentId">Agent 标识</param>
/// <param name="AgentName">Agent 名称</param>
/// <param name="Status">状态文本</param>
/// <param name="CpuUsage">CPU 使用率</param>
/// <param name="MemoryUsage">内存使用率</param>
/// <param name="TaskCount">执行中任务数</param>
/// <param name="LastHeartbeat">最后心跳时间</param>
public record RuntimeAgentStatusDto(
    string AgentId,
    string AgentName,
    string Status,
    decimal? CpuUsage,
    decimal? MemoryUsage,
    int TaskCount,
    DateTime? LastHeartbeat
);

/// <summary>
/// 运行时输出日志项
/// </summary>
/// <param name="Sequence">日志序号（用于前端排序和去重）</param>
/// <param name="TimestampUtc">时间戳（UTC）</param>
/// <param name="Level">日志级别</param>
/// <param name="Source">日志来源</param>
/// <param name="Message">日志消息</param>
public record RuntimeOutputLogDto(
    long Sequence,
    DateTime TimestampUtc,
    string Level,
    string Source,
    string Message
);

/// <summary>
/// 系统运行时状态详情
/// </summary>
/// <param name="Status">当前整体状态</param>
/// <param name="Version">版本号</param>
/// <param name="Environment">运行环境</param>
/// <param name="MachineName">机器名</param>
/// <param name="Framework">运行时框架</param>
/// <param name="OsDescription">操作系统描述</param>
/// <param name="ProcessId">进程 ID</param>
/// <param name="Uptime">运行时长</param>
/// <param name="CurrentLoad">当前负载</param>
/// <param name="AgentLoad">Agent 负载聚合</param>
/// <param name="Agents">Agent 详情列表</param>
/// <param name="RecentLogs">最近系统日志</param>
/// <param name="RuntimeOutputLogs">程序运行时输出（内存缓冲）</param>
/// <param name="TimestampUtc">响应时间戳（UTC）</param>
public record SystemRuntimeStatusDto(
    string Status,
    string Version,
    string Environment,
    string MachineName,
    string Framework,
    string OsDescription,
    int ProcessId,
    TimeSpan Uptime,
    SystemLoadSnapshotDto CurrentLoad,
    AgentLoadSnapshotDto AgentLoad,
    List<RuntimeAgentStatusDto> Agents,
    List<SystemLogDto> RecentLogs,
    List<RuntimeOutputLogDto> RuntimeOutputLogs,
    DateTime TimestampUtc
);

/// <summary>
/// 系统日志数据传输对象
/// </summary>
/// <param name="LogId">日志 ID</param>
/// <param name="Level">日志级别</param>
/// <param name="Category">日志分类</param>
/// <param name="Message">日志消息</param>
/// <param name="Detail">详细信息</param>
/// <param name="UserId">关联用户 ID</param>
/// <param name="CreatedAt">创建时间</param>
public record SystemLogDto(
    string LogId,
    string Level,
    string Category,
    string Message,
    string? Detail,
    string? UserId,
    DateTime CreatedAt
);

/// <summary>
/// 系统日志列表响应，包含分页信息
/// </summary>
/// <param name="Logs">日志列表</param>
/// <param name="Total">总数</param>
/// <param name="PageIndex">当前页码</param>
/// <param name="PageSize">每页数量</param>
public record SystemLogListResponse(
    List<SystemLogDto> Logs,
    int Total,
    int PageIndex,
    int PageSize
);
