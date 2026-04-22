namespace ResourceSpider.Server.DTOs;

/// <summary>
/// 系统健康状态数据传输对象
/// </summary>
/// <param name="Status">系统状态（Healthy/Degraded/Unhealthy）</param>
/// <param name="Version">系统版本号</param>
/// <param name="Uptime">系统运行时长</param>
/// <param name="Components">依赖组件状态字典</param>
public record SystemHealthDto(
    string Status,
    string Version,
    TimeSpan Uptime,
    Dictionary<string, string> Components
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
