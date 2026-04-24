using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

/// <summary>
/// 采集结果管理控制器，提供结果查询、批量删除、导出和统计功能
/// 支持按任务 ID 筛选结果，并可将结果导出为 CSV、JSON 或 Excel 格式
/// </summary>
[ApiController]
[Route("api/admin/results")]
[Authorize]
public class ResultController : ControllerBase
{
    /// <summary>
    /// 采集结果服务实例，处理结果的查询逻辑
    /// </summary>
    private readonly ICollectionResultService _resultService;

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<ResultController> _logger;

    /// <summary>
    /// 初始化结果管理控制器
    /// </summary>
    /// <param name="resultService">采集结果服务</param>
    /// <param name="logger">日志记录器</param>
    public ResultController(ICollectionResultService resultService, ILogger<ResultController> logger)
    {
        _resultService = resultService;
        _logger = logger;
    }

    /// <summary>
    /// 获取采集结果列表，支持按任务 ID 筛选和分页
    /// </summary>
    /// <param name="taskId">任务 ID 筛选条件，为 null 时返回空列表</param>
    /// <param name="pageIndex">页码，默认第 1 页</param>
    /// <param name="pageSize">每页数量，默认 20 条</param>
    /// <returns>采集结果列表及分页信息</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CollectionResultListResponse>), 200)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? taskId = null,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        if (!string.IsNullOrEmpty(taskId))
        {
            var result = await _resultService.GetByTaskIdAsync(taskId, pageIndex, pageSize);
            return Ok(ApiResponse<CollectionResultListResponse>.Success(result));
        }

        return Ok(ApiResponse<CollectionResultListResponse>.Success(
            new CollectionResultListResponse(new List<CollectionResultDto>(), 0, pageIndex, pageSize)));
    }

    /// <summary>
    /// 根据结果 ID 获取单条采集结果详情
    /// </summary>
    /// <param name="resultId">采集结果 ID</param>
    /// <returns>结果存在返回详情，不存在返回 404 状态码</returns>
    [HttpGet("{resultId}")]
    [ProducesResponseType(typeof(ApiResponse<CollectionResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(string resultId)
    {
        var result = await _resultService.GetByIdAsync(resultId);
        if (result == null)
        {
            return NotFound(ApiResponse<object>.Error(1005, "结果不存在"));
        }
        return Ok(ApiResponse<CollectionResultDto>.Success(result));
    }

    /// <summary>
    /// 批量删除采集结果，根据结果 ID 列表删除对应任务的所有结果
    /// </summary>
    /// <param name="resultIds">待删除的结果 ID 列表</param>
    /// <param name="resultRepository">结果仓储，通过 DI 注入</param>
    /// <returns>删除成功返回确认</returns>
    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> BatchDelete([FromBody] List<string> resultIds,
        [FromServices] Repositories.ICollectionResultRepository resultRepository)
    {
        foreach (var resultId in resultIds)
        {
            var entity = await resultRepository.GetByIdAsync(resultId);
            if (entity != null)
            {
                await resultRepository.DeleteByTaskIdAsync(entity.TaskId);
            }
        }
        return Ok(ApiResponse<object>.Success(new { }, "删除成功"));
    }

    /// <summary>
    /// 导出指定任务的采集结果，支持 CSV、JSON 和 Excel 格式
    /// </summary>
    /// <param name="request">导出请求，包含任务 ID 和导出格式</param>
    /// <param name="resultRepository">结果仓储，通过 DI 注入</param>
    /// <returns>导出成功返回文件信息（文件名、下载路径、记录数和文件大小）</returns>
    [HttpPost("export")]
    [ProducesResponseType(typeof(ApiResponse<ExportResultDto>), 200)]
    public async Task<IActionResult> Export([FromBody] ExportRequest request,
        [FromServices] Repositories.ICollectionResultRepository resultRepository)
    {
        var results = await resultRepository.GetByTaskIdAsync(request.TaskId, 1, 10000);
        var fileName = $"{request.TaskId}_{DateTime.UtcNow:yyyyMMddHHmmss}.{request.Format.ToString().ToLower()}";
        var exportDir = Path.Combine("exports", request.TaskId);
        Directory.CreateDirectory(exportDir);
        var filePath = Path.Combine(exportDir, fileName);

        switch (request.Format)
        {
            case ExportFormat.Csv:
                await WriteCsvAsync(results, filePath);
                break;
            case ExportFormat.Json:
                await WriteJsonAsync(results, filePath);
                break;
            case ExportFormat.Excel:
                await WriteCsvAsync(results, filePath);
                break;
        }

        var fileInfo = new FileInfo(filePath);
        var dto = new ExportResultDto(fileName, $"/exports/{request.TaskId}/{fileName}", results.Count, fileInfo.Length);
        return Ok(ApiResponse<ExportResultDto>.Success(dto));
    }

    /// <summary>
    /// 获取指定任务的采集结果统计信息
    /// </summary>
    /// <param name="taskId">任务 ID，为 null 时返回空对象</param>
    /// <param name="resultRepository">结果仓储，通过 DI 注入</param>
    /// <returns>任务结果统计（总数）</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> GetStats([FromQuery] string? taskId = null,
        [FromServices] Repositories.ICollectionResultRepository resultRepository = null!)
    {
        if (string.IsNullOrEmpty(taskId))
        {
            return Ok(ApiResponse<object>.Success(new { }));
        }

        var count = await resultRepository.CountByTaskIdAsync(taskId);
        return Ok(ApiResponse<object>.Success(new { taskId, totalResults = count }));
    }

    /// <summary>
    /// 将采集结果写入 CSV 文件
    /// </summary>
    /// <param name="results">采集结果实体列表</param>
    /// <param name="filePath">目标文件路径</param>
    private static async Task WriteCsvAsync(List<Entities.CollectionResultEntity> results, string filePath)
    {
        using var writer = new StreamWriter(filePath);
        if (results.Count == 0) return;

        var fields = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object?>>(results[0].Fields);
        if (fields == null) return;

        await writer.WriteLineAsync(string.Join(",", fields.Keys));
        foreach (var result in results)
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object?>>(result.Fields);
            if (data == null) continue;
            await writer.WriteLineAsync(string.Join(",", data.Values.Select(v => v?.ToString() ?? "")));
        }
    }

    /// <summary>
    /// 将采集结果写入 JSON 文件
    /// </summary>
    /// <param name="results">采集结果实体列表</param>
    /// <param name="filePath">目标文件路径</param>
    private static async Task WriteJsonAsync(List<Entities.CollectionResultEntity> results, string filePath)
    {
        var data = results.Select(r => new
        {
            r.ResultId,
            r.TaskId,
            r.SourceUrl,
            Fields = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object?>>(r.Fields),
            r.CollectedAt
        });
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
        await System.IO.File.WriteAllTextAsync(filePath, json);
    }
}
