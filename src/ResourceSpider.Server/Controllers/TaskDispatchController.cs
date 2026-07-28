using Microsoft.AspNetCore.Mvc;
using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Services;

namespace Larpx.PersonalTools.ResourceSpider.Server.Controllers;

/// <summary>
/// Agent 任务调度控制器，处理 Agent 的任务拉取、结果上报等通信接口
/// 与管理接口分离，专用于 Agent 与服务端之间的通信
/// </summary>
[ApiController]
[Route("api/agent")]
public class TaskDispatchController : ControllerBase
{
    /// <summary>
    /// 任务调度服务
    /// </summary>
    private readonly ITaskDispatchService _taskDispatchService;

    /// <summary>
    /// 日志记录器
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
    /// Agent 拉取待执行任务
    /// </summary>
    /// <param name="request">包含 AgentId、AgentToken 和最大任务数的请求</param>
    /// <returns>任务列表</returns>
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
    /// Agent 上报任务执行结果
    /// </summary>
    /// <param name="request">包含 AgentId、AgentToken、任务ID、状态、数据量和耗时的请求</param>
    /// <returns>上报结果</returns>
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
    /// Agent 上报步骤执行状态
    /// </summary>
    /// <param name="request">包含 AgentId、AgentToken、任务ID、步骤ID、状态和数据量的请求</param>
    /// <returns>上报结果</returns>
    [HttpPost("tasks/step/report")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> ReportStepStatus([FromBody] ReportStepStatusRequest request)
    {
        var result = await _taskDispatchService.ReportStepStatusAsync(
            request.AgentId, request.AgentToken, request.TaskId, request.StepId, request.State, request.DataCount);

        if (!result)
        {
            return Unauthorized(ApiResponse<object>.Error(1002, "Invalid token"));
        }

        return Ok(ApiResponse<object>.Success(new { }, "Step status reported successfully"));
    }

    /// <summary>
    /// Agent 预取任务，准备执行
    /// </summary>
    /// <param name="request">包含 AgentId、AgentToken 和预取数量的请求</param>
    /// <returns>预取结果</returns>
    [HttpPost("tasks/prefetch")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> PrefetchTasks([FromBody] PrefetchTasksRequest request)
    {
        var result = await _taskDispatchService.PrefetchTasksAsync(
            request.AgentId, request.AgentToken, request.Count);

        if (!result)
        {
            return Unauthorized(ApiResponse<object>.Error(1002, "Invalid token"));
        }

        return Ok(ApiResponse<object>.Success(new { }, "Tasks prefetched successfully"));
    }

    /// <summary>
    /// Agent 拉取指定表达式配置
    /// </summary>
    /// <param name="request">包含 AgentId、AgentToken 和表达式ID的请求</param>
    /// <returns>表达式配置</returns>
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
    /// Agent 拉取所有激活的表达式配置
    /// </summary>
    /// <param name="request">包含 AgentId 和 AgentToken 的请求</param>
    /// <returns>激活的表达式列表</returns>
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
    /// Agent 存储采集结果到服务端
    /// </summary>
    /// <param name="request">包含 AgentId、AgentToken 和采集结果的请求</param>
    /// <returns>存储结果</returns>
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
    /// Agent 上报表达式的可用性状态
    /// </summary>
    /// <param name="request">包含 AgentId、AgentToken、表达式ID、可用性状态和失败原因的请求</param>
    /// <returns>上报结果</returns>
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

    /// <summary>
    /// Agent 获取指定任务的完整配置内容
    /// </summary>
    /// <param name="request">包含 AgentId、AgentToken 和任务ID的请求</param>
    /// <returns>任务配置内容</returns>
    [HttpPost("tasks/content")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetTaskContent([FromBody] PullTaskContentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TaskId))
        {
            return BadRequest(ApiResponse<object>.Error(1007, "TaskId is required"));
        }

        var (isValid, task) = await _taskDispatchService.GetTaskContentAsync(
            request.AgentId, request.AgentToken, request.TaskId);

        if (!isValid)
        {
            return Unauthorized(ApiResponse<object>.Error(1002, "Invalid token"));
        }

        if (task == null)
        {
            return NotFound(ApiResponse<object>.Error(1001, "Task not found"));
        }

        return Ok(ApiResponse<TaskDto>.Success(task));
    }

    /// <summary>
    /// Agent 拉取步骤资源列表
    /// </summary>
    /// <param name="request">包含 AgentId、AgentToken、任务ID、步骤ID 和获取数量的请求</param>
    /// <returns>步骤资源列表</returns>
    [HttpPost("resources/pull")]
    [ProducesResponseType(typeof(ApiResponse<List<StepResourceDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> PullStepResources([FromBody] PullStepResourcesRequest request)
    {
        var (isValid, resources) = await _taskDispatchService.PullStepResourcesAsync(
            request.AgentId, request.AgentToken, request.TaskId, request.StepId, request.Take);

        if (!isValid)
        {
            return Unauthorized(ApiResponse<object>.Error(1002, "Invalid token"));
        }

        return Ok(ApiResponse<List<StepResourceDto>>.Success(resources));
    }

    /// <summary>
    /// Agent 获取自身的状态信息
    /// </summary>
    /// <param name="request">包含 AgentId 和 AgentToken 的请求</param>
    /// <returns>Agent 状态信息</returns>
    [HttpPost("status")]
    [ProducesResponseType(typeof(ApiResponse<AgentStatusDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> GetAgentStatus([FromBody] PullActiveExpressionsRequest request)
    {
        var (isValid, status) = await _taskDispatchService.GetAgentStatusAsync(
            request.AgentId, request.AgentToken);

        if (!isValid)
        {
            return Unauthorized(ApiResponse<object>.Error(1002, "Invalid token"));
        }

        return Ok(ApiResponse<AgentStatusDto>.Success(status!));
    }
}

/// <summary>
/// Agent 拉取任务请求
/// </summary>
/// <param name="AgentId">Agent ID</param>
/// <param name="AgentToken">Agent 认证令牌</param>
/// <param name="MaxCount">最大任务数</param>
public record PullTasksRequest(string AgentId, string AgentToken, int MaxCount = 10);

/// <summary>
/// Agent 上报任务执行结果请求
/// </summary>
/// <param name="AgentId">Agent ID</param>
/// <param name="AgentToken">Agent 认证令牌</param>
/// <param name="TaskId">任务 ID</param>
/// <param name="Status">执行状态</param>
/// <param name="DataCount">采集数据量</param>
/// <param name="Duration">执行耗时（毫秒）</param>
public record ReportTaskRequest(string AgentId, string AgentToken, string TaskId, int Status, int DataCount = 0, int Duration = 0);

/// <summary>
/// Agent 拉取激活表达式请求
/// </summary>
/// <param name="AgentId">Agent ID</param>
/// <param name="AgentToken">Agent 认证令牌</param>
public record PullActiveExpressionsRequest(string AgentId, string AgentToken);

/// <summary>
/// Agent 获取任务内容请求
/// </summary>
/// <param name="AgentId">Agent ID</param>
/// <param name="AgentToken">Agent 认证令牌</param>
/// <param name="TaskId">任务 ID</param>
public record PullTaskContentRequest(string AgentId, string AgentToken, string TaskId);
