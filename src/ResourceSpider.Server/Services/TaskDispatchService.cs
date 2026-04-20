using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface ITaskDispatchService
{
    Task<List<TaskDto>> PullTasksAsync(string agentId, string agentToken, int maxCount);
    Task<bool> ReportTaskAsync(string agentId, string taskId, int status, int dataCount, int duration);
}

public class TaskDispatchService : ITaskDispatchService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAgentRegisterService _agentRegisterService;
    private readonly ILogger<TaskDispatchService> _logger;

    public TaskDispatchService(
        ITaskRepository taskRepository,
        IAgentRegisterService agentRegisterService,
        ILogger<TaskDispatchService> logger)
    {
        _taskRepository = taskRepository;
        _agentRegisterService = agentRegisterService;
        _logger = logger;
    }

    public async Task<List<TaskDto>> PullTasksAsync(string agentId, string agentToken, int maxCount)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, agentToken);
        if (!isValid)
        {
            _logger.LogWarning("Invalid token for agent {AgentId}", agentId);
            return new List<TaskDto>();
        }

        var tasks = await _taskRepository.GetPendingTasksAsync(maxCount);
        var result = tasks.Select(MapToDto).ToList();

        foreach (var task in tasks)
        {
            task.AssignedAgentId = agentId;
            task.Status = 1;
            task.StartTime = task.StartTime ?? DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task);
        }

        _logger.LogInformation("Agent {AgentId} pulled {Count} tasks", agentId, result.Count);
        return result;
    }

    public async Task<bool> ReportTaskAsync(string agentId, string taskId, int status, int dataCount, int duration)
    {
        var isValid = await _agentRegisterService.ValidateTokenAsync(agentId, "");
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
