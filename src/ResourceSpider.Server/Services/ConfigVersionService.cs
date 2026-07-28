using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using Larpx.PersonalTools.ResourceSpider.Server.Repositories;
using System.Text.Json;

namespace Larpx.PersonalTools.ResourceSpider.Server.Services;

/// <summary>
/// 配置版本服务接口，提供任务配置的版本管理、回滚和差异对比功能
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

    /// <summary>
    /// 对比两个版本的配置差异，返回逐字段的变更列表
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <param name="fromVersion">源版本号</param>
    /// <param name="toVersion">目标版本号</param>
    /// <returns>差异项列表，若任一版本不存在返回 null</returns>
    Task<List<ConfigDiffItem>?> DiffAsync(string taskId, int fromVersion, int toVersion);
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
    /// 任务步骤仓储，用于在回滚时恢复完整步骤配置。
    /// </summary>
    private readonly ITaskStepRepository _taskStepRepository;

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
        ITaskStepRepository taskStepRepository,
        ILogger<ConfigVersionService> logger)
    {
        _repository = repository;
        _taskRepository = taskRepository;
        _taskStepRepository = taskStepRepository;
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

        var snapshot = DeserializeSnapshot(versionEntity.ConfigContent);
        if (snapshot == null)
        {
            task.RequestConfig = versionEntity.ConfigContent;
            task.ConfigVersion = version;
            await _taskRepository.UpdateAsync(task);
            _logger.LogInformation("任务 {TaskId} 以兼容模式回滚到旧配置版本 {Version}", taskId, version);
            return true;
        }

        task.TaskName = snapshot.Task.TaskName;
        task.TaskType = snapshot.Task.TaskType;
        task.Priority = snapshot.Task.Priority;
        task.RequestConfig = snapshot.Task.RequestConfig;
        task.ScheduleConfig = snapshot.Task.ScheduleConfig;
        task.RetryPolicy = snapshot.Task.RetryPolicy;
        task.AntiCrawlConfig = snapshot.Task.AntiCrawlConfig;
        task.GlobalConfig = snapshot.Task.GlobalConfig;
        task.Tags = snapshot.Task.Tags;
        task.AgentGroupId = snapshot.Task.AgentGroupId;
        task.ExpressionId = snapshot.Task.ExpressionId;
        task.ConfigVersion = version;
        await _taskRepository.UpdateAsync(task);

        await _taskStepRepository.DeleteByTaskIdAsync(taskId);
        if (snapshot.Steps.Count > 0)
        {
            var steps = snapshot.Steps.Select(step => new TaskStepEntity
            {
                StepId = string.IsNullOrWhiteSpace(step.StepId) ? Guid.NewGuid().ToString("N") : step.StepId,
                TaskId = taskId,
                StepOrder = step.StepOrder,
                StepName = step.StepName,
                CollectionMode = step.CollectionMode,
                AgentGroupId = step.AgentGroupId,
                RequestConfig = step.RequestConfig,
                ExtractionRules = step.ExtractionRules,
                VariableMappings = step.VariableMappings,
                PaginationConfig = step.PaginationConfig,
                OutputConfig = step.OutputConfig,
                StartCondition = step.StartCondition,
                EndCondition = step.EndCondition,
                DependsOnStepIds = step.DependsOnStepIds == null ? null : System.Text.Json.JsonSerializer.Serialize(step.DependsOnStepIds),
                StepConfig = step.StepConfig,
                State = step.State
            }).ToList();

            await _taskStepRepository.AddRangeAsync(steps);
        }

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

    private static TaskConfigurationSnapshot? DeserializeSnapshot(string content)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<TaskConfigurationSnapshot>(content);
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<List<ConfigDiffItem>?> DiffAsync(string taskId, int fromVersion, int toVersion)
    {
        var fromEntity = await _repository.GetByVersionAsync(taskId, fromVersion);
        var toEntity = await _repository.GetByVersionAsync(taskId, toVersion);
        if (fromEntity == null || toEntity == null) return null;

        var fromDict = FlattenJson(fromEntity.ConfigContent);
        var toDict = FlattenJson(toEntity.ConfigContent);

        var diffs = new List<ConfigDiffItem>();

        foreach (var (path, oldValue) in fromDict)
        {
            if (!toDict.TryGetValue(path, out var newValue))
            {
                diffs.Add(new ConfigDiffItem(path, oldValue, null, "Removed"));
            }
            else if (oldValue != newValue)
            {
                diffs.Add(new ConfigDiffItem(path, oldValue, newValue, "Modified"));
            }
        }

        foreach (var (path, newValue) in toDict)
        {
            if (!fromDict.ContainsKey(path))
            {
                diffs.Add(new ConfigDiffItem(path, null, newValue, "Added"));
            }
        }

        return diffs;
    }

    /// <summary>
    /// 将 JSON 字符串展平为点分隔路径的键值字典
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <returns>展平后的键值对字典</returns>
    private static Dictionary<string, string?> FlattenJson(string json)
    {
        var result = new Dictionary<string, string?>();
        try
        {
            using var doc = JsonDocument.Parse(json);
            FlattenElement(doc.RootElement, string.Empty, result);
        }
        catch
        {
            result[string.Empty] = json;
        }
        return result;
    }

    /// <summary>
    /// 递归展平 JsonElement 为点分隔路径的键值对
    /// </summary>
    /// <param name="element">当前 JSON 元素</param>
    /// <param name="prefix">当前路径前缀</param>
    /// <param name="result">结果字典</param>
    private static void FlattenElement(JsonElement element, string prefix, Dictionary<string, string?> result)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var path = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    FlattenElement(property.Value, path, result);
                }
                break;
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    FlattenElement(item, $"{prefix}[{index}]", result);
                    index++;
                }
                break;
            case JsonValueKind.String:
                result[prefix] = element.GetString();
                break;
            case JsonValueKind.Number:
                result[prefix] = element.GetRawText();
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                result[prefix] = element.GetRawText();
                break;
            case JsonValueKind.Null:
                result[prefix] = null;
                break;
            default:
                result[prefix] = element.GetRawText();
                break;
        }
    }
}
