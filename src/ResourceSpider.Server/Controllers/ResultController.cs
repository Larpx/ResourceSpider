using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

[ApiController]
[Route("api/results")]
[Authorize]
public class ResultController : ControllerBase
{
    private readonly ICollectionResultService _resultService;
    private readonly ILogger<ResultController> _logger;

    public ResultController(ICollectionResultService resultService, ILogger<ResultController> logger)
    {
        _resultService = resultService;
        _logger = logger;
    }

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
