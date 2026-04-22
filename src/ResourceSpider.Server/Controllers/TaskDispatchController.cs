using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

/// <summary>
/// 任务调度控制器，供代理节点调用以拉取任务、报告状态和提交结果
/// 实现代理与服务端之间的任务分发和数据回传机制
/// </summary>
[ApiController]
[Route("api/agent")]
public class TaskDispatchController : ControllerBase
{
    /// <summary>
    /// 任务调度服务实例，处理任务分发和结果收集逻辑
    /// </summary>
    private readonly ITaskDispatchService _taskDispatchService;

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<TaskDispatchController> _logger;

    /// <summary>
    /// 初始化任务调度控制器
    /// </summary>
    /// <param name="taskDispatchService">任务调度服务</param>
    /// <param name="logger">日志记录器</param>
    public TaskDispatchController(
        ITaskDispatchService taskDispatchService,
        ILogger<TaskDispatchController> logger)
    {
        _taskDispatchService = taskDispatchService;
        _logger = logger;
    }

    /// <summary>
    /// 代理拉取待执行的任务列表
    /// </summary>
    /// <param name="request">拉取任务请求，包含代理 ID、令牌和最大拉取数量</param>
    /// <returns>令牌有效返回任务列表，令牌无效返回 401 状态码</returns>
    [HttpPost("tasks/pull")]
    [ProducesResponseType(typeof(ApiResponse<List<TaskDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> PullTasks([FromBody] PullTasksRequest request)
    {
        var (isValid, tasks) = await _taskDispatchService.PullTasksAsync(
            request.AgentId, request.AgentToken, request.MaxCount);

        if (!isValid)
        {
            return Unauthorized(ApiResponse<object>.Error(1002, "Invalid token"));
        }

        if (!tasks.Any())
        {
            return Ok(ApiResponse<List<TaskDto>>.Success(tasks, "No tasks available"));
        }

        return Ok(ApiResponse<List<TaskDto>>.Success(tasks, "Tasks pulled successfully"));
    }

    /// <summary>
    /// 代理报告任务执行状态，包括完成、失败等
    /// </summary>
    /// <param name="request">报告任务请求，包含代理 ID、令牌、任务 ID、状态和数据量</param>
    /// <returns>报告成功返回确认，令牌无效返回 401 状态码</returns>
    [HttpPost("tasks/report")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> ReportTask([FromBody] ReportTaskRequest request)
    {
        var result = await _taskDispatchService.ReportTaskAsync(
            request.AgentId, request.AgentToken, request.TaskId, request.Status, request.DataCount, request.Duration);

        if (!result)
        {
            return Unauthorized(ApiResponse<object>.Error(1002, "Invalid token"));
        }

        return Ok(ApiResponse<object>.Success(new { }, "Task reported successfully"));
    }

    /// <summary>
    /// 代理拉取指定表达式的配置信息
    /// </summary>
    /// <param name="request">拉取表达式请求，包含代理 ID、令牌和表达式 ID</param>
    /// <returns>令牌有效返回表达式配置，令牌无效返回 401，表达式不存在返回 404</returns>
    [HttpPost("expressions/pull")]
    [ProducesResponseType(typeof(ApiResponse<ExpressionConfigDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> PullExpression([FromBody] PullExpressionRequest request)
    {
        if (string.IsNullOrEmpty(request.ExpressionId))
        {
            return BadRequest(ApiResponse<object>.Error(1006, "ExpressionId is required"));
        }

        var (isValid, expression) = await _taskDispatchService.PullExpressionAsync(
            request.AgentId, request.AgentToken, request.ExpressionId);

        if (!isValid)
        {
            return Unauthorized(ApiResponse<object>.Error(1002, "Invalid token"));
        }

        if (expression == null)
        {
            return NotFound(ApiResponse<object>.Error(1004, "Expression not found"));
        }

        return Ok(ApiResponse<ExpressionConfigDto>.Success(expression));
    }

    /// <summary>
    /// 代理拉取所有活跃表达式的配置列表
    /// </summary>
    /// <param name="request">拉取活跃表达式请求，包含代理 ID 和令牌</param>
    /// <returns>令牌有效返回活跃表达式列表，令牌无效返回 401 状态码</returns>
    [HttpPost("expressions/active")]
    [ProducesResponseType(typeof(ApiResponse<List<ExpressionConfigDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> PullActiveExpressions([FromBody] PullActiveExpressionsRequest request)
    {
        var (isValid, expressions) = await _taskDispatchService.PullActiveExpressionsAsync(
            request.AgentId, request.AgentToken);

        if (!isValid)
        {
            return Unauthorized(ApiResponse<object>.Error(1002, "Invalid token"));
        }

        return Ok(ApiResponse<List<ExpressionConfigDto>>.Success(expressions));
    }

    /// <summary>
    /// 代理提交采集结果数据，将采集到的数据存储到服务端
    /// </summary>
    /// <param name="request">存储结果请求，包含代理 ID、令牌和采集数据</param>
    /// <returns>存储成功返回确认，令牌无效返回 401 状态码</returns>
    [HttpPost("results/store")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> StoreResults([FromBody] StoreCollectionResultsRequest request)
    {
        var result = await _taskDispatchService.StoreResultsAsync(
            request.AgentId, request.AgentToken, request);

        if (!result)
        {
            return Unauthorized(ApiResponse<object>.Error(1002, "Invalid token"));
        }

        return Ok(ApiResponse<object>.Success(new { }, "Results stored successfully"));
    }

    /// <summary>
    /// 代理报告表达式的可用性状态，用于标记失效的表达式
    /// </summary>
    /// <param name="request">报告可用性请求，包含代理 ID、令牌、表达式 ID 和可用性状态</param>
    /// <returns>报告成功返回确认，令牌无效返回 401 状态码</returns>
    [HttpPost("expressions/availability")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> ReportExpressionAvailability([FromBody] ReportExpressionAvailabilityRequest request)
    {
        var result = await _taskDispatchService.ReportExpressionAvailabilityAsync(
            request.AgentId, request.AgentToken, request.ExpressionId, request.IsAvailable, request.FailureReason);

        if (!result)
        {
            return Unauthorized(ApiResponse<object>.Error(1002, "Invalid token"));
        }

        return Ok(ApiResponse<object>.Success(new { }, "Availability reported successfully"));
    }
}

/// <summary>
/// 拉取任务请求记录
/// </summary>
/// <param name="AgentId">代理 ID</param>
/// <param name="AgentToken">代理认证令牌</param>
/// <param name="MaxCount">最大拉取任务数量，默认 10</param>
public record PullTasksRequest(string AgentId, string AgentToken, int MaxCount = 10);

/// <summary>
/// 报告任务状态请求记录
/// </summary>
/// <param name="AgentId">代理 ID</param>
/// <param name="AgentToken">代理认证令牌</param>
/// <param name="TaskId">任务 ID</param>
/// <param name="Status">任务状态</param>
/// <param name="DataCount">采集数据数量，默认 0</param>
/// <param name="Duration">执行时长（毫秒），默认 0</param>
public record ReportTaskRequest(string AgentId, string AgentToken, string TaskId, int Status, int DataCount = 0, int Duration = 0);

/// <summary>
/// 拉取活跃表达式请求记录
/// </summary>
/// <param name="AgentId">代理 ID</param>
/// <param name="AgentToken">代理认证令牌</param>
public record PullActiveExpressionsRequest(string AgentId, string AgentToken);
