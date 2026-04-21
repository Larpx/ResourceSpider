using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Infrastructure.Storage;

public class FileStorageOptions
{
    public string OutputPath { get; set; } = "./results";
    public string Format { get; set; } = "csv";
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string Mode { get; set; } = "Local";
    public string TaskId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
}

public class FileStorage : IStorage
{
    private readonly FileStorageOptions _options;
    private readonly ILogger<FileStorage> _logger;
    private readonly string _outputDirectory;

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

    public Task HandleAsync(DataContext context, CancellationToken ct = default)
    {
        if (context?.DataRecords.Any() == true)
        {
            return StoreAsync(context.DataRecords, ct);
        }
        return Task.CompletedTask;
    }

    public async Task StoreAsync(IEnumerable<DataRecord> records, CancellationToken ct = default)
    {
        var recordList = records.ToList();
        if (!recordList.Any()) return;

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

    private async Task SaveAsCsvAsync(List<DataRecord> records, string fileName, CancellationToken ct)
    {
        var filePath = Path.Combine(_outputDirectory, $"{fileName}.csv");
        var allFields = records.SelectMany(r => r.Fields.Keys).Distinct().ToList();
        
        var headers = new List<string> 
        { 
            "AgentId", "AgentName", "HostName", "CollectTime", "TaskId", "TaskName", "Url", "Status"
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

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
