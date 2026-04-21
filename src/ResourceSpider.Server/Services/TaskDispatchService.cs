using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Services;

public interface ITaskDispatchService
{
    Task<(bool IsValid, List<TaskDto> Tasks)> PullTasksAsync(string agentId, string agentToken, int maxCount);
    Task<bool> ReportTaskAsync(string agentId, string agentToken, string taskId, int status, int dataCount, int duration);
    Task<(bool IsValid, ExpressionConfigDto? Expression)> PullExpressionAsync(string agentId, string agentToken, string expressionId);
    Task<(bool IsValid, List<ExpressionConfigDto> Expressions)> PullActiveExpressionsAsync(string agentId, string agentToken);
    Task<bool> StoreResultsAsync(string agentId, string agentToken, StoreCollectionResultsRequest request);
    Task<bool> ReportExpressionAvailabilityAsync(string agentId, string agentToken, string expressionId, bool isAvailable, string? failureReason);
}

public class TaskDispatchService : ITaskDispatchService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAgentRegisterService _agentRegisterService;
    private readonly IExpressionService _expressionService;
    private readonly ICollectionResultService _resultService;
    private readonly ILogger<TaskDispatchService> _logger;

    public TaskDispatchService(
        ITaskRepository taskRepository,
        IAgentRegisterService agentRegisterService,
        IExpressionService expressionService,
        ICollectionResultService resultService,
        ILogger<TaskDispatchService> logger)
    {
        _taskRepository = taskRepository;
        _agentRegisterService = agentRegisterService;
        _expressionService = expressionService;
        _resultService = resultService;
        _logger = logger;
    }

    public async Task<(bool IsValid, List<TaskDto> Tasks)> PullTasksAsync(string agentId, string agentToken, int maxCount)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            _logger.LogWarning("Invalid token for agent {AgentId}", agentId);
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

            if (!string.IsNullOrEmpty(task.ExpressionId))
            {
                try
                {
                    dto = dto with { ExpressionConfig = await _expressionService.GetConfigAsync(task.ExpressionId) };
                }
                catch (KeyNotFoundException ex)
                {
                    _logger.LogWarning(ex, "Expression {ExpressionId} not found for task {TaskId}",
                        task.ExpressionId, task.TaskId);
                }
            }

            result.Add(dto);
        }

        _logger.LogInformation("Agent {AgentId} pulled {Count} tasks", agentId, result.Count);
        return (true, result);
    }

    public async Task<bool> ReportTaskAsync(string agentId, string agentToken, string taskId, int status, int dataCount, int duration)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            _logger.LogWarning("Invalid token for agent {AgentId}", agentId);
            return false;
        }

        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found", taskId);
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
        _logger.LogInformation("Agent {AgentId} reported task {TaskId} status: {Status}",
            agentId, taskId, status);

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
            _logger.LogInformation("Agent {AgentId} pulled expression {ExpressionId}", agentId, expressionId);
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
        _logger.LogInformation("Agent {AgentId} pulled {Count} active expressions", agentId, expressions.Count);
        return (true, expressions);
    }

    public async Task<bool> StoreResultsAsync(string agentId, string agentToken, StoreCollectionResultsRequest request)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            _logger.LogWarning("Invalid token for agent {AgentId}", agentId);
            return false;
        }

        await _resultService.StoreResultsAsync(
            request.TaskId, request.ExpressionId, agentId, request.Results);

        _logger.LogInformation(
            "Agent {AgentId} stored {Count} results for task {TaskId}",
            agentId, request.Results.Count, request.TaskId);
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

    private static TaskDto MapToDto(TaskEntity entity)
    {
        return new TaskDto(
            TaskId: entity.TaskId,
            TaskName: entity.TaskName,
            TaskType: entity.TaskType,
            Priority: entity.Priority,
            Status: entity.Status,
            RequestConfig: entity.RequestConfig,
            ScheduleConfig: entity.ScheduleConfig,
            RetryPolicy: entity.RetryPolicy,
            AssignedAgentId: entity.AssignedAgentId,
            Progress: entity.Progress,
            TotalRequests: entity.TotalRequests,
            CompletedRequests: entity.CompletedRequests,
            FailedRequests: entity.FailedRequests,
            StartTime: entity.StartTime,
            EndTime: entity.EndTime,
            CreatedBy: entity.CreatedBy,
            CreatedAt: entity.CreatedAt
        );
    }
}
