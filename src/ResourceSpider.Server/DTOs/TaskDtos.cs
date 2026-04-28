using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

/// <summary>
/// 创建爬虫任务请求
/// </summary>
/// <param name="TaskName">任务名称，最大长度 256</param>
/// <param name="TaskType">任务类型，最大长度 64</param>
/// <param name="Priority">任务优先级，1-10，默认 5</param>
/// <param name="RequestConfig">请求配置 JSON，可选</param>
/// <param name="ScheduleConfig">调度配置 JSON，可选</param>
/// <param name="RetryPolicy">重试策略 JSON，可选</param>
/// <param name="AntiCrawlConfig">反爬配置 JSON，可选</param>
/// <param name="GlobalConfig">全局配置 JSON，可选</param>
/// <param name="Tags">任务标签，可选</param>
/// <param name="AgentGroupId">代理分组 ID，可选</param>
/// <param name="ExpressionId">关联表达式 ID，可选</param>
/// <param name="Steps">任务步骤列表，可选</param>
public record CreateTaskRequest(
    [Required, StringLength(256)] string TaskName,
    [Required, StringLength(64)] string TaskType,
    [Range(1, 10)] int Priority = 5,
    string? RequestConfig = null,
    string? ScheduleConfig = null,
    string? RetryPolicy = null,
    string? AntiCrawlConfig = null,
    string? GlobalConfig = null,
    string? Tags = null,
    string? AgentGroupId = null,
    string? ExpressionId = null,
    List<CreateTaskStepRequest>? Steps = null,
    string? ChangeDescription = null
);

/// <summary>
/// 更新爬虫任务请求
/// </summary>
/// <param name="TaskName">任务名称，可选</param>
/// <param name="Priority">任务优先级，可选</param>
/// <param name="RequestConfig">请求配置 JSON，可选</param>
/// <param name="ScheduleConfig">调度配置 JSON，可选</param>
/// <param name="RetryPolicy">重试策略 JSON，可选</param>
/// <param name="AntiCrawlConfig">反爬配置 JSON，可选</param>
/// <param name="GlobalConfig">全局配置 JSON，可选</param>
/// <param name="Tags">任务标签，可选</param>
/// <param name="AgentGroupId">代理分组 ID，可选</param>
public record UpdateTaskRequest(
    string? TaskName,
    string? TaskType,
    int? Priority,
    string? RequestConfig,
    string? ScheduleConfig,
    string? RetryPolicy,
    string? AntiCrawlConfig,
    string? GlobalConfig,
    string? Tags,
    string? AgentGroupId,
    string? ExpressionId,
    List<CreateTaskStepRequest>? Steps,
    string? ChangeDescription
);

/// <summary>
/// 爬虫任务数据传输对象
/// </summary>
/// <param name="TaskId">任务 ID</param>
/// <param name="TaskName">任务名称</param>
/// <param name="TaskType">任务类型</param>
/// <param name="Priority">优先级</param>
/// <param name="Status">任务状态</param>
/// <param name="RequestConfig">请求配置 JSON</param>
/// <param name="ScheduleConfig">调度配置 JSON</param>
/// <param name="RetryPolicy">重试策略 JSON</param>
/// <param name="AntiCrawlConfig">反爬配置 JSON</param>
/// <param name="GlobalConfig">全局配置 JSON</param>
/// <param name="ConfigVersion">配置版本号</param>
/// <param name="Tags">任务标签</param>
/// <param name="AgentGroupId">代理分组 ID</param>
/// <param name="AssignedAgentId">分配的代理 ID</param>
/// <param name="Progress">执行进度百分比</param>
/// <param name="TotalRequests">总请求数</param>
/// <param name="CompletedRequests">已完成请求数</param>
/// <param name="FailedRequests">失败请求数</param>
/// <param name="StartTime">开始时间</param>
/// <param name="EndTime">结束时间</param>
/// <param name="CreatedBy">创建者</param>
/// <param name="CreatedAt">创建时间</param>
/// <param name="ExpressionId">关联表达式 ID</param>
/// <param name="ExpressionConfig">表达式配置</param>
/// <param name="Steps">任务步骤列表</param>
public record TaskDto(
    string TaskId,
    string TaskName,
    string TaskType,
    int Priority,
    int Status,
    string RequestConfig,
    string? ScheduleConfig,
    string? RetryPolicy,
    string? AntiCrawlConfig,
    string? GlobalConfig,
    int ConfigVersion,
    string? Tags,
    string? AgentGroupId,
    string? AssignedAgentId,
    decimal Progress,
    int TotalRequests,
    int CompletedRequests,
    int FailedRequests,
    DateTime? StartTime,
    DateTime? EndTime,
    string? CreatedBy,
    DateTime CreatedAt,
    string? ExpressionId = null,
    ExpressionConfigDto? ExpressionConfig = null,
    List<TaskStepDto>? Steps = null
);

/// <summary>
/// 任务列表响应，包含分页信息
/// </summary>
/// <param name="Tasks">任务列表</param>
/// <param name="Total">总数</param>
/// <param name="PageIndex">当前页码</param>
/// <param name="PageSize">每页数量</param>
public record TaskListResponse(
    List<TaskDto> Tasks,
    int Total,
    int PageIndex,
    int PageSize
);

/// <summary>
/// 任务配置快照，用于版本记录与回滚。
/// </summary>
public record TaskConfigurationSnapshot(
    TaskConfigurationTaskSnapshot Task,
    List<TaskConfigurationStepSnapshot> Steps
);

/// <summary>
/// 任务级配置快照。
/// </summary>
public record TaskConfigurationTaskSnapshot(
    string TaskId,
    string TaskName,
    string TaskType,
    int Priority,
    string RequestConfig,
    string? ScheduleConfig,
    string? RetryPolicy,
    string? AntiCrawlConfig,
    string? GlobalConfig,
    string? Tags,
    string? AgentGroupId,
    string? ExpressionId
);

/// <summary>
/// 步骤级配置快照。
/// </summary>
public record TaskConfigurationStepSnapshot(
    string StepId,
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
    int State
);
