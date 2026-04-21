using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

public class ExpressionConfig
{
    public string ExpressionId { get; set; } = Guid.NewGuid().ToString("N");

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ExpressionType SelectorType { get; set; }

    public string ContainerExpression { get; set; } = string.Empty;

    public List<ExpressionField> Fields { get; set; } = new();

    public List<GlobalValueConfig> GlobalValues { get; set; } = new();

    public List<FollowRequestConfig> FollowRequests { get; set; } = new();

    public ExpressionStatus Status { get; set; } = ExpressionStatus.Active;

    public int SuccessCount { get; set; }

    public int FailureCount { get; set; }

    public DateTime? LastValidatedAt { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ExpressionField
{
    public string FieldId { get; set; } = Guid.NewGuid().ToString("N");

    public string ExpressionId { get; set; } = string.Empty;

    public string FieldName { get; set; } = string.Empty;

    public ExpressionType SelectorType { get; set; }

    public string Expression { get; set; } = string.Empty;

    public string? AttributeName { get; set; }

    public bool IsRequired { get; set; }

    public string? DefaultValue { get; set; }

    public string? Formatter { get; set; }

    public string? FormatterArgs { get; set; }

    public int Order { get; set; }
}

public class GlobalValueConfig
{
    public string Name { get; set; } = string.Empty;

    public ExpressionType SelectorType { get; set; }

    public string Expression { get; set; } = string.Empty;
}

public class FollowRequestConfig
{
    public ExpressionType SelectorType { get; set; }

    public List<string> Expressions { get; set; } = new();

    public List<string> Patterns { get; set; } = new();
}
