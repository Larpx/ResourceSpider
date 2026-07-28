using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Services;

namespace Larpx.PersonalTools.ResourceSpider.Server.Controllers;

/// <summary>
/// 提取表达式控制器，提供表达式的增删改查和配置管理功能
/// 表达式定义了从网页中提取数据的规则，支持 XPath、CSS 选择器、正则和 JSONPath
/// </summary>
[ApiController]
[Route("api/admin/expressions")]
[Authorize]
public class ExpressionController : ControllerBase
{
    /// <summary>
    /// 表达式服务实例，处理表达式的业务逻辑
    /// </summary>
    private readonly IExpressionService _expressionService;

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<ExpressionController> _logger;

    /// <summary>
    /// 初始化表达式控制器
    /// </summary>
    /// <param name="expressionService">表达式服务</param>
    /// <param name="logger">日志记录器</param>
    public ExpressionController(
        IExpressionService expressionService,
        ILogger<ExpressionController> logger)
    {
        _expressionService = expressionService;
        _logger = logger;
    }

    /// <summary>
    /// 创建新的提取表达式
    /// </summary>
    /// <param name="request">创建表达式请求，包含表达式名称、类型和内容</param>
    /// <returns>创建成功返回表达式详情</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExpressionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create([FromBody] CreateExpressionRequest request)
    {
        var result = await _expressionService.CreateAsync(request, User.Identity?.Name);
        return Ok(ApiResponse<ExpressionDto>.Success(result, "Expression created successfully"));
    }

    /// <summary>
    /// 根据表达式 ID 获取表达式详情
    /// </summary>
    /// <param name="expressionId">表达式 ID</param>
    /// <returns>表达式存在返回详情，不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 获取表达式列表，支持分页和按状态筛选
    /// </summary>
    /// <param name="pageIndex">页码，默认第 1 页</param>
    /// <param name="pageSize">每页数量，默认 20 条</param>
    /// <param name="status">表达式状态筛选条件，为 null 时不筛选</param>
    /// <param name="keyword">关键字筛选条件，为 null 时不筛选</param>
    /// <returns>表达式列表及分页信息</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ExpressionListResponse>), 200)]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? status = null,
        [FromQuery] string? keyword = null)
    {
        var result = await _expressionService.GetListAsync(pageIndex, pageSize, status, keyword);
        return Ok(ApiResponse<ExpressionListResponse>.Success(result));
    }

    /// <summary>
    /// 更新指定表达式的信息
    /// </summary>
    /// <param name="expressionId">表达式 ID</param>
    /// <param name="request">更新表达式请求，包含需要更新的字段</param>
    /// <returns>更新成功返回确认，表达式不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 删除指定表达式
    /// </summary>
    /// <param name="expressionId">表达式 ID</param>
    /// <returns>删除成功返回确认，表达式不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 获取指定表达式的完整配置信息，供代理节点使用
    /// </summary>
    /// <param name="expressionId">表达式 ID</param>
    /// <returns>表达式配置详情，表达式不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 使已过期的表达式失效，超过指定失败次数阈值的表达式将被标记为不可用
    /// </summary>
    /// <param name="threshold">失败次数阈值，默认为 5 次</param>
    /// <returns>操作完成返回确认</returns>
    [HttpPost("invalidate")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> InvalidateExpired([FromQuery] int threshold = 5)
    {
        await _expressionService.InvalidateExpiredExpressionsAsync(threshold);
        return Ok(ApiResponse<object>.Success(new { }, "Expired expressions invalidated"));
    }
}
