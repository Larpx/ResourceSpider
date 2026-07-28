using Larpx.PersonalTools.ResourceSpider.Core.Enums;
using Larpx.PersonalTools.ResourceSpider.Core.Models;
using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using Larpx.PersonalTools.ResourceSpider.Server.Repositories;

namespace Larpx.PersonalTools.ResourceSpider.Server.Services;

/// <summary>
/// 任务管理服务接口，定义任务的创建、查询、更新、暂停、恢复、停止和删除操作
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// 创建新任务
    /// </summary>
    /// <param name="request">任务创建请求</param>
    /// <param name="createdBy">创建者用户名</param>
    /// <returns>创建的任务DTO</returns>
    Task<TaskDto> CreateAsync(CreateTaskRequest request, string? createdBy = null);

    /// <summary>
    /// 根据ID获取任务详情
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <returns>任务DTO，不存在返回null</returns>
    Task<TaskDto?> GetByIdAsync(string taskId);

    /// <summary>
    /// 分页获取任务列表
    /// </summary>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="status">状态筛选，可选</param>
    /// <param name="keyword">关键词筛选，可选</param>
    /// <returns>任务列表响应</returns>
    Task<TaskListResponse> GetListAsync(int pageIndex, int pageSize, int? status = null, string? keyword = null);

    /// <summary>
    /// 更新任务配置
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <param name="request">更新请求</param>
    /// <returns>更新是否成功</returns>
    Task<bool> UpdateAsync(string taskId, UpdateTaskRequest request);

    /// <summary>
    /// 暂停任务
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <returns>暂停是否成功</returns>
    Task<bool> PauseAsync(string taskId);

    /// <summary>
    /// 恢复任务
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <returns>恢复是否成功</returns>
    Task<bool> ResumeAsync(string taskId);

    /// <summary>
    /// 停止任务
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <returns>停止是否成功</returns>
    Task<bool> StopAsync(string taskId);

    /// <summary>
    /// 删除任务
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <returns>删除是否成功</returns>
    Task<bool> DeleteAsync(string taskId);

    /// <summary>
    /// 触发任务立即执行
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <returns>触发是否成功</returns>
    Task<bool> TriggerExecutionAsync(string taskId);

    /// <summary>
    /// 获取任务的配置快照
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <returns>配置快照，不存在返回null</returns>
    Task<TaskConfigurationSnapshot?> GetConfigurationSnapshotAsync(string taskId);

    /// <summary>
    /// 获取待调度的任务列表
    /// </summary>
    /// <returns>SpiderTask列表</returns>
    Task<List<SpiderTask>> GetScheduledTasksAsync();
}

/// <summary>
/// 任务管理服务实现，负责任务的CRUD操作和调度触发
/// </summary>
public class TaskService : ITaskService
{
    /// <summary>
    /// 任务仓储
    /// </summary>
    private readonly ITaskRepository _taskRepository;

    /// <summary>
    /// 任务步骤仓储
    /// </summary>
    private readonly ITaskStepRepository _taskStepRepository;

    /// <summary>
    /// 配置版本服务
    /// </summary>
    private readonly IConfigVersionService _configVersionService;

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<TaskService> _logger;

    /// <summary>
    /// 初始化任务服务
    /// </summary>
    /// <param name="taskRepository">任务仓储</param>
    /// <param name="taskStepRepository">任务步骤仓储</param>
    /// <param name="configVersionService">配置版本服务</param>
    /// <param name="logger">日志记录器</param>
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

    /// <summary>
    /// 创建新任务
    /// </summary>
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

    public async Task<List<SpiderTask>> GetScheduledTasksAsync()
    {
        var entities = await _taskRepository.GetPendingTasksAsync(100);
        var tasks = new List<SpiderTask>();

        foreach (var entity in entities)
        {
            var scheduleConfig = !string.IsNullOrEmpty(entity.ScheduleConfig)
                ? System.Text.Json.JsonSerializer.Deserialize<TaskScheduleConfig>(entity.ScheduleConfig)
                : null;

            if (scheduleConfig == null || !scheduleConfig.Enabled) continue;

            var task = new SpiderTask
            {
                TaskId = entity.TaskId,
                TaskName = entity.TaskName,
                TaskType = Enum.TryParse<TaskType>(entity.TaskType, out var t) ? t : TaskType.SinglePage,
                Priority = entity.Priority,
                ScheduleConfig = scheduleConfig,
                AgentGroupId = entity.AgentGroupId
            };

            tasks.Add(task);
        }

        return tasks;
    }
}
