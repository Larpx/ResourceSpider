using ResourceSpider.Core.Models;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface ITaskService
{
    Task<TaskDto> CreateAsync(CreateTaskRequest request, string? createdBy = null);
    Task<TaskDto?> GetByIdAsync(string taskId);
    Task<TaskListResponse> GetListAsync(int pageIndex, int pageSize, int? status = null);
    Task<bool> PauseAsync(string taskId);
    Task<bool> ResumeAsync(string taskId);
    Task<bool> DeleteAsync(string taskId);
}

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository taskRepository,
        ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, string? createdBy = null)
    {
        var taskId = Guid.NewGuid().ToString("N");
        var entity = new TaskEntity
        {
            TaskId = taskId,
            TaskName = request.TaskName,
            TaskType = request.TaskType,
            Priority = request.Priority,
            Status = 0,
            RequestConfig = request.RequestConfig ?? "{}",
            ScheduleConfig = request.ScheduleConfig,
            RetryPolicy = request.RetryPolicy,
            CreatedBy = createdBy
        };

        await _taskRepository.AddAsync(entity);
        _logger.LogInformation("Task {TaskId} created: {TaskName}", taskId, request.TaskName);

        return MapToDto(entity);
    }

    public async Task<TaskDto?> GetByIdAsync(string taskId)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<TaskListResponse> GetListAsync(int pageIndex, int pageSize, int? status = null)
    {
        var tasks = await _taskRepository.GetAllAsync(pageIndex, pageSize, status);
        var total = await _taskRepository.CountAsync(status);

        return new TaskListResponse(
            Tasks: tasks.Select(MapToDto).ToList(),
            Total: (int)total,
            PageIndex: pageIndex,
            PageSize: pageSize
        );
    }

    public async Task<bool> PauseAsync(string taskId)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        if (entity == null) return false;

        entity.Status = 4;
        await _taskRepository.UpdateAsync(entity);
        _logger.LogInformation("Task {TaskId} paused", taskId);
        return true;
    }

    public async Task<bool> ResumeAsync(string taskId)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        if (entity == null) return false;

        entity.Status = 1;
        entity.StartTime = entity.StartTime ?? DateTime.UtcNow;
        await _taskRepository.UpdateAsync(entity);
        _logger.LogInformation("Task {TaskId} resumed", taskId);
        return true;
    }

    public async Task<bool> DeleteAsync(string taskId)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        if (entity == null) return false;

        await _taskRepository.DeleteAsync(taskId);
        _logger.LogInformation("Task {TaskId} deleted", taskId);
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
