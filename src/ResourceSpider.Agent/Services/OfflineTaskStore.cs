using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ResourceSpider.Core;
using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Agent.Services;

public interface IOfflineTaskStore
{
    Task SavePendingResultAsync(ExecutionResult result);

    Task<List<ExecutionResult>> GetPendingResultsAsync();

    Task MarkResultUploadedAsync(string taskId);

    Task SaveTaskCheckpointAsync(string taskId, string stepId, int dataCount, Dictionary<string, object?> variables);

    Task<TaskCheckpoint?> GetCheckpointAsync(string taskId);

    Task ClearCheckpointAsync(string taskId);
}

public class OfflineTaskStore : IOfflineTaskStore
{
    private readonly string _offlineDir;
    private readonly string _checkpointDir;
    private readonly ILogger<OfflineTaskStore> _logger;

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

    public async Task SavePendingResultAsync(ExecutionResult result)
    {
        var filePath = Path.Combine(_offlineDir, $"{result.TaskId}_{DateTime.UtcNow:yyyyMMddHHmmss}.json");
        var json = JsonSerializer.Serialize(result);
        await File.WriteAllTextAsync(filePath, json);
        _logger.LogInformation("离线结果已保存：{FilePath}", filePath);
    }

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
}

public class TaskCheckpoint
{
    public string TaskId { get; set; } = string.Empty;

    public string CurrentStepId { get; set; } = string.Empty;

    public int DataCount { get; set; }

    public Dictionary<string, object?> Variables { get; set; } = new();

    public DateTime SavedAt { get; set; }
}
