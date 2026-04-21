namespace ResourceSpider.Core.Interfaces;

public interface IVariableResolver
{
    string Resolve(string template, Dictionary<string, object?> variables);

    Dictionary<string, object?> GetSystemVariables(string? taskId = null, string? stepId = null, string? agentId = null, int? pageNum = null);
}
