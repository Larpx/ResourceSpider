namespace ResourceSpider.Server.DTOs;

public record ConfigVersionDto(
    string VersionId,
    string TaskId,
    int Version,
    string ConfigContent,
    string? ChangeDescription,
    string? CreatedBy,
    DateTime CreatedAt
);

public record ConfigVersionListResponse(
    List<ConfigVersionDto> Versions,
    int Total
);

public record RollbackConfigRequest(
    int Version
);
