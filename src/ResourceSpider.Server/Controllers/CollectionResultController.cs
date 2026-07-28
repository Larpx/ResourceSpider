using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Services;

namespace Larpx.PersonalTools.ResourceSpider.Server.Controllers;

/// <summary>
/// 采集结果控制器，提供按 ID、任务 ID 和表达式 ID 查询采集结果的功能
/// </summary>
[ApiController]
[Route("api/admin/collection-results")]
[Authorize]
public class CollectionResultController : ControllerBase
{
    /// <summary>
    /// 采集结果服务实例，处理采集结果的查询逻辑
    /// </summary>
    private readonly ICollectionResultService _resultService;

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<CollectionResultController> _logger;

    /// <summary>
    /// 初始化采集结果控制器
    /// </summary>
    /// <param name="resultService">采集结果服务</param>
    /// <param name="logger">日志记录器</param>
    public CollectionResultController(
        ICollectionResultService resultService,
        ILogger<CollectionResultController> logger)
    {
        _resultService = resultService;
        _logger = logger;
    }

    /// <summary>
    /// 根据结果 ID 获取单条采集结果
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
            return NotFound(ApiResponse<object>.Error(1005, "Result not found"));
        }
        return Ok(ApiResponse<CollectionResultDto>.Success(result));
    }

    /// <summary>
    /// 根据任务 ID 获取采集结果列表，支持分页
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="pageIndex">页码，默认第 1 页</param>
    /// <param name="pageSize">每页数量，默认 20 条</param>
    /// <returns>采集结果列表及分页信息</returns>
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

    /// <summary>
    /// 根据表达式 ID 获取采集结果列表，支持分页
    /// </summary>
    /// <param name="expressionId">表达式 ID</param>
    /// <param name="pageIndex">页码，默认第 1 页</param>
    /// <param name="pageSize">每页数量，默认 20 条</param>
    /// <returns>采集结果列表及分页信息</returns>
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
