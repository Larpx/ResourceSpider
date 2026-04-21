using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

public record CreateExpressionRequest(
    [Required, StringLength(128)] string Name,
    [StringLength(512)] string? Description,
    [Required, StringLength(32)] string SelectorType,
    [StringLength(1024)] string? ContainerExpression,
    List<CreateExpressionFieldRequest>? Fields
);

public record CreateExpressionFieldRequest(
    [Required, StringLength(128)] string FieldName,
    [Required, StringLength(32)] string SelectorType,
    [Required, StringLength(1024)] string Expression,
    [StringLength(128)] string? AttributeName,
    bool IsRequired = false,
    [StringLength(256)] string? DefaultValue = null,
    [StringLength(64)] string? Formatter = null,
    [StringLength(512)] string? FormatterArgs = null
);

public record UpdateExpressionRequest(
    [StringLength(128)] string? Name,
    [StringLength(512)] string? Description,
    [StringLength(32)] string? SelectorType,
    [StringLength(1024)] string? ContainerExpression,
    int? Status,
    List<CreateExpressionFieldRequest>? Fields
);

public record ExpressionDto(
    string ExpressionId,
    string Name,
    string Description,
    string SelectorType,
    string ContainerExpression,
    List<ExpressionFieldDto> Fields,
    int Status,
    int SuccessCount,
    int FailureCount,
    int ConsecutiveFailures,
    DateTime? LastValidatedAt,
    DateTime? LastUsedAt,
    DateTime CreatedAt
);

public record ExpressionFieldDto(
    string FieldId,
    string ExpressionId,
    string FieldName,
    string SelectorType,
    string Expression,
    string? AttributeName,
    bool IsRequired,
    string? DefaultValue,
    string? Formatter,
    string? FormatterArgs,
    int Order
);

public record ExpressionListResponse(
    List<ExpressionDto> Expressions,
    int Total,
    int PageIndex,
    int PageSize
);

public record ExpressionConfigDto(
    string ExpressionId,
    string Name,
    string SelectorType,
    string ContainerExpression,
    List<ExpressionFieldConfigDto> Fields
);

public record ExpressionFieldConfigDto(
    string FieldName,
    string SelectorType,
    string Expression,
    string? AttributeName,
    bool IsRequired,
    string? DefaultValue,
    string? Formatter,
    string? FormatterArgs,
    int Order
);

public record ReportExpressionAvailabilityRequest(
    [Required] string AgentId,
    [Required] string AgentToken,
    [Required] string ExpressionId,
    bool IsAvailable,
    [StringLength(1024)] string? FailureReason
);
