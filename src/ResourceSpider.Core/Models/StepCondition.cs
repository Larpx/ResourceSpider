using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

/// <summary>
/// 步骤开始条件模型，定义步骤执行的前置条件，支持手动触发、步骤依赖、资源阈值和表达式判断
/// </summary>
public class StepStartCondition
{
    /// <summary>
    /// 条件类型，默认 Manual（手动触发）
    /// </summary>
    public ConditionType Type { get; set; } = ConditionType.Manual;

    /// <summary>
    /// 依赖的步骤 ID，当 Type 为 StepDependency 时使用
    /// </summary>
    public string? DependsOnStepId { get; set; }

    /// <summary>
    /// 资源字段名称，当 Type 为 ResourceThreshold 时使用
    /// </summary>
    public string? ResourceField { get; set; }

    /// <summary>
    /// 条件比较运算符，默认 GreaterThan
    /// </summary>
    public ConditionOperator Operator { get; set; } = ConditionOperator.GreaterThan;

    /// <summary>
    /// 阈值，与资源计数值进行比较
    /// </summary>
    public int Threshold { get; set; }

    /// <summary>
    /// 自定义表达式，当 Type 为 Expression 时使用，支持 {{变量名}} 占位符
    /// </summary>
    public string? Expression { get; set; }

    /// <summary>
    /// 根据条件类型评估步骤是否应该开始执行
    /// </summary>
    /// <param name="context">上下文变量字典，包含步骤状态和资源计数等信息</param>
    /// <returns>条件满足返回 true，否则返回 false</returns>
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

    /// <summary>
    /// 评估步骤依赖条件，检查依赖步骤是否已完成
    /// </summary>
    /// <param name="context">上下文变量字典</param>
    /// <returns>依赖步骤已完成返回 true</returns>
    private bool EvaluateStepDependency(Dictionary<string, object?> context)
    {
        if (string.IsNullOrEmpty(DependsOnStepId)) return false;
        var key = $"step_{DependsOnStepId}_state";
        if (!context.TryGetValue(key, out var value) || value == null) return false;
        var stateValue = Convert.ToInt32(value);
        return stateValue == (int)StepState.Completed;
    }

    /// <summary>
    /// 评估资源阈值条件，检查资源计数是否满足运算符和阈值要求
    /// </summary>
    /// <param name="context">上下文变量字典</param>
    /// <returns>资源计数满足条件返回 true</returns>
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

    /// <summary>
    /// 评估自定义表达式，将变量替换后解析比较表达式
    /// </summary>
    /// <param name="context">上下文变量字典</param>
    /// <returns>表达式结果为真返回 true</returns>
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

    /// <summary>
    /// 解析并计算简单的比较表达式，支持 >=、<=、!=、>、<、== 运算符
    /// </summary>
    /// <param name="expr">待计算的表达式字符串</param>
    /// <returns>表达式结果</returns>
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

/// <summary>
/// 步骤结束条件模型，定义步骤提前结束的条件，支持资源阈值和表达式判断
/// </summary>
public class StepEndCondition
{
    /// <summary>
    /// 条件类型，默认 ResourceThreshold（资源阈值）
    /// </summary>
    public ConditionType Type { get; set; } = ConditionType.ResourceThreshold;

    /// <summary>
    /// 资源字段名称，当 Type 为 ResourceThreshold 时使用
    /// </summary>
    public string? ResourceField { get; set; }

    /// <summary>
    /// 条件比较运算符，默认 GreaterThanOrEqual
    /// </summary>
    public ConditionOperator Operator { get; set; } = ConditionOperator.GreaterThanOrEqual;

    /// <summary>
    /// 阈值，与当前数据量进行比较
    /// </summary>
    public int Threshold { get; set; }

    /// <summary>
    /// 自定义表达式，当 Type 为 Expression 时使用
    /// </summary>
    public string? Expression { get; set; }

    /// <summary>
    /// 判断当前数据量是否满足结束条件
    /// </summary>
    /// <param name="currentCount">当前已采集的数据量</param>
    /// <param name="context">上下文变量字典</param>
    /// <returns>满足结束条件返回 true</returns>
    public bool IsSatisfied(int currentCount, Dictionary<string, object?> context)
    {
        return Type switch
        {
            ConditionType.ResourceThreshold => EvaluateResourceThreshold(currentCount),
            ConditionType.Expression => EvaluateExpression(context),
            _ => false
        };
    }

    /// <summary>
    /// 评估资源阈值条件
    /// </summary>
    /// <param name="currentCount">当前数据量</param>
    /// <returns>满足阈值条件返回 true</returns>
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

    /// <summary>
    /// 评估自定义表达式
    /// </summary>
    /// <param name="context">上下文变量字典</param>
    /// <returns>表达式结果为真返回 true</returns>
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

    /// <summary>
    /// 解析并计算简单的比较表达式
    /// </summary>
    /// <param name="expr">待计算的表达式字符串</param>
    /// <returns>表达式结果</returns>
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

/// <summary>
/// 资源池配置模型，定义步骤间数据传递和资源管理的策略
/// </summary>
public class ResourcePoolConfig
{
    /// <summary>
    /// 是否自动将步骤采集的数据传递给下一步骤，默认启用
    /// </summary>
    public bool AutoFeedToNextStep { get; set; } = true;

    /// <summary>
    /// 资源类型标识
    /// </summary>
    public string? ResourceType { get; set; }

    /// <summary>
    /// 每个步骤的最大资源数量，0 表示不限制
    /// </summary>
    public int MaxResourcesPerStep { get; set; } = 0;

    /// <summary>
    /// 接收资源传递的目标步骤 ID 列表
    /// </summary>
    public List<string>? FeedToStepIds { get; set; }
}
