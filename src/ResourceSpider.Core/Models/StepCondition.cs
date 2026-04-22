using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

public class StepStartCondition
{
    public ConditionType Type { get; set; } = ConditionType.Manual;

    public string? DependsOnStepId { get; set; }

    public string? ResourceField { get; set; }

    public ConditionOperator Operator { get; set; } = ConditionOperator.GreaterThan;

    public int Threshold { get; set; }

    public string? Expression { get; set; }

    public bool Evaluate(Dictionary<string, object?> context)
    {
        return Type switch
        {
            ConditionType.Manual => true,
            ConditionType.StepDependency => EvaluateStepDependency(context),
            ConditionType.ResourceThreshold => EvaluateResourceThreshold(context),
            ConditionType.Expression => EvaluateExpression(context),
            _ => false
        };
    }

    private bool EvaluateStepDependency(Dictionary<string, object?> context)
    {
        if (string.IsNullOrEmpty(DependsOnStepId)) return false;
        var key = $"step_{DependsOnStepId}_state";
        if (!context.TryGetValue(key, out var value) || value == null) return false;
        var stateValue = Convert.ToInt32(value);
        return stateValue == (int)StepState.Completed;
    }

    private bool EvaluateResourceThreshold(Dictionary<string, object?> context)
    {
        if (string.IsNullOrEmpty(ResourceField)) return false;
        var key = $"resource_{ResourceField}_count";
        if (!context.TryGetValue(key, out var value) || value == null) return false;
        var count = Convert.ToInt32(value);
        return Operator switch
        {
            ConditionOperator.GreaterThan => count > Threshold,
            ConditionOperator.GreaterThanOrEqual => count >= Threshold,
            ConditionOperator.LessThan => count < Threshold,
            ConditionOperator.LessThanOrEqual => count <= Threshold,
            ConditionOperator.Equal => count == Threshold,
            ConditionOperator.NotEqual => count != Threshold,
            _ => false
        };
    }

    private bool EvaluateExpression(Dictionary<string, object?> context)
    {
        if (string.IsNullOrEmpty(Expression)) return false;
        try
        {
            var expr = Expression;
            foreach (var kvp in context)
            {
                expr = expr.Replace($"{{{{{kvp.Key}}}}}", kvp.Value?.ToString() ?? "0");
            }
            return EvaluateSimpleExpression(expr);
        }
        catch
        {
            return false;
        }
    }

    private static bool EvaluateSimpleExpression(string expr)
    {
        var operators = new[] { ">=", "<=", "!=", ">", "<", "==" };
        foreach (var op in operators)
        {
            var parts = expr.Split(op, 2);
            if (parts.Length == 2)
            {
                var left = double.TryParse(parts[0].Trim(), out var l) ? l : 0;
                var right = double.TryParse(parts[1].Trim(), out var r) ? r : 0;
                return op switch
                {
                    ">=" => left >= right,
                    "<=" => left <= right,
                    "!=" => left != right,
                    ">" => left > right,
                    "<" => left < right,
                    "==" => Math.Abs(left - right) < 0.001,
                    _ => false
                };
            }
        }
        return bool.TryParse(expr, out var result) && result;
    }
}

public class StepEndCondition
{
    public ConditionType Type { get; set; } = ConditionType.ResourceThreshold;

    public string? ResourceField { get; set; }

    public ConditionOperator Operator { get; set; } = ConditionOperator.GreaterThanOrEqual;

    public int Threshold { get; set; }

    public string? Expression { get; set; }

    public bool IsSatisfied(int currentCount, Dictionary<string, object?> context)
    {
        return Type switch
        {
            ConditionType.ResourceThreshold => EvaluateResourceThreshold(currentCount),
            ConditionType.Expression => EvaluateExpression(context),
            _ => false
        };
    }

    private bool EvaluateResourceThreshold(int currentCount)
    {
        return Operator switch
        {
            ConditionOperator.GreaterThan => currentCount > Threshold,
            ConditionOperator.GreaterThanOrEqual => currentCount >= Threshold,
            ConditionOperator.LessThan => currentCount < Threshold,
            ConditionOperator.LessThanOrEqual => currentCount <= Threshold,
            ConditionOperator.Equal => currentCount == Threshold,
            ConditionOperator.NotEqual => currentCount != Threshold,
            _ => false
        };
    }

    private bool EvaluateExpression(Dictionary<string, object?> context)
    {
        if (string.IsNullOrEmpty(Expression)) return false;
        try
        {
            var expr = Expression;
            foreach (var kvp in context)
            {
                expr = expr.Replace($"{{{{{kvp.Key}}}}}", kvp.Value?.ToString() ?? "0");
            }
            return EvaluateSimpleExpression(expr);
        }
        catch
        {
            return false
            ;
        }
    }

    private static bool EvaluateSimpleExpression(string expr)
    {
        var operators = new[] { ">=", "<=", "!=", ">", "<", "==" };
        foreach (var op in operators)
        {
            var parts = expr.Split(op, 2);
            if (parts.Length == 2)
            {
                var left = double.TryParse(parts[0].Trim(), out var l) ? l : 0;
                var right = double.TryParse(parts[1].Trim(), out var r) ? r : 0;
                return op switch
                {
                    ">=" => left >= right,
                    "<=" => left <= right,
                    "!=" => left != right,
                    ">" => left > right,
                    "<" => left < right,
                    "==" => Math.Abs(left - right) < 0.001,
                    _ => false
                };
            }
        }
        return bool.TryParse(expr, out var result) && result;
    }
}

public class ResourcePoolConfig
{
    public bool AutoFeedToNextStep { get; set; } = true;

    public string? ResourceType { get; set; }

    public int MaxResourcesPerStep { get; set; } = 0;

    public List<string>? FeedToStepIds { get; set; }
}
