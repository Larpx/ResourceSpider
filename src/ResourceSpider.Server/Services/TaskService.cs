using ResourceSpider.Core.Models;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

/// <summary>
/// 任务服务接口，提供爬虫任务的完整生命周期管理
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// 创建新的爬虫任务，包含任务步骤
    /// </summary>
    /// <param name="request">创建任务请求</param>
    /// <param name="createdBy">任务创建者标识</param>
    /// <returns>创建后的任务 DTO</returns>
    Task<TaskDto> CreateAsync(CreateTaskRequest request, string? createdBy = null);

    /// <summary>
    /// 根据任务标识获取任务详情，包含步骤信息
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <returns>任务 DTO，若不存在返回 null</returns>
    Task<TaskDto?> GetByIdAsync(string taskId);

    /// <summary>
    /// 分页获取任务列表，支持按状态筛选
    /// </summary>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="status">任务状态筛选条件，null 表示不筛选</param>
    /// <returns>任务列表响应</returns>
    Task<TaskListResponse> GetListAsync(int pageIndex, int pageSize, int? status = null);

    /// <summary>
    /// 更新任务配置信息
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <param name="request">更新任务请求</param>
    /// <returns>更新成功返回 true，任务不存在返回 false</returns>
    Task<bool> UpdateAsync(string taskId, UpdateTaskRequest request);

    /// <summary>
    /// 暂停正在运行的任务
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <returns>暂停成功返回 true，任务不存在返回 false</returns>
    Task<bool> PauseAsync(string taskId);

    /// <summary>
    /// 恢复已暂停的任务
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <returns>恢复成功返回 true，任务不存在返回 false</returns>
    Task<bool> ResumeAsync(string taskId);

    /// <summary>
    /// 终止任务执行
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <returns>终止成功返回 true，任务不存在返回 false</returns>
    Task<bool> StopAsync(string taskId);

    /// <summary>
    /// 删除任务及其关联的步骤数据
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <returns>删除成功返回 true，任务不存在返回 false</returns>
    Task<bool> DeleteAsync(string taskId);

    /// <summary>
    /// 触发任务重新执行，重置进度和状态
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <returns>触发成功返回 true，任务不存在返回 false</returns>
    Task<bool> TriggerExecutionAsync(string taskId);
}

/// <summary>
/// 任务服务实现，管理爬虫任务的创建、查询、更新、暂停/恢复/终止及删除等操作
/// </summary>
public class TaskService : ITaskService
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
    /// 日志记录器，用于记录任务操作相关事件
    /// </summary>
    private readonly ILogger<TaskService> _logger;

    /// <summary>
    /// 初始化任务服务实例
    /// </summary>
    /// <param name="taskRepository">任务数据仓库</param>
    /// <param name="taskStepRepository">任务步骤数据仓库</param>
    /// <param name="logger">日志记录器</param>
    public TaskService(
        ITaskRepository taskRepository,
        ITaskStepRepository taskStepRepository,
        ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _taskStepRepository = taskStepRepository;
        _logger = logger;
    }

    /// <inheritdoc />
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

        if (request.Steps != null && request.Steps.Count > 0)
        {
            var stepEntities = request.Steps.Select((s, i) => new TaskStepEntity
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
                OutputConfig = s.OutputConfig
            }).ToList();

            await _taskStepRepository.AddRangeAsync(stepEntities);
        }

        _logger.LogInformation("任务 {TaskId} 创建成功：{TaskName}", taskId, request.TaskName);
        return MapToDto(entity);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<TaskListResponse> GetListAsync(int pageIndex, int pageSize, int? status = null)
    {
        var tasks = await _taskRepository.GetAllAsync(pageIndex, pageSize, status);
        var total = await _taskRepository.CountAsync(status);

        return new TaskListResponse(
            tasks.Select(MapToDto).ToList(),
            (int)total,
            pageIndex,
            pageSize
        );
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(string taskId, UpdateTaskRequest request)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        if (entity == null) return false;

        if (request.TaskName != null) entity.TaskName = request.TaskName;
        if (request.Priority.HasValue) entity.Priority = request.Priority.Value;
        if (request.RequestConfig != null) entity.RequestConfig = request.RequestConfig;
        if (request.ScheduleConfig != null) entity.ScheduleConfig = request.ScheduleConfig;
        if (request.RetryPolicy != null) entity.RetryPolicy = request.RetryPolicy;
        if (request.AntiCrawlConfig != null) entity.AntiCrawlConfig = request.AntiCrawlConfig;
        if (request.GlobalConfig != null) entity.GlobalConfig = request.GlobalConfig;
        if (request.Tags != null) entity.Tags = request.Tags;
        if (request.AgentGroupId != null) entity.AgentGroupId = request.AgentGroupId;

        await _taskRepository.UpdateAsync(entity);
        _logger.LogInformation("任务 {TaskId} 更新成功", taskId);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> PauseAsync(string taskId)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        if (entity == null) return false;

        entity.Status = 4;
        await _taskRepository.UpdateAsync(entity);
        _logger.LogInformation("任务 {TaskId} 已暂停", taskId);
        return true;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string taskId)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId);
        if (entity == null) return false;

        await _taskStepRepository.DeleteByTaskIdAsync(taskId);
        await _taskRepository.DeleteAsync(taskId);
        _logger.LogInformation("任务 {TaskId} 已删除", taskId);
        return true;
    }

    /// <inheritdoc />
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
        _logger.LogInformation("任务 {TaskId} 已触发执行", taskId);
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

    /// <summary>
    /// 将任务步骤实体映射为任务步骤 DTO
    /// </summary>
    /// <param name="entity">任务步骤实体</param>
    /// <returns>任务步骤 DTO</returns>
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
            entity.CreatedAt
        );
    }
}
