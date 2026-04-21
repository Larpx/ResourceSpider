namespace ResourceSpider.Server.DTOs;

public record SystemHealthDto(
    string Status,
    string Version,
    TimeSpan Uptime,
    Dictionary<string, string> Components
);

public record SystemLogDto(
    string LogId,
    string Level,
    string Category,
    string Message,
    string? Detail,
    string? UserId,
    DateTime CreatedAt
);

public record SystemLogListResponse(
    List<SystemLogDto> Logs,
    int Total,
    int PageIndex,
    int PageSize
);
