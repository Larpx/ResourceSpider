namespace ResourceSpider.Server.DTOs;

/// <summary>
/// 导出格式枚举
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// CSV 格式
    /// </summary>
    Csv,
    /// <summary>
    /// Excel 格式
    /// </summary>
    Excel,
    /// <summary>
    /// JSON 格式
    /// </summary>
    Json
}

/// <summary>
/// 导出采集结果请求
/// </summary>
/// <param name="TaskId">任务 ID</param>
/// <param name="Format">导出格式</param>
/// <param name="StepId">步骤 ID 筛选，可选</param>
/// <param name="StartTime">起始时间筛选，可选</param>
/// <param name="EndTime">结束时间筛选，可选</param>
/// <param name="Fields">导出字段列表，可选</param>
public record ExportRequest(
    string TaskId,
    ExportFormat Format,
    string? StepId = null,
    string? AgentId = null,
    string? Keyword = null,
    DateTime? StartTime = null,
    DateTime? EndTime = null,
    List<string>? Fields = null,
    bool? IsDuplicate = null
);

/// <summary>
/// 导出结果数据传输对象
/// </summary>
/// <param name="FileName">文件名</param>
/// <param name="DownloadUrl">下载 URL</param>
/// <param name="TotalRecords">总记录数</param>
/// <param name="FileSizeBytes">文件大小（字节）</param>
public record ExportResultDto(
    string FileName,
    string DownloadUrl,
    int TotalRecords,
    long FileSizeBytes
);
