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
    private readonly ITaskExecutionService _taskExecutionService;
    private readonly IConfigVersionService _configVersionService;
    private readonly ILogger<TaskController> _logger;

    public TaskController(
        ITaskService taskService,
        ITaskExecutionService taskExecutionService,
        IConfigVersionService configVersionService,
        ILogger<TaskController> logger)
    {
        _taskService = taskService;
        _taskExecutionService = taskExecutionService;
        _configVersionService = configVersionService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var result = await _taskService.CreateAsync(request, User.Identity?.Name);
        return Ok(ApiResponse<TaskDto>.Success(result, "任务创建成功"));
    }

    [HttpGet("{taskId}")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(string taskId)
    {
        var result = await _taskService.GetByIdAsync(taskId);
        if (result == null)
        {
            return NotFound(ApiResponse<object>.Error(10001, "任务不存在"));
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

    [HttpPut("{taskId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Update(string taskId, [FromBody] UpdateTaskRequest request)
    {
        var existing = await _taskService.GetByIdAsync(taskId);
        if (existing == null)
        {
            return NotFound(ApiResponse<object>.Error(10001, "任务不存在"));
        }

        await _taskService.UpdateAsync(taskId, request);
        return Ok(ApiResponse<object>.Success(new { }, "任务更新成功"));
    }

    [HttpDelete("{taskId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(string taskId)
    {
        var result = await _taskService.DeleteAsync(taskId);
        if (!result)
        {
            return NotFound(ApiResponse<object>.Error(10001, "任务不存在"));
        }
        return Ok(ApiResponse<object>.Success(new { }, "任务删除成功"));
    }

    [HttpPost("{taskId}/execute")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Execute(string taskId)
    {
        var result = await _taskService.TriggerExecutionAsync(taskId);
        if (!result)
        {
            return NotFound(ApiResponse<object>.Error(10001, "任务不存在"));
        }
        return Ok(ApiResponse<object>.Success(new { }, "任务已触发执行"));
    }

    [HttpPost("{taskId}/pause")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Pause(string taskId)
    {
        var result = await _taskService.PauseAsync(taskId);
        if (!result)
        {
            return NotFound(ApiResponse<object>.Error(10001, "任务不存在"));
        }
        return Ok(ApiResponse<object>.Success(new { }, "任务已暂停"));
    }

    [HttpPost("{taskId}/resume")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Resume(string taskId)
    {
        var result = await _taskService.ResumeAsync(taskId);
        if (!result)
        {
            return NotFound(ApiResponse<object>.Error(10001, "任务不存在"));
        }
        return Ok(ApiResponse<object>.Success(new { }, "任务已恢复"));
    }

    [HttpPost("{taskId}/stop")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Stop(string taskId)
    {
        var result = await _taskService.StopAsync(taskId);
        if (!result)
        {
            return NotFound(ApiResponse<object>.Error(10001, "任务不存在"));
        }
        return Ok(ApiResponse<object>.Success(new { }, "任务已终止"));
    }

    [HttpGet("{taskId}/executions")]
    [ProducesResponseType(typeof(ApiResponse<TaskExecutionListResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetExecutions(string taskId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _taskExecutionService.GetByTaskIdAsync(taskId, pageIndex, pageSize);
        return Ok(ApiResponse<TaskExecutionListResponse>.Success(result));
    }

    [HttpGet("{taskId}/config/versions")]
    [ProducesResponseType(typeof(ApiResponse<List<ConfigVersionDto>>), 200)]
    public async Task<IActionResult> GetConfigVersions(string taskId)
    {
        var result = await _configVersionService.GetVersionsAsync(taskId);
        return Ok(ApiResponse<List<ConfigVersionDto>>.Success(result));
    }

    [HttpPost("{taskId}/config/rollback/{version:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> RollbackConfig(string taskId, int version)
    {
        var result = await _configVersionService.RollbackAsync(taskId, version);
        if (!result)
        {
            return NotFound(ApiResponse<object>.Error(10101, "配置版本不存在"));
        }
        return Ok(ApiResponse<object>.Success(new { }, "配置已回滚"));
    }
}
