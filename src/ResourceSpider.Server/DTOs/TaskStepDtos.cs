using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

/// <summary>
/// 创建任务步骤请求
/// </summary>
/// <param name="StepName">步骤名称，最大长度 100</param>
/// <param name="StepOrder">步骤执行顺序，1-100</param>
/// <param name="CollectionMode">采集模式，最大长度 64</param>
/// <param name="AgentGroupId">代理分组 ID，可选</param>
/// <param name="RequestConfig">请求配置 JSON，可选</param>
/// <param name="ExtractionRules">提取规则 JSON，可选</param>
/// <param name="VariableMappings">变量映射 JSON，可选</param>
/// <param name="PaginationConfig">分页配置 JSON，可选</param>
/// <param name="OutputConfig">输出配置 JSON，可选</param>
public record CreateTaskStepRequest(
    [Required, StringLength(100)] string StepName,
    [Range(1, 100)] int StepOrder,
    [Required, StringLength(64)] string CollectionMode,
    string? AgentGroupId = null,
    string? RequestConfig = null,
    string? ExtractionRules = null,
    string? VariableMappings = null,
    string? PaginationConfig = null,
    string? OutputConfig = null
);

/// <summary>
/// 更新任务步骤请求
/// </summary>
/// <param name="StepName">步骤名称，可选</param>
/// <param name="StepOrder">步骤执行顺序，可选</param>
/// <param name="CollectionMode">采集模式，可选</param>
/// <param name="AgentGroupId">代理分组 ID，可选</param>
/// <param name="RequestConfig">请求配置 JSON，可选</param>
/// <param name="ExtractionRules">提取规则 JSON，可选</param>
/// <param name="VariableMappings">变量映射 JSON，可选</param>
/// <param name="PaginationConfig">分页配置 JSON，可选</param>
/// <param name="OutputConfig">输出配置 JSON，可选</param>
public record UpdateTaskStepRequest(
    string? StepName,
    int? StepOrder,
    string? CollectionMode,
    string? AgentGroupId,
    string? RequestConfig,
    string? ExtractionRules,
    string? VariableMappings,
    string? PaginationConfig,
    string? OutputConfig
);

/// <summary>
/// 任务步骤数据传输对象
/// </summary>
/// <param name="StepId">步骤 ID</param>
/// <param name="TaskId">关联任务 ID</param>
/// <param name="StepOrder">步骤执行顺序</param>
/// <param name="StepName">步骤名称</param>
/// <param name="CollectionMode">采集模式</param>
/// <param name="AgentGroupId">代理分组 ID</param>
/// <param name="RequestConfig">请求配置 JSON</param>
/// <param name="ExtractionRules">提取规则 JSON</param>
/// <param name="VariableMappings">变量映射 JSON</param>
/// <param name="PaginationConfig">分页配置 JSON</param>
/// <param name="OutputConfig">输出配置 JSON</param>
/// <param name="CreatedAt">创建时间</param>
public record TaskStepDto(
    string StepId,
    string TaskId,
    int StepOrder,
    string StepName,
    string CollectionMode,
    string? AgentGroupId,
    string RequestConfig,
    string ExtractionRules,
    string? VariableMappings,
    string? PaginationConfig,
    string? OutputConfig,
    DateTime CreatedAt
);
