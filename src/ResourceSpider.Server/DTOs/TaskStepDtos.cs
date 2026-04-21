using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

public record CreateTaskStepRequest(
    [Required, StringLength(100)] string StepName,
    [Range(1, 100)] int StepOrder,
    [Required, StringLength(64)] string CollectionMode,
    string? AgentGroupId = null,
    string? RequestConfig = null,
    string? ExtractionRules = null,
    string? VariableMappings = null,
    string? PaginationConfig = null,
    string? OutputConfig = null
);

public record UpdateTaskStepRequest(
    string? StepName,
    int? StepOrder,
    string? CollectionMode,
    string? AgentGroupId,
    string? RequestConfig,
    string? ExtractionRules,
    string? VariableMappings,
    string? PaginationConfig,
    string? OutputConfig
);

public record TaskStepDto(
    string StepId,
    string TaskId,
    int StepOrder,
    string StepName,
    string CollectionMode,
    string? AgentGroupId,
    string RequestConfig,
    string ExtractionRules,
    string? VariableMappings,
    string? PaginationConfig,
    string? OutputConfig,
    DateTime CreatedAt
);
