namespace ResourceSpider.Core.Models;

/// <summary>
/// 变量映射模型，定义任务步骤之间的数据传递关系
/// </summary>
public class VariableMapping
{
    /// <summary>
    /// 源字段名称，从上一步的输出中获取数据
    /// </summary>
    public string SourceField { get; set; } = string.Empty;

    /// <summary>
    /// 目标变量名称，作为当前步骤的输入变量
    /// </summary>
    public string TargetVariable { get; set; } = string.Empty;

    /// <summary>
    /// 转换表达式，对源字段值进行转换后赋给目标变量
    /// </summary>
    public string? Transform { get; set; }
}
