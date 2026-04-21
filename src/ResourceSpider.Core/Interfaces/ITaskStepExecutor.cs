using ResourceSpider.Core.Models;

namespace ResourceSpider.Core.Interfaces;

public interface ITaskStepExecutor
{
    Task<List<DataRecord>> ExecuteStepAsync(TaskStep step, Dictionary<string, object?> inputVariables, CancellationToken ct = default);
}
