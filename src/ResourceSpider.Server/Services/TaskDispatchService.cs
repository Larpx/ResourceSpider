using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

/// <summary>
/// 任务调度服务接口，提供 Agent 拉取任务、上报状态、拉取表达式及存储结果等调度功能
/// </summary>
public interface ITaskDispatchService
{
    /// <summary>
    /// Agent 拉取待执行的任务列表
    /// </summary>
    /// <param name="agentId">Agent 唯一标识</param>
    /// <param name="agentToken">Agent 认证令牌</param>
    /// <param name="maxCount">最大拉取任务数量</param>
    /// <returns>元组：令牌是否有效 + 任务 DTO 列表</returns>
    Task<(bool IsValid, List<TaskDto> Tasks)> PullTasksAsync(string agentId, string agentToken, int maxCount);

    /// <summary>
    /// Agent 上报任务执行状态和结果数据
    /// </summary>
    /// <param name="agentId">Agent 唯一标识</param>
    /// <param name="agentToken">Agent 认证令牌</param>
    /// <param name="taskId">任务唯一标识</param>
    /// <param name="status">任务状态码</param>
    /// <param name="dataCount">本次采集的数据条数</param>
    /// <param name="duration">执行耗时（毫秒）</param>
    /// <returns>上报成功返回 true</returns>
    Task<bool> ReportTaskAsync(string agentId, string agentToken, string taskId, int status, int dataCount, int duration);

    /// <summary>
    /// Agent 拉取指定表达式的配置信息
    /// </summary>
    /// <param name="agentId">Agent 唯一标识</param>
    /// <param name="agentToken">Agent 认证令牌</param>
    /// <param name="expressionId">表达式唯一标识</param>
    /// <returns>元组：令牌是否有效 + 表达式配置 DTO</returns>
    Task<(bool IsValid, ExpressionConfigDto? Expression)> PullExpressionAsync(string agentId, string agentToken, string expressionId);

    /// <summary>
    /// Agent 拉取所有活跃表达式的配置信息
    /// </summary>
    /// <param name="agentId">Agent 唯一标识</param>
    /// <param name="agentToken">Agent 认证令牌</param>
    /// <returns>元组：令牌是否有效 + 活跃表达式配置列表</returns>
    Task<(bool IsValid, List<ExpressionConfigDto> Expressions)> PullActiveExpressionsAsync(string agentId, string agentToken);

    /// <summary>
    /// Agent 存储采集结果数据
    /// </summary>
    /// <param name="agentId">Agent 唯一标识</param>
    /// <param name="agentToken">Agent 认证令牌</param>
    /// <param name="request">存储采集结果请求</param>
    /// <returns>存储成功返回 true</returns>
    Task<bool> StoreResultsAsync(string agentId, string agentToken, StoreCollectionResultsRequest request);

    /// <summary>
    /// Agent 上报表达式可用性检测结果
    /// </summary>
    /// <param name="agentId">Agent 唯一标识</param>
    /// <param name="agentToken">Agent 认证令牌</param>
    /// <param name="expressionId">表达式唯一标识</param>
    /// <param name="isAvailable">表达式是否可用</param>
    /// <param name="failureReason">不可用时的失败原因</param>
    /// <returns>上报成功返回 true</returns>
    Task<bool> ReportExpressionAvailabilityAsync(string agentId, string agentToken, string expressionId, bool isAvailable, string? failureReason);
}

/// <summary>
/// 任务调度服务实现，协调 Agent 与 Server 之间的任务分发、状态上报和数据存储
/// </summary>
public class TaskDispatchService : ITaskDispatchService
{
    /// <summary>
    /// 任务数据仓库，用于任务实体的持久化操作
    /// </summary>
    private readonly ITaskRepository _taskRepository;

    /// <summary>
    /// 任务步骤数据仓库，用于任务步骤的持久化操作
    /// </summary>
    private readonly ITaskStepRepository _taskStepRepository;

    /// <summary>
    /// Agent 注册服务，用于验证 Agent 令牌
    /// </summary>
    private readonly IAgentRegisterService _agentRegisterService;

    /// <summary>
    /// 表达式服务，用于获取表达式配置
    /// </summary>
    private readonly IExpressionService _expressionService;

    /// <summary>
    /// 采集结果服务，用于存储采集数据
    /// </summary>
    private readonly ICollectionResultService _resultService;

    /// <summary>
    /// 日志记录器，用于记录任务调度相关事件
    /// </summary>
    private readonly ILogger<TaskDispatchService> _logger;

    /// <summary>
    /// 初始化任务调度服务实例
    /// </summary>
    /// <param name="taskRepository">任务数据仓库</param>
    /// <param name="taskStepRepository">任务步骤数据仓库</param>
    /// <param name="agentRegisterService">Agent 注册服务</param>
    /// <param name="expressionService">表达式服务</param>
    /// <param name="resultService">采集结果服务</param>
    /// <param name="logger">日志记录器</param>
    public TaskDispatchService(
        ITaskRepository taskRepository,
        ITaskStepRepository taskStepRepository,
        IAgentRegisterService agentRegisterService,
        IExpressionService expressionService,
        ICollectionResultService resultService,
        ILogger<TaskDispatchService> logger)
    {
        _taskRepository = taskRepository;
        _taskStepRepository = taskStepRepository;
        _agentRegisterService = agentRegisterService;
        _expressionService = expressionService;
        _resultService = resultService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<(bool IsValid, List<TaskDto> Tasks)> PullTasksAsync(string agentId, string agentToken, int maxCount)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            _logger.LogWarning("Agent {AgentId} Token 无效", agentId);
            return (false, new List<TaskDto>());
        }

        var tasks = await _taskRepository.GetPendingTasksAsync(maxCount);
        var result = new List<TaskDto>();

        foreach (var task in tasks)
        {
            task.AssignedAgentId = agentId;
            task.Status = 1;
            task.StartTime = task.StartTime ?? DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task);

            var dto = MapToDto(task);

            var steps = await _taskStepRepository.GetByTaskIdAsync(task.TaskId);
            if (steps.Count > 0)
            {
                dto = dto with
                {
                    Steps = steps.Select(s => new TaskStepDto(
                        s.StepId, s.TaskId, s.StepOrder, s.StepName, s.CollectionMode,
                        s.AgentGroupId, s.RequestConfig, s.ExtractionRules, s.VariableMappings,
                        s.PaginationConfig, s.OutputConfig, s.CreatedAt)).ToList()
                };
            }

            if (!string.IsNullOrEmpty(task.ExpressionId))
            {
                try
                {
                    dto = dto with { ExpressionConfig = await _expressionService.GetConfigAsync(task.ExpressionId) };
                }
                catch (KeyNotFoundException ex)
                {
                    _logger.LogWarning(ex, "表达式 {ExpressionId} 未找到，任务 {TaskId}", task.ExpressionId, task.TaskId);
                }
            }

            result.Add(dto);
        }

        _logger.LogInformation("Agent {AgentId} 拉取 {Count} 个任务", agentId, result.Count);
        return (true, result);
    }

    /// <inheritdoc />
    public async Task<bool> ReportTaskAsync(string agentId, string agentToken, string taskId, int status, int dataCount, int duration)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            _logger.LogWarning("Agent {AgentId} Token 无效", agentId);
            return false;
        }

        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
        {
            _logger.LogWarning("任务 {TaskId} 不存在", taskId);
            return false;
        }

        task.Status = status;
        task.CompletedRequests += dataCount;

        if (status == 3)
        {
            task.FailedRequests++;
        }

        task.Progress = task.TotalRequests > 0
            ? (decimal)(task.CompletedRequests + task.FailedRequests) / task.TotalRequests * 100
            : 100;

        if (status == 2 || status == 3)
        {
            task.EndTime = DateTime.UtcNow;
        }

        await _taskRepository.UpdateAsync(task);
        _logger.LogInformation("Agent {AgentId} 上报任务 {TaskId} 状态：{Status}", agentId, taskId, status);

        return true;
    }

    /// <inheritdoc />
    public async Task<(bool IsValid, ExpressionConfigDto? Expression)> PullExpressionAsync(
        string agentId, string agentToken, string expressionId)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            return (false, null);
        }

        try
        {
            var config = await _expressionService.GetConfigAsync(expressionId);
            _logger.LogInformation("Agent {AgentId} 拉取表达式 {ExpressionId}", agentId, expressionId);
            return (true, config);
        }
        catch (KeyNotFoundException)
        {
            return (true, null);
        }
    }

    /// <inheritdoc />
    public async Task<(bool IsValid, List<ExpressionConfigDto> Expressions)> PullActiveExpressionsAsync(
        string agentId, string agentToken)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            return (false, new List<ExpressionConfigDto>());
        }

        var expressions = await _expressionService.GetActiveExpressionsAsync();
        _logger.LogInformation("Agent {AgentId} 拉取 {Count} 个活跃表达式", agentId, expressions.Count);
        return (true, expressions);
    }

    /// <inheritdoc />
    public async Task<bool> StoreResultsAsync(string agentId, string agentToken, StoreCollectionResultsRequest request)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            _logger.LogWarning("Agent {AgentId} Token 无效", agentId);
            return false;
        }

        await _resultService.StoreResultsAsync(
            request.TaskId, request.ExpressionId, agentId, request.Results);

        _logger.LogInformation(
            "Agent {AgentId} 存储 {Count} 条结果，任务 {TaskId}",
            agentId, request.Results.Count, request.TaskId);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ReportExpressionAvailabilityAsync(
        string agentId, string agentToken, string expressionId, bool isAvailable, string? failureReason)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            return false;
        }

        await _expressionService.ReportAvailabilityAsync(expressionId, agentId, isAvailable, failureReason);
        return true;
    }

    /// <summary>
    /// 将任务实体映射为任务 DTO
    /// </summary>
    /// <param name="entity">任务实体</param>
    /// <returns>任务 DTO</returns>
    private static TaskDto MapToDto(TaskEntity entity)
    {
        return new TaskDto(
            entity.TaskId,
            entity.TaskName,
            entity.TaskType,
            entity.Priority,
            entity.Status,
            entity.RequestConfig,
            entity.ScheduleConfig,
            entity.RetryPolicy,
            entity.AntiCrawlConfig,
            entity.GlobalConfig,
            entity.ConfigVersion,
            entity.Tags,
            entity.AgentGroupId,
            entity.AssignedAgentId,
            entity.Progress,
            entity.TotalRequests,
            entity.CompletedRequests,
            entity.FailedRequests,
            entity.StartTime,
            entity.EndTime,
            entity.CreatedBy,
            entity.CreatedAt,
            entity.ExpressionId
        );
    }
}
