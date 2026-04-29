using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ResourceSpider.Core;
using ResourceSpider.Core.Interfaces;

namespace ResourceSpider.Infrastructure.Utils;

public class VariableResolver : IVariableResolver
{
    private readonly ILogger<VariableResolver> _logger;

    private static readonly Regex VariablePattern = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

    public VariableResolver(ILogger<VariableResolver> logger)
    {
        _logger = logger;
    }

    public string Resolve(string template, Dictionary<string, object?> variables)
    {
        if (string.IsNullOrEmpty(template)) return template;

        return VariablePattern.Replace(template, match =>
        {
            var varName = match.Groups[1].Value;

            if (variables.TryGetValue(varName, out var value) && value != null)
            {
                return value.ToString() ?? string.Empty;
            }

            var systemVar = TryResolveSystemVariable(varName, null, null, null, null);
            return systemVar ?? match.Value;
        });
    }

    public Dictionary<string, object?> GetSystemVariables(string? taskId = null, string? stepId = null, string? agentId = null, int? pageNum = null)
    {
        var variables = new Dictionary<string, object?>();

        if (!string.IsNullOrEmpty(taskId))
            variables["TASK_ID"] = taskId;

        if (!string.IsNullOrEmpty(stepId))
            variables["STEP_ID"] = stepId;

        if (!string.IsNullOrEmpty(agentId))
            variables["AGENT_ID"] = agentId;

        variables["TIMESTAMP"] = DateTime.UtcNow.ToString("O");

        if (pageNum.HasValue)
            variables["PAGE_NUM"] = pageNum.Value;

        variables["RANDOM_INT"] = Random.Shared.Next(100000, 999999);
        variables["UUID"] = Guid.NewGuid().ToString();

        return variables;
    }

    private static string? TryResolveSystemVariable(string varName, string? taskId, string? stepId, string? agentId, int? pageNum)
    {
        return varName switch
        {
            "TASK_ID" => taskId,
            "STEP_ID" => stepId,
            "AGENT_ID" => agentId,
            "TIMESTAMP" => DateTime.UtcNow.ToString("O"),
            "PAGE_NUM" => pageNum?.ToString(),
            "RANDOM_INT" => Random.Shared.Next(100000, 999999).ToString(),
            "UUID" => Guid.NewGuid().ToString(),
            _ => null
        };
    }
}
