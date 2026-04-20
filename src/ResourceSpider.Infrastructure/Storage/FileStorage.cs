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
    public string TaskId { get; set; } = string.Empty;
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
            "RecordId", "TaskId", "RequestId", "SourceUrl", "CreatedAt" 
        };
        headers.AddRange(allFields);

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers));

        foreach (var record in records)
        {
            var line = new List<string>
            {
                EscapeCsv(record.RecordId),
                EscapeCsv(record.TaskId ?? string.Empty),
                EscapeCsv(record.RequestId ?? string.Empty),
                EscapeCsv(record.SourceUrl ?? string.Empty),
                record.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
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

        foreach (var record in records)
        {
            sb.AppendLine($"[Record] {record.RecordId}");
            sb.AppendLine($"TaskId: {record.TaskId}");
            sb.AppendLine($"RequestId: {record.RequestId}");
            sb.AppendLine($"SourceUrl: {record.SourceUrl}");
            sb.AppendLine($"CreatedAt: {record.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

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
        var jsonRecords = records.Select(r => new
        {
            r.RecordId,
            r.TaskId,
            r.RequestId,
            r.SourceUrl,
            r.CreatedAt,
            Fields = r.Fields
        }).ToList();

        var json = System.Text.Json.JsonSerializer.Serialize(jsonRecords, new System.Text.Json.JsonSerializerOptions
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
