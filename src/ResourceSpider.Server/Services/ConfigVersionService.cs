using Newtonsoft.Json;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

/// <summary>
/// 配置版本服务接口，提供任务配置的版本管理和回滚功能
/// </summary>
public interface IConfigVersionService
{
    /// <summary>
    /// 获取指定任务的所有配置版本列表
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <returns>配置版本 DTO 列表</returns>
    Task<List<ConfigVersionDto>> GetVersionsAsync(string taskId);

    /// <summary>
    /// 为指定任务创建新的配置版本
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <param name="configContent">配置内容 JSON 字符串</param>
    /// <param name="changeDescription">变更描述</param>
    /// <param name="createdBy">版本创建者标识</param>
    /// <returns>创建后的配置版本 DTO</returns>
    Task<ConfigVersionDto> CreateVersionAsync(string taskId, string configContent, string? changeDescription = null, string? createdBy = null);

    /// <summary>
    /// 将任务配置回滚到指定版本
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <param name="version">目标版本号</param>
    /// <returns>回滚成功返回 true，版本不存在返回 false</returns>
    Task<bool> RollbackAsync(string taskId, int version);
}

/// <summary>
/// 配置版本服务实现，管理任务配置的版本创建、查询和回滚操作
/// </summary>
public class ConfigVersionService : IConfigVersionService
{
    /// <summary>
    /// 配置版本数据仓库，用于版本实体的持久化操作
    /// </summary>
    private readonly IConfigVersionRepository _repository;

    /// <summary>
    /// 任务数据仓库，用于更新任务的配置版本号
    /// </summary>
    private readonly ITaskRepository _taskRepository;

    /// <summary>
    /// 日志记录器，用于记录配置版本操作相关事件
    /// </summary>
    private readonly ILogger<ConfigVersionService> _logger;

    /// <summary>
    /// 初始化配置版本服务实例
    /// </summary>
    /// <param name="repository">配置版本数据仓库</param>
    /// <param name="taskRepository">任务数据仓库</param>
    /// <param name="logger">日志记录器</param>
    public ConfigVersionService(
        IConfigVersionRepository repository,
        ITaskRepository taskRepository,
        ILogger<ConfigVersionService> logger)
    {
        _repository = repository;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<ConfigVersionDto>> GetVersionsAsync(string taskId)
    {
        var versions = await _repository.GetByTaskIdAsync(taskId);
        return versions.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<ConfigVersionDto> CreateVersionAsync(string taskId, string configContent, string? changeDescription = null, string? createdBy = null)
    {
        var existingVersions = await _repository.GetByTaskIdAsync(taskId);
        var nextVersion = existingVersions.Count > 0 ? existingVersions.Max(v => v.Version) + 1 : 1;

        var entity = new ConfigVersionEntity
        {
            VersionId = Guid.NewGuid().ToString("N"),
            TaskId = taskId,
            Version = nextVersion,
            ConfigContent = configContent,
            ChangeDescription = changeDescription,
            CreatedBy = createdBy
        };

        await _repository.AddAsync(entity);

        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task != null)
        {
            task.ConfigVersion = nextVersion;
            await _taskRepository.UpdateAsync(task);
        }

        _logger.LogInformation("任务 {TaskId} 创建配置版本 {Version}", taskId, nextVersion);
        return MapToDto(entity);
    }

    /// <inheritdoc />
    public async Task<bool> RollbackAsync(string taskId, int version)
    {
        var versionEntity = await _repository.GetByVersionAsync(taskId, version);
        if (versionEntity == null) return false;

        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null) return false;

        task.RequestConfig = versionEntity.ConfigContent;
        task.ConfigVersion = version;
        await _taskRepository.UpdateAsync(task);

        _logger.LogInformation("任务 {TaskId} 回滚到配置版本 {Version}", taskId, version);
        return true;
    }

    /// <summary>
    /// 将配置版本实体映射为配置版本 DTO
    /// </summary>
    /// <param name="entity">配置版本实体</param>
    /// <returns>配置版本 DTO</returns>
    private static ConfigVersionDto MapToDto(ConfigVersionEntity entity)
    {
        return new ConfigVersionDto(
            entity.VersionId,
            entity.TaskId,
            entity.Version,
            entity.ConfigContent,
            entity.ChangeDescription,
            entity.CreatedBy,
            entity.CreatedAt);
    }
}
