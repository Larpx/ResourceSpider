using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

[ApiController]
[Route("api/agent")]
public class TaskDispatchController : ControllerBase
{
    private readonly ITaskDispatchService _taskDispatchService;
    private readonly ILogger<TaskDispatchController> _logger;

    public TaskDispatchController(
        ITaskDispatchService taskDispatchService,
        ILogger<TaskDispatchController> logger)
    {
        _taskDispatchService = taskDispatchService;
        _logger = logger;
    }

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

public record PullTasksRequest(string AgentId, string AgentToken, int MaxCount = 10);

public record ReportTaskRequest(string AgentId, string AgentToken, string TaskId, int Status, int DataCount = 0, int Duration = 0);

public record PullActiveExpressionsRequest(string AgentId, string AgentToken);

public record PullTaskContentRequest(string AgentId, string AgentToken, string TaskId);
