using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

[ApiController]
[Route("api/results")]
[Authorize]
public class CollectionResultController : ControllerBase
{
    private readonly ICollectionResultService _resultService;
    private readonly ILogger<CollectionResultController> _logger;

    public CollectionResultController(
        ICollectionResultService resultService,
        ILogger<CollectionResultController> logger)
    {
        _resultService = resultService;
        _logger = logger;
    }

    [HttpGet("{resultId}")]
    [ProducesResponseType(typeof(ApiResponse<CollectionResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(string resultId)
    {
        var result = await _resultService.GetByIdAsync(resultId);
        if (result == null)
        {
            return NotFound(ApiResponse<object>.Error(1005, "Result not found"));
        }
        return Ok(ApiResponse<CollectionResultDto>.Success(result));
    }

    [HttpGet("task/{taskId}")]
    [ProducesResponseType(typeof(ApiResponse<CollectionResultListResponse>), 200)]
    public async Task<IActionResult> GetByTaskId(
        string taskId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _resultService.GetByTaskIdAsync(taskId, pageIndex, pageSize);
        return Ok(ApiResponse<CollectionResultListResponse>.Success(result));
    }

    [HttpGet("expression/{expressionId}")]
    [ProducesResponseType(typeof(ApiResponse<CollectionResultListResponse>), 200)]
    public async Task<IActionResult> GetByExpressionId(
        string expressionId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _resultService.GetByExpressionIdAsync(expressionId, pageIndex, pageSize);
        return Ok(ApiResponse<CollectionResultListResponse>.Success(result));
    }
}
