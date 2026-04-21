namespace ResourceSpider.Server.DTOs;

public record CollectionResultDto(
    string ResultId,
    string TaskId,
    string ExpressionId,
    string AgentId,
    string SourceUrl,
    Dictionary<string, object?> Fields,
    Dictionary<string, string> FieldExpressionMap,
    DateTime? CollectedAt,
    DateTime CreatedAt
);

public record CollectionResultListResponse(
    List<CollectionResultDto> Results,
    int Total,
    int PageIndex,
    int PageSize
);

public record CollectionResultItemDto(
    string? ResultId,
    string? SourceUrl,
    Dictionary<string, object?> Fields,
    Dictionary<string, string>? FieldExpressionMap,
    DateTime? CollectedAt
);

public record StoreCollectionResultsRequest(
    string AgentId,
    string AgentToken,
    string TaskId,
    string? ExpressionId,
    List<CollectionResultItemDto> Results
);

public record PullExpressionRequest(
    [System.ComponentModel.DataAnnotations.Required] string AgentId,
    [System.ComponentModel.DataAnnotations.Required] string AgentToken,
    string? ExpressionId
);
