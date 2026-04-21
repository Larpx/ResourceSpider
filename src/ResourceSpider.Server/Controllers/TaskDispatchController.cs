using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

[ApiController]
[Route("api/agent/tasks")]
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

    [HttpPost("pull")]
    [ProducesResponseType(typeof(ApiResponse<List<TaskDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> PullTasks(
        [FromBody] PullTasksRequest request)
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

    [HttpPost("report")]
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
}

public record PullTasksRequest(
    string AgentId,
    string AgentToken,
    int MaxCount = 10
);

public record ReportTaskRequest(
    string AgentId,
    string AgentToken,
    string TaskId,
    int Status,
    int DataCount = 0,
    int Duration = 0
);
