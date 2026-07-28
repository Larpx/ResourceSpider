using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Larpx.PersonalTools.ResourceSpider.Agent.Config;
using Larpx.PersonalTools.ResourceSpider.Agent.Services;
using Larpx.PersonalTools.ResourceSpider.Core;
using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Agent.Modes;

/// <summary>
/// 本地模式运行器，继承 BackgroundService，用于本地独立运行爬虫任务
/// 从本地目录加载 JSON 任务配置，执行后按配置格式保存结果到本地文件
/// </summary>
public class LocalModeRunner : BackgroundService
{
    /// <summary>
    /// 任务执行器
    /// </summary>
    private readonly ITaskExecutor _taskExecutor;

    /// <summary>
    /// Agent 配置选项
    /// </summary>
    private readonly AgentOptions _agentOptions;

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<LocalModeRunner> _logger;

    /// <summary>
    /// 初始化本地模式运行器
    /// </summary>
    /// <param name="taskExecutor">任务执行器</param>
    /// <param name="agentOptions">Agent 配置选项</param>
    /// <param name="logger">日志记录器</param>
    public LocalModeRunner(
        ITaskExecutor taskExecutor,
        IOptions<AgentOptions> agentOptions,
        ILogger<LocalModeRunner> logger)
    {
        _taskExecutor = taskExecutor;
        _agentOptions = agentOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// 后台服务主循环，加载本地任务并顺序执行
    /// </summary>
    /// <param name="stoppingToken">取消令牌</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Local 模式启动，任务配置目录: {TaskDir}", _agentOptions.LocalMode?.TaskDirectory);

        try
        {
            var tasks = LoadLocalTasks();
            if (tasks.Count == 0)
            {
                _logger.LogWarning("未找到本地任务配置");
                return;
            }

            foreach (var task in tasks)
            {
                if (stoppingToken.IsCancellationRequested) break;

                _logger.LogInformation("执行本地任务: {TaskName}", task.TaskName);
                task.AssignedAgentId = $"{Constants.Agent.LocalAgentIdPrefix}{Environment.MachineName}";

                var result = await _taskExecutor.ExecuteAsync(task, stoppingToken);

                await SaveResultAsync(task, result);
            }

            _logger.LogInformation("所有本地任务执行完成");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("本地任务执行被取消");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "本地任务执行出错");
        }
    }

    /// <summary>
    /// 从本地目录加载所有 JSON 任务配置文件
    /// </summary>
    /// <returns>加载的爬虫任务列表</returns>
    private List<SpiderTask> LoadLocalTasks()
    {
        var tasks = new List<SpiderTask>();
        var taskDir = _agentOptions.LocalMode?.TaskDirectory;

        if (string.IsNullOrEmpty(taskDir) || !Directory.Exists(taskDir))
        {
            _logger.LogWarning("任务配置目录不存在: {TaskDir}", taskDir);
            return tasks;
        }

        foreach (var file in Directory.GetFiles(taskDir, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var task = JsonSerializer.Deserialize<SpiderTask>(json);
                if (task != null)
                {
                    tasks.Add(task);
                    _logger.LogInformation("加载任务: {TaskName} 来自 {File}", task.TaskName, Path.GetFileName(file));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "加载任务配置失败: {File}", file);
            }
        }

        return tasks;
    }

    /// <summary>
    /// 保存执行结果到本地文件，根据配置格式选择 JSON、CSV 或 TXT 格式
    /// </summary>
    /// <param name="task">爬虫任务</param>
    /// <param name="result">执行结果</param>
    private async Task SaveResultAsync(SpiderTask task, ExecutionResult result)
    {
        var outputDir = BuildOutputDirectory(task);
        Directory.CreateDirectory(outputDir);

        var format = _agentOptions.LocalMode?.OutputFormat ?? Constants.Defaults.DefaultOutputFormat;
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        switch (format.ToLowerInvariant())
        {
            case "json":
                await SaveAsJsonAsync(task, result, outputDir, timestamp);
                break;
            case "csv":
                await SaveAsCsvAsync(task, result, outputDir, timestamp);
                break;
            case "txt":
            default:
                await SaveAsTxtAsync(task, result, outputDir, timestamp);
                break;
        }

        _logger.LogInformation("结果已保存到: {OutputDir}", outputDir);
    }

    /// <summary>
    /// 构建输出目录路径，格式为：基础目录/AgentID/日期/任务ID
    /// </summary>
    /// <param name="task">爬虫任务</param>
    /// <returns>输出目录的绝对路径</returns>
    private string BuildOutputDirectory(SpiderTask task)
    {
        var baseDir = _agentOptions.LocalMode?.OutputDirectory ?? "results";
        var agentId = $"{Constants.Agent.LocalAgentIdPrefix}{Environment.MachineName}";
        var dateDir = DateTime.UtcNow.ToString("yyyyMMdd");

        return Path.Combine(baseDir, agentId, dateDir, task.TaskId);
    }

    /// <summary>
    /// 保存结果为 JSON 格式，包含完整的数据记录和元信息
    /// </summary>
    /// <param name="task">爬虫任务</param>
    /// <param name="result">执行结果</param>
    /// <param name="outputDir">输出目录</param>
    /// <param name="timestamp">时间戳字符串</param>
    private static async Task SaveAsJsonAsync(SpiderTask task, ExecutionResult result, string outputDir, string timestamp)
    {
        var outputFile = Path.Combine(outputDir, $"result_{timestamp}.json");

        var output = new
        {
            task.TaskId,
            task.TaskName,
            result.Status,
            result.TotalRequests,
            result.SuccessRequests,
            result.FailedRequests,
            result.Duration,
            StartTime = result.StartTime.ToString("O"),
            EndTime = result.EndTime?.ToString("O"),
            DataCount = result.DataRecords.Count,
            Records = result.DataRecords.Select(r => new
            {
                r.SourceUrl,
                Fields = r.Fields,
                ExtractedAt = r.ExtractedAt.ToString("O")
            }),
            Errors = result.Errors
        };

        var json = JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(outputFile, json);
    }

    /// <summary>
    /// 保存结果为 CSV 格式，每行一条数据记录
    /// </summary>
    /// <param name="task">爬虫任务</param>
    /// <param name="result">执行结果</param>
    /// <param name="outputDir">输出目录</param>
    /// <param name="timestamp">时间戳字符串</param>
    private static async Task SaveAsCsvAsync(SpiderTask task, ExecutionResult result, string outputDir, string timestamp)
    {
        var outputFile = Path.Combine(outputDir, $"result_{timestamp}.csv");

        if (result.DataRecords.Count == 0)
        {
            await File.WriteAllTextAsync(outputFile, string.Empty);
            return;
        }

        var allKeys = result.DataRecords
            .SelectMany(r => r.Fields.Keys)
            .Distinct()
            .ToList();

        var header = string.Join(",", allKeys.Select(EscapeCsvField));
        var sb = new StringBuilder();
        sb.AppendLine(header);

        foreach (var record in result.DataRecords)
        {
            var values = allKeys.Select(key =>
            {
                record.Fields.TryGetValue(key, out var value);
                return EscapeCsvField(value?.ToString() ?? string.Empty);
            });
            sb.AppendLine(string.Join(",", values));
        }

        await File.WriteAllTextAsync(outputFile, sb.ToString());
    }

    /// <summary>
    /// 保存结果为 TXT 格式，包含任务概要和详细数据记录
    /// </summary>
    /// <param name="task">爬虫任务</param>
    /// <param name="result">执行结果</param>
    /// <param name="outputDir">输出目录</param>
    /// <param name="timestamp">时间戳字符串</param>
    private static async Task SaveAsTxtAsync(SpiderTask task, ExecutionResult result, string outputDir, string timestamp)
    {
        var outputFile = Path.Combine(outputDir, $"result_{timestamp}.txt");

        var sb = new StringBuilder();
        sb.AppendLine($"任务: {task.TaskName}");
        sb.AppendLine($"状态: {result.Status}");
        sb.AppendLine($"总请求数: {result.TotalRequests}");
        sb.AppendLine($"成功: {result.SuccessRequests}");
        sb.AppendLine($"失败: {result.FailedRequests}");
        sb.AppendLine($"耗时: {result.Duration}ms");
        sb.AppendLine($"数据量: {result.DataRecords.Count}");
        sb.AppendLine(new string('-', 80));

        foreach (var record in result.DataRecords)
        {
            sb.AppendLine($"来源: {record.SourceUrl}");
            foreach (var field in record.Fields)
            {
                sb.AppendLine($"  {field.Key}: {field.Value}");
            }
            sb.AppendLine();
        }

        if (result.Errors.Count > 0)
        {
            sb.AppendLine("错误:");
            foreach (var error in result.Errors)
            {
                sb.AppendLine($"  - {error}");
            }
        }

        await File.WriteAllTextAsync(outputFile, sb.ToString());
    }

    /// <summary>
    /// 转义 CSV 字段值，对包含逗号、引号或换行的字段添加双引号包裹
    /// </summary>
    /// <param name="field">原始字段值</param>
    /// <returns>转义后的字段值</returns>
    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
