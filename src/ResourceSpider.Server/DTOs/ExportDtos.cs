namespace ResourceSpider.Server.DTOs;

public enum ExportFormat
{
    Csv,
    Excel,
    Json
}

public record ExportRequest(
    string TaskId,
    ExportFormat Format,
    string? StepId = null,
    DateTime? StartTime = null,
    DateTime? EndTime = null,
    List<string>? Fields = null
);

public record ExportResultDto(
    string FileName,
    string DownloadUrl,
    int TotalRecords,
    long FileSizeBytes
);
