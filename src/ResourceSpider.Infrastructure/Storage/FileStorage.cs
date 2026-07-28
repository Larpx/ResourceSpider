using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Larpx.PersonalTools.ResourceSpider.Core.Interfaces;
using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Infrastructure.Storage;

/// <summary>
/// 文件存储配置选项，定义输出路径、格式和采集元数据信息
/// </summary>
public class FileStorageOptions
{
    /// <summary>
    /// 输出目录路径，默认为 "./results"
    /// </summary>
    public string OutputPath { get; set; } = "./results";

    /// <summary>
    /// 输出文件格式，支持 csv、txt、json，默认为 csv
    /// </summary>
    public string Format { get; set; } = "csv";

    /// <summary>
    /// 代理节点唯一标识
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// 代理节点名称
    /// </summary>
    public string AgentName { get; set; } = string.Empty;

    /// <summary>
    /// 主机名称
    /// </summary>
    public string HostName { get; set; } = string.Empty;

    /// <summary>
    /// 运行模式（Local 或 Online）
    /// </summary>
    public string Mode { get; set; } = "Local";

    /// <summary>
    /// 关联的任务标识
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的任务名称
    /// </summary>
    public string TaskName { get; set; } = string.Empty;
}

/// <summary>
/// 文件存储实现，将采集数据记录持久化到本地文件系统
/// 支持 CSV、TXT、JSON 三种输出格式，按日期和代理 ID 组织目录结构
/// </summary>
public class FileStorage : IStorage
{
    /// <summary>
    /// 文件存储配置选项
    /// </summary>
    private readonly FileStorageOptions _options;

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<FileStorage> _logger;

    /// <summary>
    /// 输出目录路径，由 OutputPath/AgentId/日期 组合而成
    /// </summary>
    private readonly string _outputDirectory;

    /// <summary>
    /// 初始化文件存储实例
    /// </summary>
    /// <param name="options">文件存储配置选项</param>
    /// <param name="logger">日志记录器</param>
    public FileStorage(
        IOptions<FileStorageOptions> options, 
        ILogger<FileStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
        _outputDirectory = Path.Combine(
            _options.OutputPath, 
            _options.AgentId, 
            DateTime.UtcNow.ToString("yyyy-MM-dd"));
    }

    /// <summary>
    /// 处理数据上下文，如果包含数据记录则执行存储操作
    /// </summary>
    /// <param name="context">数据上下文，包含待存储的数据记录</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    public Task HandleAsync(DataContext context, CancellationToken ct = default)
    {
        if (context?.DataRecords.Any() == true)
        {
            return StoreAsync(context.DataRecords, ct);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 将数据记录集合存储到文件，根据配置的格式选择对应的存储方法
    /// </summary>
    /// <param name="records">待存储的数据记录集合</param>
    /// <param name="ct">取消令牌</param>
    /// <exception cref="ArgumentException">当配置了不支持的输出格式时抛出</exception>
    public async Task StoreAsync(IEnumerable<DataRecord> records, CancellationToken ct = default)
    {
        var recordList = records.ToList();
        if (!recordList.Any()) return;

        ApplyTaskMetadata(recordList);
        Directory.CreateDirectory(_outputDirectory);
        
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var fileName = $"{_options.AgentId}_{_options.TaskId}_{timestamp}";

        switch (_options.Format.ToLower())
        {
            case "csv":
                await SaveAsCsvAsync(recordList, fileName, ct);
                break;
            case "txt":
                await SaveAsTxtAsync(recordList, fileName, ct);
                break;
            case "json":
                await SaveAsJsonAsync(recordList, fileName, ct);
                break;
            default:
                throw new ArgumentException($"Unsupported format: {_options.Format}");
        }

        _logger.LogInformation("Stored {Count} records to {Format} file", recordList.Count, _options.Format);
    }

    /// <summary>
    /// 将数据记录保存为 CSV 格式文件
    /// 包含代理信息、任务信息和动态字段列
    /// </summary>
    /// <param name="records">数据记录列表</param>
    /// <param name="fileName">文件名（不含扩展名）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    private async Task SaveAsCsvAsync(List<DataRecord> records, string fileName, CancellationToken ct)
    {
        var filePath = Path.Combine(_outputDirectory, $"{fileName}.csv");
        var allFields = records.SelectMany(r => r.Fields.Keys).Distinct().ToList();
        
        var headers = new List<string> 
        { 
            "AgentId", "AgentName", "HostName", "Mode", "CollectTime", "TaskId", "TaskName", "Url", "Status"
        };
        headers.AddRange(allFields);

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers));

        var collectTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        foreach (var record in records)
        {
            var line = new List<string>
            {
                EscapeCsv(_options.AgentId),
                EscapeCsv(_options.AgentName),
                EscapeCsv(_options.HostName),
                EscapeCsv(_options.Mode),
                collectTime,
                EscapeCsv(_options.TaskId),
                EscapeCsv(_options.TaskName),
                EscapeCsv(record.SourceUrl ?? string.Empty),
                "Success"
            };

            foreach (var field in allFields)
            {
                var value = record.Fields.TryGetValue(field, out var v) ? v?.ToString() : string.Empty;
                line.Add(EscapeCsv(value ?? string.Empty));
            }

            sb.AppendLine(string.Join(",", line));
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), ct);
    }

    /// <summary>
    /// 将数据记录保存为 TXT 格式文件
    /// 以键值对形式输出，包含代理信息、任务信息和数据内容
    /// </summary>
    /// <param name="records">数据记录列表</param>
    /// <param name="fileName">文件名（不含扩展名）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    private async Task SaveAsTxtAsync(List<DataRecord> records, string fileName, CancellationToken ct)
    {
        var filePath = Path.Combine(_outputDirectory, $"{fileName}.txt");
        var sb = new StringBuilder();

        var collectTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        sb.AppendLine("[Agent Info]");
        sb.AppendLine($"AgentId: {_options.AgentId}");
        sb.AppendLine($"AgentName: {_options.AgentName}");
        sb.AppendLine($"HostName: {_options.HostName}");
        sb.AppendLine($"Mode: {_options.Mode}");
        sb.AppendLine($"CollectTime: {collectTime}");
        sb.AppendLine();

        sb.AppendLine("[Task Info]");
        sb.AppendLine($"TaskId: {_options.TaskId}");
        sb.AppendLine($"TaskName: {_options.TaskName}");
        sb.AppendLine($"Url: {(records.FirstOrDefault()?.SourceUrl ?? string.Empty)}");
        sb.AppendLine($"Status: Success");
        sb.AppendLine();

        sb.AppendLine("[Data]");
        foreach (var record in records)
        {
            foreach (var field in record.Fields)
            {
                sb.AppendLine($"{field.Key}: {field.Value}");
            }
            sb.AppendLine(new string('-', 40));
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), ct);
    }

    /// <summary>
    /// 将数据记录保存为 JSON 格式文件
    /// 输出结构化的 JSON 对象，包含代理信息、任务信息和数据数组
    /// </summary>
    /// <param name="records">数据记录列表</param>
    /// <param name="fileName">文件名（不含扩展名）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    private async Task SaveAsJsonAsync(List<DataRecord> records, string fileName, CancellationToken ct)
    {
        var filePath = Path.Combine(_outputDirectory, $"{fileName}.json");
        var collectTime = DateTime.UtcNow;

        var jsonObject = new
        {
            AgentInfo = new
            {
                AgentId = _options.AgentId,
                AgentName = _options.AgentName,
                HostName = _options.HostName,
                Mode = _options.Mode,
                CollectTime = collectTime.ToString("yyyy-MM-dd HH:mm:ss")
            },
            TaskInfo = new
            {
                TaskId = _options.TaskId,
                TaskName = _options.TaskName,
                Url = records.FirstOrDefault()?.SourceUrl ?? string.Empty,
                Status = "Success"
            },
            Data = records.Select(r => new
            {
                r.RecordId,
                r.SourceUrl,
                Fields = r.Fields
            }).ToList()
        };

        var json = System.Text.Json.JsonSerializer.Serialize(jsonObject, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(filePath, json, ct);
    }

    /// <summary>
    /// 转义 CSV 字段值，处理逗号、双引号和换行符
    /// </summary>
    /// <param name="value">原始字段值</param>
    /// <returns>转义后的 CSV 安全字符串</returns>
    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    /// <summary>
    /// 当调用方未提前写入任务元信息时，自动从记录中补齐本地文件头需要的任务标识。
    /// 这样可以保证本地模式输出文件稳定满足需求文档的必填字段格式。
    /// </summary>
    private void ApplyTaskMetadata(List<DataRecord> records)
    {
        if (string.IsNullOrWhiteSpace(_options.TaskId))
        {
            _options.TaskId = records.FirstOrDefault()?.TaskId ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(_options.TaskName)
            && records.FirstOrDefault()?.Fields.TryGetValue("TaskName", out var taskName) == true)
        {
            _options.TaskName = taskName?.ToString() ?? string.Empty;
        }
    }
}
