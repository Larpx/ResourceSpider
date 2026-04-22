namespace ResourceSpider.Server.DTOs;

public record StepResourceDto(
    string ResourceId,
    string TaskId,
    string StepId,
    string? SourceStepId,
    string ResourceType,
    string Payload,
    string? SourceUrl,
    int Status,
    DateTime CreatedAt
);

public record AgentStatusDto(
    int OnlineCount,
    int BusyCount
);

public record ReportStepStatusRequest(
    string AgentId,
    string AgentToken,
    string TaskId,
    string StepId,
    int State,
    int DataCount = 0
);

public record PullStepResourcesRequest(
    string AgentId,
    string AgentToken,
    string TaskId,
    string StepId,
    int Take = 100
);

public record PrefetchTasksRequest(
    string AgentId,
    string AgentToken,
    int Count = 5
);

public record StepStatusDto(
    string StepId,
    string StepName,
    int StepOrder,
    int State,
    string? StartCondition,
    string? EndCondition,
    List<string>? DependsOnStepIds
);

public record StepResourceOverviewDto(
    string StepId,
    int TotalCount,
    List<StepResourceDto> Resources
);
