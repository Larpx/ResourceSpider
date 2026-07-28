namespace Larpx.PersonalTools.ResourceSpider.Core.Interfaces;

/// <summary>
/// 变量解析器接口，用于解析模板字符串中的变量占位符
/// </summary>
public interface IVariableResolver
{
    /// <summary>
    /// 解析模板字符串，将变量占位符替换为实际值
    /// </summary>
    /// <param name="template">包含变量占位符的模板字符串</param>
    /// <param name="variables">变量名值对</param>
    /// <returns>替换变量后的结果字符串</returns>
    string Resolve(string template, Dictionary<string, object?> variables);

    /// <summary>
    /// 获取系统变量集合，包括任务ID、步骤ID、代理ID、时间戳等
    /// </summary>
    /// <param name="taskId">任务标识</param>
    /// <param name="stepId">步骤标识</param>
    /// <param name="agentId">代理标识</param>
    /// <param name="pageNum">页码</param>
    /// <returns>系统变量字典</returns>
    Dictionary<string, object?> GetSystemVariables(string? taskId = null, string? stepId = null, string? agentId = null, int? pageNum = null);
}
