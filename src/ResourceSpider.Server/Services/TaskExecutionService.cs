using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface ITaskExecutionService
{
    Task<TaskExecutionDto> CreateAsync(string taskId, string agentId, string? configSnapshot = null);
    Task<TaskExecutionDto?> GetByIdAsync(string executionId);
    Task<TaskExecutionListResponse> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize);
    Task UpdateStatusAsync(string executionId, int status, string? errorMessage = null);
    Task UpdateProgressAsync(string executionId, int totalPages, int successCount, int failCount);
}

public class TaskExecutionService : ITaskExecutionService
{
    private readonly ITaskExecutionRepository _repository;
    private readonly ILogger<TaskExecutionService> _logger;

    public TaskExecutionService(ITaskExecutionRepository repository, ILogger<TaskExecutionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<TaskExecutionDto> CreateAsync(string taskId, string agentId, string? configSnapshot = null)
    {
        var entity = new TaskExecutionEntity
        {
            ExecutionId = Guid.NewGuid().ToString("N"),
            TaskId = taskId,
            AgentId = agentId,
            Status = 0,
            ConfigSnapshot = configSnapshot,
            StartedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity);
        _logger.LogInformation("创建任务执行记录 {ExecutionId}，任务 {TaskId}，Agent {AgentId}", entity.ExecutionId, taskId, agentId);

        return MapToDto(entity);
    }

    public async Task<TaskExecutionDto?> GetByIdAsync(string executionId)
    {
        var entity = await _repository.GetByIdAsync(executionId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<TaskExecutionListResponse> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize)
    {
        var executions = await _repository.GetByTaskIdAsync(taskId, pageIndex, pageSize);
        var total = await _repository.CountByTaskIdAsync(taskId);

        return new TaskExecutionListResponse(
            executions.Select(MapToDto).ToList(),
            (int)total,
            pageIndex,
            pageSize);
    }

    public async Task UpdateStatusAsync(string executionId, int status, string? errorMessage = null)
    {
        var entity = await _repository.GetByIdAsync(executionId);
        if (entity == null) return;

        entity.Status = status;
        entity.ErrorMessage = errorMessage;

        if (status is 2 or 3 or 4)
        {
            entity.CompletedAt = DateTime.UtcNow;
        }

        await _repository.UpdateAsync(entity);
    }

    public async Task UpdateProgressAsync(string executionId, int totalPages, int successCount, int failCount)
    {
        var entity = await _repository.GetByIdAsync(executionId);
        if (entity == null) return;

        entity.TotalPages = totalPages;
        entity.SuccessCount = successCount;
        entity.FailCount = failCount;

        await _repository.UpdateAsync(entity);
    }

    private static TaskExecutionDto MapToDto(TaskExecutionEntity entity)
    {
        return new TaskExecutionDto(
            entity.ExecutionId,
            entity.TaskId,
            entity.AgentId,
            entity.Status,
            entity.ConfigSnapshot,
            entity.StartedAt,
            entity.CompletedAt,
            entity.TotalPages,
            entity.SuccessCount,
            entity.FailCount,
            entity.ErrorMessage,
            entity.CreatedAt);
    }
}
