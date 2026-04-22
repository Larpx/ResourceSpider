using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

/// <summary>
/// 任务执行记录服务接口，提供任务执行记录的创建、查询和状态更新功能
/// </summary>
public interface ITaskExecutionService
{
    /// <summary>
    /// 创建新的任务执行记录
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <param name="agentId">执行该任务的 Agent 标识</param>
    /// <param name="configSnapshot">任务配置快照</param>
    /// <returns>创建后的执行记录 DTO</returns>
    Task<TaskExecutionDto> CreateAsync(string taskId, string agentId, string? configSnapshot = null);

    /// <summary>
    /// 根据执行标识获取执行记录详情
    /// </summary>
    /// <param name="executionId">执行记录唯一标识</param>
    /// <returns>执行记录 DTO，若不存在返回 null</returns>
    Task<TaskExecutionDto?> GetByIdAsync(string executionId);

    /// <summary>
    /// 根据任务标识分页查询执行记录
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>执行记录列表响应</returns>
    Task<TaskExecutionListResponse> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize);

    /// <summary>
    /// 更新执行记录的状态
    /// </summary>
    /// <param name="executionId">执行记录唯一标识</param>
    /// <param name="status">执行状态码（0-运行中，1-暂停，2-成功，3-失败，4-取消）</param>
    /// <param name="errorMessage">错误消息（失败时填写）</param>
    Task UpdateStatusAsync(string executionId, int status, string? errorMessage = null);

    /// <summary>
    /// 更新执行记录的进度信息
    /// </summary>
    /// <param name="executionId">执行记录唯一标识</param>
    /// <param name="totalPages">总页数</param>
    /// <param name="successCount">成功请求数</param>
    /// <param name="failCount">失败请求数</param>
    Task UpdateProgressAsync(string executionId, int totalPages, int successCount, int failCount);
}

/// <summary>
/// 任务执行记录服务实现，管理任务执行记录的创建、查询、状态和进度更新
/// </summary>
public class TaskExecutionService : ITaskExecutionService
{
    /// <summary>
    /// 任务执行记录数据仓库，用于执行记录实体的持久化操作
    /// </summary>
    private readonly ITaskExecutionRepository _repository;

    /// <summary>
    /// 日志记录器，用于记录执行记录操作相关事件
    /// </summary>
    private readonly ILogger<TaskExecutionService> _logger;

    /// <summary>
    /// 初始化任务执行记录服务实例
    /// </summary>
    /// <param name="repository">任务执行记录数据仓库</param>
    /// <param name="logger">日志记录器</param>
    public TaskExecutionService(ITaskExecutionRepository repository, ILogger<TaskExecutionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<TaskExecutionDto?> GetByIdAsync(string executionId)
    {
        var entity = await _repository.GetByIdAsync(executionId);
        return entity != null ? MapToDto(entity) : null;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task UpdateProgressAsync(string executionId, int totalPages, int successCount, int failCount)
    {
        var entity = await _repository.GetByIdAsync(executionId);
        if (entity == null) return;

        entity.TotalPages = totalPages;
        entity.SuccessCount = successCount;
        entity.FailCount = failCount;

        await _repository.UpdateAsync(entity);
    }

    /// <summary>
    /// 将任务执行记录实体映射为执行记录 DTO
    /// </summary>
    /// <param name="entity">执行记录实体</param>
    /// <returns>执行记录 DTO</returns>
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
