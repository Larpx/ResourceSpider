using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

[ApiController]
[Route("api/expressions")]
[Authorize]
public class ExpressionController : ControllerBase
{
    private readonly IExpressionService _expressionService;
    private readonly ILogger<ExpressionController> _logger;

    public ExpressionController(
        IExpressionService expressionService,
        ILogger<ExpressionController> logger)
    {
        _expressionService = expressionService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExpressionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create([FromBody] CreateExpressionRequest request)
    {
        var result = await _expressionService.CreateAsync(request, User.Identity?.Name);
        return Ok(ApiResponse<ExpressionDto>.Success(result, "Expression created successfully"));
    }

    [HttpGet("{expressionId}")]
    [ProducesResponseType(typeof(ApiResponse<ExpressionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(string expressionId)
    {
        var result = await _expressionService.GetByIdAsync(expressionId);
        if (result == null)
        {
            return NotFound(ApiResponse<object>.Error(1004, "Expression not found"));
        }
        return Ok(ApiResponse<ExpressionDto>.Success(result));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ExpressionListResponse>), 200)]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? status = null)
    {
        var result = await _expressionService.GetListAsync(pageIndex, pageSize, status);
        return Ok(ApiResponse<ExpressionListResponse>.Success(result));
    }

    [HttpPut("{expressionId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Update(string expressionId, [FromBody] UpdateExpressionRequest request)
    {
        var result = await _expressionService.UpdateAsync(expressionId, request);
        if (!result)
        {
            return NotFound(ApiResponse<object>.Error(1004, "Expression not found"));
        }
        return Ok(ApiResponse<object>.Success(new { }, "Expression updated successfully"));
    }

    [HttpDelete("{expressionId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(string expressionId)
    {
        var result = await _expressionService.DeleteAsync(expressionId);
        if (!result)
        {
            return NotFound(ApiResponse<object>.Error(1004, "Expression not found"));
        }
        return Ok(ApiResponse<object>.Success(new { }, "Expression deleted successfully"));
    }

    [HttpGet("{expressionId}/config")]
    [ProducesResponseType(typeof(ApiResponse<ExpressionConfigDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetConfig(string expressionId)
    {
        try
        {
            var result = await _expressionService.GetConfigAsync(expressionId);
            return Ok(ApiResponse<ExpressionConfigDto>.Success(result));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object>.Error(1004, "Expression not found"));
        }
    }

    [HttpPost("invalidate")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> InvalidateExpired([FromQuery] int threshold = 5)
    {
        await _expressionService.InvalidateExpiredExpressionsAsync(threshold);
        return Ok(ApiResponse<object>.Success(new { }, "Expired expressions invalidated"));
    }
}
