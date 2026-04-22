namespace ResourceSpider.Core.Enums;

public enum StepState
{
    Waiting = 0,
    Ready = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Skipped = 5
}

public enum ConditionType
{
    Manual = 0,
    StepDependency = 1,
    ResourceThreshold = 2,
    Expression = 3
}

public enum ConditionOperator
{
    GreaterThan = 0,
    GreaterThanOrEqual = 1,
    LessThan = 2,
    LessThanOrEqual = 3,
    Equal = 4,
    NotEqual = 5
}

public enum StorageEngine
{
    MySQL = 0,
    PostgreSQL = 1
}
