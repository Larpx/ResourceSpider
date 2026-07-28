using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Larpx.PersonalTools.ResourceSpider.Core;
using Larpx.PersonalTools.ResourceSpider.Core.Enums;
using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Agent.Services;

/// <summary>
/// 离线任务存储接口，定义断网场景下任务结果和检查点的持久化操作
/// </summary>
public interface IOfflineTaskStore
{
    /// <summary>
    /// 保存待上传的执行结果到本地文件
    /// </summary>
    /// <param name="result">任务执行结果</param>
    Task SavePendingResultAsync(ExecutionResult result);

    /// <summary>
    /// 获取所有待上传的执行结果
    /// </summary>
    /// <returns>待上传的执行结果列表</returns>
    Task<List<ExecutionResult>> GetPendingResultsAsync();

    /// <summary>
    /// 标记指定任务的结果已成功上传，删除本地缓存文件
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    Task MarkResultUploadedAsync(string taskId);

    /// <summary>
    /// 保存任务执行检查点，用于断点续传
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="stepId">当前步骤 ID</param>
    /// <param name="dataCount">已采集的数据量</param>
    /// <param name="variables">当前步骤变量字典</param>
    Task SaveTaskCheckpointAsync(string taskId, string stepId, int dataCount, Dictionary<string, object?> variables);

    /// <summary>
    /// 获取指定任务的检查点信息
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <returns>检查点信息，不存在时返回 null</returns>
    Task<TaskCheckpoint?> GetCheckpointAsync(string taskId);

    /// <summary>
    /// 清除指定任务的检查点
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    Task ClearCheckpointAsync(string taskId);

    /// <summary>
    /// 移除指定任务的所有离线数据（结果文件和检查点）
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    void Remove(string taskId);

    /// <summary>
    /// 获取所有离线存储的任务 ID 和内容
    /// </summary>
    /// <returns>任务 ID 与内容的键值对列表</returns>
    List<KeyValuePair<string, string>> GetAll();
}

/// <summary>
/// 离线任务存储实现，使用本地文件系统持久化任务结果和检查点
/// 支持断网场景下的数据恢复和断点续传
/// </summary>
public class OfflineTaskStore : IOfflineTaskStore
{
    /// <summary>
    /// 离线结果文件的存储目录
    /// </summary>
    private readonly string _offlineDir;

    /// <summary>
    /// 检查点文件的存储目录
    /// </summary>
    private readonly string _checkpointDir;

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<OfflineTaskStore> _logger;

    /// <summary>
    /// 初始化离线任务存储，从配置中读取存储路径并创建目录
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="configuration">应用配置，用于读取存储路径</param>
    public OfflineTaskStore(ILogger<OfflineTaskStore> logger, IConfiguration configuration)
    {
        _logger = logger;
        
        // 从配置中读取路径，提供默认值作为回退
        var offlineResultsPath = configuration.GetValue<string>("Agent:OfflineStorage:OfflineResultsPath") ?? "./offline_results";
        var checkpointsPath = configuration.GetValue<string>("Agent:OfflineStorage:CheckpointsPath") ?? "./checkpoints";
        
        // 解析相对路径为绝对路径
        _offlineDir = Path.IsPathRooted(offlineResultsPath) 
            ? offlineResultsPath 
            : Path.Combine(AppContext.BaseDirectory, offlineResultsPath);
        
        _checkpointDir = Path.IsPathRooted(checkpointsPath) 
            ? checkpointsPath 
            : Path.Combine(AppContext.BaseDirectory, checkpointsPath);
        
        Directory.CreateDirectory(_offlineDir);
        Directory.CreateDirectory(_checkpointDir);
        
        _logger.LogInformation("离线存储目录配置：离线结果={OfflineDir}, 检查点={CheckpointDir}", _offlineDir, _checkpointDir);
    }

    /// <summary>
    /// 保存待上传的执行结果到本地 JSON 文件
    /// </summary>
    /// <param name="result">任务执行结果</param>
    public async Task SavePendingResultAsync(ExecutionResult result)
    {
        var filePath = Path.Combine(_offlineDir, $"{result.TaskId}_{DateTime.UtcNow:yyyyMMddHHmmss}.json");
        var json = JsonSerializer.Serialize(result);
        await File.WriteAllTextAsync(filePath, json);
        _logger.LogInformation("离线结果已保存：{FilePath}", filePath);
    }

    /// <summary>
    /// 获取所有待上传的执行结果，从本地文件中读取并反序列化
    /// </summary>
    /// <returns>待上传的执行结果列表</returns>
    public async Task<List<ExecutionResult>> GetPendingResultsAsync()
    {
        var results = new List<ExecutionResult>();
        var files = Directory.GetFiles(_offlineDir, "*.json");

        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var result = JsonSerializer.Deserialize<ExecutionResult>(json);
                if (result != null)
                {
                    results.Add(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "读取离线结果文件失败：{File}", file);
            }
        }

        return results;
    }

    /// <summary>
    /// 标记指定任务的结果已成功上传，删除对应的本地缓存文件
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public Task MarkResultUploadedAsync(string taskId)
    {
        var files = Directory.GetFiles(_offlineDir, $"{taskId}_*.json");
        foreach (var file in files)
        {
            try
            {
                File.Delete(file);
                _logger.LogInformation("已删除已上传的离线结果：{File}", file);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "删除离线结果文件失败：{File}", file);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 保存任务执行检查点，记录当前步骤和数据量，用于断点续传
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="stepId">当前步骤 ID</param>
    /// <param name="dataCount">已采集的数据量</param>
    /// <param name="variables">当前步骤变量字典</param>
    public async Task SaveTaskCheckpointAsync(string taskId, string stepId, int dataCount, Dictionary<string, object?> variables)
    {
        var checkpoint = new TaskCheckpoint
        {
            TaskId = taskId,
            CurrentStepId = stepId,
            DataCount = dataCount,
            Variables = variables,
            SavedAt = DateTime.UtcNow
        };

        var filePath = Path.Combine(_checkpointDir, $"{taskId}.json");
        var json = JsonSerializer.Serialize(checkpoint);
        await File.WriteAllTextAsync(filePath, json);
        _logger.LogInformation("任务 {TaskId} 断点已保存，步骤 {StepId}，数据量 {DataCount}", taskId, stepId, dataCount);
    }

    /// <summary>
    /// 获取指定任务的检查点信息，用于恢复中断的任务
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <returns>检查点信息，不存在时返回 null</returns>
    public async Task<TaskCheckpoint?> GetCheckpointAsync(string taskId)
    {
        var filePath = Path.Combine(_checkpointDir, $"{taskId}.json");
        if (!File.Exists(filePath)) return null;

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<TaskCheckpoint>(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "读取断点文件失败：{File}", filePath);
            return null;
        }
    }

    /// <summary>
    /// 清除指定任务的检查点文件
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public Task ClearCheckpointAsync(string taskId)
    {
        var filePath = Path.Combine(_checkpointDir, $"{taskId}.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("任务 {TaskId} 断点已清除", taskId);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 移除指定任务的所有离线数据，包括结果文件和检查点文件
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    public void Remove(string taskId)
    {
        var files = Directory.GetFiles(_offlineDir, $"{taskId}_*.json");
        foreach (var file in files)
        {
            try { File.Delete(file); } catch { }
        }

        var checkpointFile = Path.Combine(_checkpointDir, $"{taskId}.json");
        if (File.Exists(checkpointFile))
        {
            try { File.Delete(checkpointFile); } catch { }
        }
    }

    /// <summary>
    /// 获取所有离线存储的任务 ID 和内容，用于断网恢复后重新上报
    /// </summary>
    /// <returns>任务 ID 与 JSON 内容的键值对列表</returns>
    public List<KeyValuePair<string, string>> GetAll()
    {
        var result = new List<KeyValuePair<string, string>>();
        var files = Directory.GetFiles(_offlineDir, "*.json");

        foreach (var file in files)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var taskId = fileName.Contains('_') ? fileName.Split('_')[0] : fileName;
                var content = File.ReadAllText(file);
                result.Add(new KeyValuePair<string, string>(taskId, content));
            }
            catch { }
        }

        return result;
    }
}

/// <summary>
/// 任务检查点模型，记录任务执行的中间状态，支持断点续传
/// </summary>
public class TaskCheckpoint
{
    /// <summary>
    /// 任务 ID
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 当前执行的步骤 ID
    /// </summary>
    public string CurrentStepId { get; set; } = string.Empty;

    /// <summary>
    /// 已采集的数据量
    /// </summary>
    public int DataCount { get; set; }

    /// <summary>
    /// 步骤变量字典，保存步骤间传递的数据
    /// </summary>
    public Dictionary<string, object?> Variables { get; set; } = new();

    /// <summary>
    /// 检查点保存时间（UTC）
    /// </summary>
    public DateTime SavedAt { get; set; }
}
