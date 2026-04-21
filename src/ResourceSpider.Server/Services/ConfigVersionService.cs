using Newtonsoft.Json;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface IConfigVersionService
{
    Task<List<ConfigVersionDto>> GetVersionsAsync(string taskId);
    Task<ConfigVersionDto> CreateVersionAsync(string taskId, string configContent, string? changeDescription = null, string? createdBy = null);
    Task<bool> RollbackAsync(string taskId, int version);
}

public class ConfigVersionService : IConfigVersionService
{
    private readonly IConfigVersionRepository _repository;
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<ConfigVersionService> _logger;

    public ConfigVersionService(
        IConfigVersionRepository repository,
        ITaskRepository taskRepository,
        ILogger<ConfigVersionService> logger)
    {
        _repository = repository;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<List<ConfigVersionDto>> GetVersionsAsync(string taskId)
    {
        var versions = await _repository.GetByTaskIdAsync(taskId);
        return versions.Select(MapToDto).ToList();
    }

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
