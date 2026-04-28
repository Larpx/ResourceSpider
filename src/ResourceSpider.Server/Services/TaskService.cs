using ResourceSpider.Core.Models;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface ITaskService
{
    Task<TaskDto> CreateAsync(CreateTaskRequest request, string? createdBy = null);
    Task<TaskDto?> GetByIdAsync(string taskId);
    Task<TaskListResponse> GetListAsync(int pageIndex, int pageSize, int? status = null, string? keyword = null);
    Task<bool> UpdateAsync(string taskId, UpdateTaskRequest request);
    Task<bool> PauseAsync(string taskId);
    Task<bool> ResumeAsync(string taskId);
    Task<bool> StopAsync(string taskId);
    Task<bool> DeleteAsync(string taskId);
    Task<bool> TriggerExecutionAsync(string taskId);
    Task<TaskConfigurationSnapshot?> GetConfigurationSnapshotAsync(string taskId);
}

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskStepRepository _taskStepRepository;
    private readonly IConfigVersionService _configVersionService;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository taskRepository,
        ITaskStepRepository taskStepRepository,
        IConfigVersionService configVersionService,
        ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _taskStepRepository = taskStepRepository;
        _configVersionService = configVersionService;
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
            AntiCrawlConfig = request.AntiCrawlConfig,
            GlobalConfig = request.GlobalConfig,
            Tags = request.Tags,
            AgentGroupId = request.AgentGroupId,
            ExpressionId = request.ExpressionId,
            CreatedBy = createdBy
        };

        await _taskRepository.AddAsync(entity);

        var stepEntities = await ReplaceStepsAsync(taskId, request.Steps);

        var createdTask = await BuildTaskDtoAsync(entity, stepEntities);
        await _configVersionService.CreateVersionAsync(
            taskId,
            SerializeConfigurationSnapshot(ToConfigurationSnapshot(createdTask)),
            request.ChangeDescription ?? "创建任务配置",
            createdBy);

        _logger.LogInformation("任务 {TaskId} 创建成功：{TaskName}", taskId, request.TaskName);
        return createdTask;
    }

    public async Task<TaskDto?> GetByIdAsync(string taskId)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        if (entity == null) return null;

        var dto = MapToDto(entity);
        var steps = await _taskStepRepository.GetByTaskIdAsync(taskId);
        if (steps.Count > 0)
        {
            dto = dto with
            {
                Steps = steps.Select(MapStepToDto).ToList()
            };
        }

        return dto;
    }

    public async Task<TaskListResponse> GetListAsync(int pageIndex, int pageSize, int? status = null, string? keyword = null)
    {
        var tasks = await _taskRepository.GetAllAsync(pageIndex, pageSize, status, keyword);
        var total = await _taskRepository.CountAsync(status, keyword);

        return new TaskListResponse(
            tasks.Select(MapToDto).ToList(),
            (int)total,
            pageIndex,
            pageSize
        );
    }

    public async Task<bool> UpdateAsync(string taskId, UpdateTaskRequest request)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        if (entity == null) return false;

        if (request.TaskName != null) entity.TaskName = request.TaskName;
        if (request.TaskType != null) entity.TaskType = request.TaskType;
        if (request.Priority.HasValue) entity.Priority = request.Priority.Value;
        if (request.RequestConfig != null) entity.RequestConfig = request.RequestConfig;
        if (request.ScheduleConfig != null) entity.ScheduleConfig = request.ScheduleConfig;
        if (request.RetryPolicy != null) entity.RetryPolicy = request.RetryPolicy;
        if (request.AntiCrawlConfig != null) entity.AntiCrawlConfig = request.AntiCrawlConfig;
        if (request.GlobalConfig != null) entity.GlobalConfig = request.GlobalConfig;
        if (request.Tags != null) entity.Tags = request.Tags;
        if (request.AgentGroupId != null) entity.AgentGroupId = request.AgentGroupId;
        if (request.ExpressionId != null) entity.ExpressionId = request.ExpressionId;

        await _taskRepository.UpdateAsync(entity);

        List<TaskStepEntity>? stepEntities = null;
        if (request.Steps != null)
        {
            stepEntities = await ReplaceStepsAsync(taskId, request.Steps);
        }

        var updatedTask = await BuildTaskDtoAsync(entity, stepEntities);
        await _configVersionService.CreateVersionAsync(
            taskId,
            SerializeConfigurationSnapshot(ToConfigurationSnapshot(updatedTask)),
            request.ChangeDescription ?? "更新任务配置",
            entity.CreatedBy);

        _logger.LogInformation("任务 {TaskId} 更新成功", taskId);
        return true;
    }

    public async Task<bool> PauseAsync(string taskId)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        if (entity == null) return false;

        entity.Status = 4;
        await _taskRepository.UpdateAsync(entity);
        _logger.LogInformation("任务 {TaskId} 已暂停", taskId);
        return true;
    }

    public async Task<bool> ResumeAsync(string taskId)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        if (entity == null) return false;

        entity.Status = 1;
        entity.StartTime = entity.StartTime ?? DateTime.UtcNow;
        await _taskRepository.UpdateAsync(entity);
        _logger.LogInformation("任务 {TaskId} 已恢复", taskId);
        return true;
    }

    public async Task<bool> StopAsync(string taskId)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        if (entity == null) return false;

        entity.Status = 6;
        entity.EndTime = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(entity);
        _logger.LogInformation("任务 {TaskId} 已终止", taskId);
        return true;
    }

    public async Task<bool> DeleteAsync(string taskId)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        if (entity == null) return false;

        await _taskStepRepository.DeleteByTaskIdAsync(taskId);
        await _taskRepository.DeleteAsync(taskId);
        _logger.LogInformation("任务 {TaskId} 已删除", taskId);
        return true;
    }

    public async Task<bool> TriggerExecutionAsync(string taskId)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        if (entity == null) return false;

        entity.Status = 0;
        entity.StartTime = null;
        entity.EndTime = null;
        entity.Progress = 0;
        entity.CompletedRequests = 0;
        entity.FailedRequests = 0;
        await _taskRepository.UpdateAsync(entity);

        var steps = await _taskStepRepository.GetByTaskIdAsync(taskId);
        if (steps.Count > 0)
        {
            foreach (var step in steps)
            {
                step.State = 0;
                await _taskStepRepository.UpdateAsync(step);
            }

            var firstStep = steps.OrderBy(x => x.StepOrder).First();
            firstStep.State = 1;
            await _taskStepRepository.UpdateAsync(firstStep);
        }

        _logger.LogInformation("任务 {TaskId} 已触发执行", taskId);
        return true;
    }

    public async Task<TaskConfigurationSnapshot?> GetConfigurationSnapshotAsync(string taskId)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        if (entity == null)
        {
            return null;
        }

        var dto = await BuildTaskDtoAsync(entity);
        return ToConfigurationSnapshot(dto);
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

    private async Task<TaskDto> BuildTaskDtoAsync(TaskEntity entity, List<TaskStepEntity>? stepEntities = null)
    {
        var dto = MapToDto(entity);
        var steps = stepEntities ?? await _taskStepRepository.GetByTaskIdAsync(entity.TaskId);
        if (steps.Count > 0)
        {
            dto = dto with
            {
                Steps = steps.Select(MapStepToDto).ToList()
            };
        }

        return dto;
    }

    private async Task<List<TaskStepEntity>> ReplaceStepsAsync(string taskId, List<CreateTaskStepRequest>? steps)
    {
        await _taskStepRepository.DeleteByTaskIdAsync(taskId);

        if (steps == null || steps.Count == 0)
        {
            return [];
        }

        var stepEntities = steps.Select((s, i) => new TaskStepEntity
        {
            StepId = Guid.NewGuid().ToString("N"),
            TaskId = taskId,
            StepOrder = s.StepOrder > 0 ? s.StepOrder : i + 1,
            StepName = s.StepName,
            CollectionMode = s.CollectionMode,
            AgentGroupId = s.AgentGroupId,
            RequestConfig = s.RequestConfig ?? "{}",
            ExtractionRules = s.ExtractionRules ?? "[]",
            VariableMappings = s.VariableMappings,
            PaginationConfig = s.PaginationConfig,
            OutputConfig = s.OutputConfig,
            StartCondition = s.StartCondition,
            EndCondition = s.EndCondition,
            DependsOnStepIds = s.DependsOnStepIds == null ? null : System.Text.Json.JsonSerializer.Serialize(s.DependsOnStepIds),
            StepConfig = s.StepConfig,
            State = 0
        }).OrderBy(s => s.StepOrder).ToList();

        if (stepEntities.Count > 0)
        {
            stepEntities[0].State = 1;
            await _taskStepRepository.AddRangeAsync(stepEntities);
        }

        return stepEntities;
    }

    private static TaskConfigurationSnapshot ToConfigurationSnapshot(TaskDto task)
    {
        var taskSnapshot = new TaskConfigurationTaskSnapshot(
            task.TaskId,
            task.TaskName,
            task.TaskType,
            task.Priority,
            task.RequestConfig,
            task.ScheduleConfig,
            task.RetryPolicy,
            task.AntiCrawlConfig,
            task.GlobalConfig,
            task.Tags,
            task.AgentGroupId,
            task.ExpressionId);

        var stepSnapshots = task.Steps?.Select(step => new TaskConfigurationStepSnapshot(
            step.StepId,
            step.StepOrder,
            step.StepName,
            step.CollectionMode,
            step.AgentGroupId,
            step.RequestConfig,
            step.ExtractionRules,
            step.VariableMappings,
            step.PaginationConfig,
            step.OutputConfig,
            step.StartCondition,
            step.EndCondition,
            step.DependsOnStepIds,
            step.StepConfig,
            step.State)).ToList() ?? [];

        return new TaskConfigurationSnapshot(taskSnapshot, stepSnapshots);
    }

    private static string SerializeConfigurationSnapshot(TaskConfigurationSnapshot snapshot)
    {
        return System.Text.Json.JsonSerializer.Serialize(snapshot);
    }

    private static TaskStepDto MapStepToDto(TaskStepEntity entity)
    {
        return new TaskStepDto(
            entity.StepId,
            entity.TaskId,
            entity.StepOrder,
            entity.StepName,
            entity.CollectionMode,
            entity.AgentGroupId,
            entity.RequestConfig,
            entity.ExtractionRules,
            entity.VariableMappings,
            entity.PaginationConfig,
            entity.OutputConfig,
            entity.StartCondition,
            entity.EndCondition,
            string.IsNullOrWhiteSpace(entity.DependsOnStepIds)
                ? null
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.DependsOnStepIds),
            entity.StepConfig,
            entity.State,
            entity.CreatedAt
        );
    }
}
