using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Core.Interfaces;

/// <summary>
/// 任务步骤执行器接口，定义单个爬取步骤的执行契约
/// </summary>
public interface ITaskStepExecutor
{
    /// <summary>
    /// 执行指定的任务步骤，根据输入变量进行数据采集
    /// </summary>
    /// <param name="step">要执行的任务步骤</param>
    /// <param name="inputVariables">输入变量字典，包含上一步的输出和系统变量</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>步骤执行后提取的数据记录列表</returns>
    Task<List<DataRecord>> ExecuteStepAsync(TaskStep step, Dictionary<string, object?> inputVariables, CancellationToken ct = default);
}
