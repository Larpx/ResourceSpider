using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface ITaskDispatchService
{
    Task<(bool IsValid, List<TaskDto> Tasks)> PullTasksAsync(string agentId, string agentToken, int maxCount);
    Task<(bool IsValid, TaskDto? Task)> GetTaskContentAsync(string agentId, string agentToken, string taskId);
    Task<bool> ReportTaskAsync(string agentId, string agentToken, string taskId, int status, int dataCount, int duration);
    Task<bool> ReportStepStatusAsync(string agentId, string agentToken, string taskId, string stepId, int state, int dataCount);
    Task<(bool IsValid, ExpressionConfigDto? Expression)> PullExpressionAsync(string agentId, string agentToken, string expressionId);
    Task<(bool IsValid, List<ExpressionConfigDto> Expressions)> PullActiveExpressionsAsync(string agentId, string agentToken);
    Task<bool> StoreResultsAsync(string agentId, string agentToken, StoreCollectionResultsRequest request);
    Task<bool> ReportExpressionAvailabilityAsync(string agentId, string agentToken, string expressionId, bool isAvailable, string? failureReason);
    Task<(bool IsValid, List<StepResourceDto> Resources)> PullStepResourcesAsync(string agentId, string agentToken, string taskId, string stepId, int take);
    Task<(bool IsValid, AgentStatusDto? Status)> GetAgentStatusAsync(string agentId, string agentToken);
    Task<bool> PrefetchTasksAsync(string agentId, string agentToken, int count);
    Task<bool> DispatchTaskAsync(string taskId);
    Task<string?> SelectBestAgentAsync(string? agentGroupId);
}

public class TaskDispatchService : ITaskDispatchService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskStepRepository _taskStepRepository;
    private readonly IAgentRegisterService _agentRegisterService;
    private readonly IExpressionService _expressionService;
    private readonly ICollectionResultService _resultService;
    private readonly IAgentTaskContentCache _taskContentCache;
    private readonly IStepResourcePoolService _resourcePoolService;
    private readonly IStepStateMachineService _stateMachineService;
    private readonly IAgentRepository _agentRepository;
    private readonly ILogger<TaskDispatchService> _logger;

    public TaskDispatchService(
        ITaskRepository taskRepository,
        ITaskStepRepository taskStepRepository,
        IAgentRegisterService agentRegisterService,
        IExpressionService expressionService,
        ICollectionResultService resultService,
        IAgentTaskContentCache taskContentCache,
        IStepResourcePoolService resourcePoolService,
        IStepStateMachineService stateMachineService,
        IAgentRepository agentRepository,
        ILogger<TaskDispatchService> logger)
    {
        _taskRepository = taskRepository;
        _taskStepRepository = taskStepRepository;
        _agentRegisterService = agentRegisterService;
        _expressionService = expressionService;
        _resultService = resultService;
        _taskContentCache = taskContentCache;
        _resourcePoolService = resourcePoolService;
        _stateMachineService = stateMachineService;
        _agentRepository = agentRepository;
        _logger = logger;
    }

    public async Task<(bool IsValid, List<TaskDto> Tasks)> PullTasksAsync(string agentId, string agentToken, int maxCount)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            _logger.LogWarning("Agent {AgentId} Token 无效", agentId);
            return (false, new List<TaskDto>());
        }

        var onlineAgents = await GetOnlineAgentCountAsync();
        var adjustedMaxCount = CalculateTaskAllocation(maxCount, onlineAgents);

        var tasks = await _taskRepository.GetPendingTasksAsync(adjustedMaxCount);
        var result = new List<TaskDto>();

        foreach (var task in tasks)
        {
            task.AssignedAgentId = agentId;
            task.Status = 1;
            task.StartTime = task.StartTime ?? DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task);

            await _stateMachineService.EvaluateStepTransitionsAsync(task.TaskId);

            var dto = await BuildTaskDtoAsync(task);
            result.Add(dto);
            await _taskContentCache.SetAsync(dto);
        }

        _logger.LogInformation("Agent {AgentId} 拉取 {Count} 个任务（在线Agent: {OnlineCount}）", agentId, result.Count, onlineAgents);
        return (true, result);
    }

    public async Task<(bool IsValid, TaskDto? Task)> GetTaskContentAsync(string agentId, string agentToken, string taskId)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            _logger.LogWarning("Agent {AgentId} Token 无效", agentId);
            return (false, null);
        }

        var cached = await _taskContentCache.GetAsync(taskId);
        if (cached != null)
        {
            return (true, cached);
        }

        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
        {
            return (true, null);
        }

        var dto = await BuildTaskDtoAsync(task);
        await _taskContentCache.SetAsync(dto);
        return (true, dto);
    }

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
            await _taskContentCache.RemoveAsync(taskId);
        }

        await _taskRepository.UpdateAsync(task);
        _logger.LogInformation("Agent {AgentId} 上报任务 {TaskId} 状态：{Status}", agentId, taskId, status);

        return true;
    }

    public async Task<bool> ReportStepStatusAsync(string agentId, string agentToken, string taskId, string stepId, int state, int dataCount)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            _logger.LogWarning("Agent {AgentId} Token 无效", agentId);
            return false;
        }

        var step = await _taskStepRepository.GetByIdAsync(stepId);
        if (step == null)
        {
            _logger.LogWarning("步骤 {StepId} 不存在", stepId);
            return false;
        }

        var targetState = (Core.Enums.StepState)state;
        var transitioned = await _stateMachineService.TryTransitionStepStateAsync(taskId, stepId, targetState);

        if (transitioned && targetState == Core.Enums.StepState.Completed)
        {
            await _resourcePoolService.FeedToNextStepsAsync(taskId, stepId);
        }

        _logger.LogInformation("Agent {AgentId} 上报步骤 {StepId} 状态：{State}，数据量：{DataCount}", agentId, stepId, targetState, dataCount);
        return true;
    }

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

    public async Task<bool> StoreResultsAsync(string agentId, string agentToken, StoreCollectionResultsRequest request)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            _logger.LogWarning("Agent {AgentId} Token 无效", agentId);
            return false;
        }

        await _resultService.StoreResultsAsync(
            request.TaskId, request.ExpressionId, agentId, request.Results ?? []);

        _logger.LogInformation(
            "Agent {AgentId} 存储 {Count} 条结果，任务 {TaskId}",
            agentId, request.Results?.Count ?? 0, request.TaskId);
        return true;
    }

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

    public async Task<(bool IsValid, List<StepResourceDto> Resources)> PullStepResourcesAsync(
        string agentId, string agentToken, string taskId, string stepId, int take)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            return (false, new List<StepResourceDto>());
        }

        var resources = await _resourcePoolService.GetAvailableResourcesAsync(taskId, stepId, take);
        var dtos = resources.Select(r => new StepResourceDto(
            r.ResourceId, r.TaskId, r.StepId, r.SourceStepId,
            r.ResourceType, r.Payload, r.SourceUrl, r.Status, r.CreatedAt
        )).ToList();

        _logger.LogInformation("Agent {AgentId} 拉取步骤 {StepId} 的 {Count} 个资源", agentId, stepId, dtos.Count);
        return (true, dtos);
    }

    public async Task<(bool IsValid, AgentStatusDto? Status)> GetAgentStatusAsync(string agentId, string agentToken)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            return (false, null);
        }

        var onlineCount = await GetOnlineAgentCountAsync();
        var busyCount = await GetBusyAgentCountAsync();

        return (true, new AgentStatusDto(onlineCount, busyCount));
    }

    public async Task<bool> PrefetchTasksAsync(string agentId, string agentToken, int count)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid) return false;

        var tasks = await _taskRepository.GetPendingTasksAsync(count);
        foreach (var task in tasks)
        {
            var dto = await BuildTaskDtoAsync(task);
            await _taskContentCache.SetAsync(dto);
        }

        _logger.LogInformation("Agent {AgentId} 预领取 {Count} 个任务内容", agentId, tasks.Count);
        return true;
    }

    private async Task<int> GetOnlineAgentCountAsync()
    {
        var agents = await _agentRepository.GetOnlineAgentsAsync();
        return agents.Count;
    }

    private async Task<int> GetBusyAgentCountAsync()
    {
        var agents = await _agentRepository.GetOnlineAgentsAsync();
        return agents.Count(a => a.Status == 2);
    }

    private static int CalculateTaskAllocation(int requestedCount, int onlineAgentCount)
    {
        if (onlineAgentCount <= 0) return requestedCount;
        var perAgent = Math.Max(1, requestedCount / onlineAgentCount);
        return Math.Min(perAgent, requestedCount);
    }

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

    private async Task<TaskDto> BuildTaskDtoAsync(TaskEntity task)
    {
        var dto = MapToDto(task);

        var steps = await _taskStepRepository.GetByTaskIdAsync(task.TaskId);
        if (steps.Count > 0)
        {
            dto = dto with
            {
                Steps = steps.Select(s => new TaskStepDto(
                    s.StepId, s.TaskId, s.StepOrder, s.StepName, s.CollectionMode,
                    s.AgentGroupId, s.RequestConfig, s.ExtractionRules, s.VariableMappings,
                    s.PaginationConfig, s.OutputConfig, s.StartCondition, s.EndCondition,
                    string.IsNullOrWhiteSpace(s.DependsOnStepIds)
                        ? null
                        : System.Text.Json.JsonSerializer.Deserialize<List<string>>(s.DependsOnStepIds),
                    s.StepConfig, s.State, s.CreatedAt)).ToList()
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

        return dto;
    }

    public async Task<bool> DispatchTaskAsync(string taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
        {
            _logger.LogWarning("调度任务 {TaskId} 不存在", taskId);
            return false;
        }

        if (task.Status != 0)
        {
            _logger.LogWarning("任务 {TaskId} 状态不是待执行: {Status}", taskId, task.Status);
            return false;
        }

        var bestAgentId = await SelectBestAgentAsync(task.AgentGroupId);
        if (bestAgentId == null)
        {
            _logger.LogWarning("任务 {TaskId} 无可用 Agent，加入待分配队列", taskId);
            return false;
        }

        task.AssignedAgentId = bestAgentId;
        task.Status = 1;
        task.StartTime = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task);

        var dto = await BuildTaskDtoAsync(task);
        await _taskContentCache.SetAsync(dto);

        _logger.LogInformation("任务 {TaskId} 已分配给 Agent {AgentId}", taskId, bestAgentId);
        return true;
    }

    public async Task<string?> SelectBestAgentAsync(string? agentGroupId)
    {
        var onlineAgents = await _agentRepository.GetOnlineAgentsAsync();

        if (!string.IsNullOrEmpty(agentGroupId))
        {
            onlineAgents = onlineAgents
                .Where(a => a.GroupId == agentGroupId || string.IsNullOrEmpty(a.GroupId))
                .ToList();
        }

        if (onlineAgents.Count == 0) return null;

        var selected = onlineAgents
            .OrderBy(a => a.TaskCount)
            .ThenByDescending(a => a.OS)
            .FirstOrDefault();

        return selected?.AgentId;
    }
}
