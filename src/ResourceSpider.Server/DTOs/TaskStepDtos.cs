using System.ComponentModel.DataAnnotations;

namespace Larpx.PersonalTools.ResourceSpider.Server.DTOs;

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
/// <param name="StartCondition">开始条件，最大长度 256，可选</param>
/// <param name="EndCondition">结束条件，最大长度 256，可选</param>
/// <param name="DependsOnStepIds">依赖的步骤 ID 列表，可选</param>
/// <param name="StepConfig">步骤配置 JSON，可选</param>
public record CreateTaskStepRequest(
    [Required, StringLength(100)] string StepName,
    [Range(1, 100)] int StepOrder,
    [Required, StringLength(64)] string CollectionMode,
    string? AgentGroupId = null,
    string? RequestConfig = null,
    string? ExtractionRules = null,
    string? VariableMappings = null,
    string? PaginationConfig = null,
    string? OutputConfig = null,
    [StringLength(256)] string? StartCondition = null,
    [StringLength(256)] string? EndCondition = null,
    List<string>? DependsOnStepIds = null,
    string? StepConfig = null
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
/// <param name="StartCondition">开始条件，最大长度 256，可选</param>
/// <param name="EndCondition">结束条件，最大长度 256，可选</param>
/// <param name="DependsOnStepIds">依赖的步骤 ID 列表，可选</param>
/// <param name="StepConfig">步骤配置 JSON，可选</param>
/// <param name="State">步骤状态，可选</param>
public record UpdateTaskStepRequest(
    string? StepName,
    int? StepOrder,
    string? CollectionMode,
    string? AgentGroupId,
    string? RequestConfig,
    string? ExtractionRules,
    string? VariableMappings,
    string? PaginationConfig,
    string? OutputConfig,
    string? StartCondition,
    string? EndCondition,
    List<string>? DependsOnStepIds,
    string? StepConfig,
    int? State
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
/// <param name="StartCondition">开始条件</param>
/// <param name="EndCondition">结束条件</param>
/// <param name="DependsOnStepIds">依赖的步骤 ID 列表</param>
/// <param name="StepConfig">步骤配置 JSON</param>
/// <param name="State">步骤状态</param>
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
    string? StartCondition,
    string? EndCondition,
    List<string>? DependsOnStepIds,
    string? StepConfig,
    int State,
    DateTime CreatedAt,
    string? StepType = null
);
