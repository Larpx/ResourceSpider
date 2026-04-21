using ResourceSpider.Core.Selector;

namespace ResourceSpider.Infrastructure.DataFlow.Parser;

public class SelectorAttribute : Attribute
{
    public SelectorType Type { get; set; } = SelectorType.XPath;
    public string Expression { get; set; } = string.Empty;
    public string? Arguments { get; set; }

    public SelectorAttribute() { }
    public SelectorAttribute(string expression, SelectorType type = SelectorType.XPath, string? arguments = null)
    {
        Type = type;
        Expression = expression;
        Arguments = arguments;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class EntitySelectorAttribute : SelectorAttribute
{
    public int Take { get; set; }
    public bool TakeByDescending { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
public class ValueSelectorAttribute : SelectorAttribute
{
    internal System.Reflection.PropertyInfo PropertyInfo { get; set; } = null!;
    internal bool NotNull { get; set; }
    public FormatterAttribute[] Formatters { get; set; } = [];

    public ValueSelectorAttribute() { }
    public ValueSelectorAttribute(string expression, SelectorType type = SelectorType.XPath) : base(expression, type) { }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FollowRequestSelectorAttribute : Attribute
{
    public SelectorType SelectorType { get; set; } = SelectorType.XPath;
    public string[] Expressions { get; set; } = [];
    public string[] Patterns { get; set; } = [];
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class GlobalValueSelectorAttribute : ValueSelectorAttribute
{
    public string Name { get; set; } = string.Empty;
}

[AttributeUsage(AttributeTargets.Property)]
public abstract class FormatterAttribute : Attribute
{
    protected FormatterAttribute() { Name = GetType().Name; }
    public string Name { get; set; } = string.Empty;
    public string? Default { get; set; }
    protected abstract string? Handle(string? value);
    protected abstract void CheckArguments();
    public string? Format(string? value) { CheckArguments(); return value == default ? Default : Handle(value); }
}
