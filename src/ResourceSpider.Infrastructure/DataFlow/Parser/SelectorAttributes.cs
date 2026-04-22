using ResourceSpider.Core.Selector;

namespace ResourceSpider.Infrastructure.DataFlow.Parser;

/// <summary>
/// 选择器特性基类，用于标注数据提取的选择器类型和表达式
/// </summary>
public class SelectorAttribute : Attribute
{
    /// <summary>
    /// 选择器类型
    /// </summary>
    public SelectorType Type { get; set; } = SelectorType.XPath;

    /// <summary>
    /// 选择器表达式
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// 选择器附加参数，如正则选项和替换模式
    /// </summary>
    public string? Arguments { get; set; }

    public SelectorAttribute() { }

    /// <summary>
    /// 通过表达式和类型初始化选择器特性
    /// </summary>
    /// <param name="expression">选择器表达式</param>
    /// <param name="type">选择器类型</param>
    /// <param name="arguments">附加参数</param>
    public SelectorAttribute(string expression, SelectorType type = SelectorType.XPath, string? arguments = null)
    {
        Type = type;
        Expression = expression;
        Arguments = arguments;
    }
}

/// <summary>
/// 实体选择器特性，标注在类上用于指定实体的容器选择器和提取数量
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class EntitySelectorAttribute : SelectorAttribute
{
    /// <summary>
    /// 提取的最大数量
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// 是否按降序提取
    /// </summary>
    public bool TakeByDescending { get; set; }
}

/// <summary>
/// 值选择器特性，标注在属性上用于指定字段的选择器配置
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ValueSelectorAttribute : SelectorAttribute
{
    /// <summary>
    /// 关联的属性信息
    /// </summary>
    internal System.Reflection.PropertyInfo PropertyInfo { get; set; } = null!;

    /// <summary>
    /// 是否不允许为空
    /// </summary>
    internal bool NotNull { get; set; }

    /// <summary>
    /// 字段格式化器数组
    /// </summary>
    public FormatterAttribute[] Formatters { get; set; } = [];

    public ValueSelectorAttribute() { }

    /// <summary>
    /// 通过表达式和类型初始化值选择器特性
    /// </summary>
    /// <param name="expression">选择器表达式</param>
    /// <param name="type">选择器类型</param>
    public ValueSelectorAttribute(string expression, SelectorType type = SelectorType.XPath) : base(expression, type) { }
}

/// <summary>
/// 后续请求选择器特性，标注在类上用于指定后续请求链接的提取规则
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FollowRequestSelectorAttribute : Attribute
{
    /// <summary>
    /// 选择器类型
    /// </summary>
    public SelectorType SelectorType { get; set; } = SelectorType.XPath;

    /// <summary>
    /// 选择器表达式数组
    /// </summary>
    public string[] Expressions { get; set; } = [];

    /// <summary>
    /// URL 匹配模式数组
    /// </summary>
    public string[] Patterns { get; set; } = [];
}

/// <summary>
/// 全局值选择器特性，用于提取全局共享的变量值
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class GlobalValueSelectorAttribute : ValueSelectorAttribute
{
    /// <summary>
    /// 全局变量名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 格式化器特性基类，提供字段值的格式化处理能力
/// 子类通过实现 Handle 方法定义具体的格式化逻辑
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public abstract class FormatterAttribute : Attribute
{
    /// <summary>
    /// 初始化格式化器特性
    /// </summary>
    protected FormatterAttribute() { Name = GetType().Name; }

    /// <summary>
    /// 格式化器名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 格式化失败时的默认值
    /// </summary>
    public string? Default { get; set; }

    /// <summary>
    /// 执行格式化处理
    /// </summary>
    /// <param name="value">输入值</param>
    /// <returns>格式化后的值</returns>
    protected abstract string? Handle(string? value);

    /// <summary>
    /// 检查格式化器参数是否有效
    /// </summary>
    protected abstract void CheckArguments();

    /// <summary>
    /// 格式化值，先检查参数再执行处理
    /// </summary>
    /// <param name="value">输入值</param>
    /// <returns>格式化后的值，输入为 null 时返回默认值</returns>
    public string? Format(string? value) { CheckArguments(); return value == default ? Default : Handle(value); }
}
