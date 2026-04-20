using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TaskController> _logger;

    public TaskController(
        ITaskService taskService,
        ILogger<TaskController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var result = await _taskService.CreateAsync(request, User.Identity?.Name);
        return Ok(ApiResponse<TaskDto>.Success(result, "Task created successfully"));
    }

    [HttpGet("{taskId}")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(string taskId)
    {
        var result = await _taskService.GetByIdAsync(taskId);
        if (result == null)
        {
            return NotFound(ApiResponse<object>.Error(1003, "Task not found"));
        }
        return Ok(ApiResponse<TaskDto>.Success(result));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<TaskListResponse>), 200)]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? status = null)
    {
        var result = await _taskService.GetListAsync(pageIndex, pageSize, status);
        return Ok(ApiResponse<TaskListResponse>.Success(result));
    }

    [HttpPut("{taskId}/pause")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Pause(string taskId)
    {
        var result = await _taskService.PauseAsync(taskId);
        if (!result)
        {
            return NotFound(ApiResponse<object>.Error(1003, "Task not found"));
        }
        return Ok(ApiResponse<object>.Success(new { }, "Task paused"));
    }

    [HttpPut("{taskId}/resume")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Resume(string taskId)
    {
        var result = await _taskService.ResumeAsync(taskId);
        if (!result)
        {
            return NotFound(ApiResponse<object>.Error(1003, "Task not found"));
        }
        return Ok(ApiResponse<object>.Success(new { }, "Task resumed"));
    }

    [HttpDelete("{taskId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(string taskId)
    {
        var result = await _taskService.DeleteAsync(taskId);
        if (!result)
        {
            return NotFound(ApiResponse<object>.Error(1003, "Task not found"));
        }
        return Ok(ApiResponse<object>.Success(new { }, "Task deleted"));
    }
}
