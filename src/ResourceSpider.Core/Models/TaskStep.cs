using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

public class TaskStep
{
    public string StepId { get; set; } = Guid.NewGuid().ToString("N");

    public string TaskId { get; set; } = string.Empty;

    public int StepOrder { get; set; }

    public string StepName { get; set; } = string.Empty;

    public string StepType { get; set; } = "DataCollection";

    public CollectionMode CollectionMode { get; set; } = CollectionMode.HttpClient;

    public string? AgentGroupId { get; set; }

    public StepRequestConfig? RequestConfig { get; set; }

    public List<ExtractionRule> ExtractionRules { get; set; } = new();

    public List<VariableMapping> VariableMappings { get; set; } = new();

    public PaginationConfig? PaginationConfig { get; set; }

    public OutputConfig? OutputConfig { get; set; }

    public StepStartCondition? StartCondition { get; set; }

    public StepEndCondition? EndCondition { get; set; }

    public List<string>? DependsOnStepIds { get; set; }

    public ResourcePoolConfig? ResourcePoolConfig { get; set; }

    public StepState State { get; set; } = StepState.Waiting;

    public string? StepConfig { get; set; }

    public int Timeout { get; set; } = 0;

    public StepRetryPolicy? RetryPolicy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
