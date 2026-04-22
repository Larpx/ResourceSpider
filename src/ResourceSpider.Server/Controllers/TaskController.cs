using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

/// <summary>
/// 爬虫任务控制器，提供任务的完整生命周期管理
/// 包括创建、查询、更新、删除以及执行控制（暂停、恢复、终止）
/// </summary>
[ApiController]
[Route("api/tasks")]
[Authorize]
public class TaskController : ControllerBase
{
    /// <summary>
    /// 任务服务实例，处理任务的业务逻辑
    /// </summary>
    private readonly ITaskService _taskService;

    /// <summary>
    /// 任务执行服务实例，处理任务执行记录的查询
    /// </summary>
    private readonly ITaskExecutionService _taskExecutionService;

    /// <summary>
    /// 配置版本服务实例，处理任务配置的版本管理和回滚
    /// </summary>
    private readonly IConfigVersionService _configVersionService;

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<TaskController> _logger;

    /// <summary>
    /// 初始化任务控制器
    /// </summary>
    /// <param name="taskService">任务服务</param>
    /// <param name="taskExecutionService">任务执行服务</param>
    /// <param name="configVersionService">配置版本服务</param>
    /// <param name="logger">日志记录器</param>
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

    /// <summary>
    /// 创建新的爬虫任务
    /// </summary>
    /// <param name="request">创建任务请求，包含任务名称、配置等信息</param>
    /// <returns>创建成功返回任务详情</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var result = await _taskService.CreateAsync(request, User.Identity?.Name);
        return Ok(ApiResponse<TaskDto>.Success(result, "任务创建成功"));
    }

    /// <summary>
    /// 根据任务 ID 获取任务详情
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <returns>任务存在返回任务详情，不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 获取任务列表，支持分页和按状态筛选
    /// </summary>
    /// <param name="pageIndex">页码，默认第 1 页</param>
    /// <param name="pageSize">每页数量，默认 20 条</param>
    /// <param name="status">任务状态筛选条件，为 null 时不筛选</param>
    /// <returns>任务列表及分页信息</returns>
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

    /// <summary>
    /// 更新指定任务的信息
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="request">更新任务请求，包含需要更新的字段</param>
    /// <returns>更新成功返回确认，任务不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 删除指定任务
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <returns>删除成功返回确认，任务不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 触发任务执行，将任务分配给可用的代理节点
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <returns>触发成功返回确认，任务不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 暂停指定任务的执行
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <returns>暂停成功返回确认，任务不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 恢复已暂停的任务执行
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <returns>恢复成功返回确认，任务不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 终止指定任务的执行，不可恢复
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <returns>终止成功返回确认，任务不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 获取指定任务的执行历史记录，支持分页
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="pageIndex">页码，默认第 1 页</param>
    /// <param name="pageSize">每页数量，默认 20 条</param>
    /// <returns>任务执行记录列表及分页信息</returns>
    [HttpGet("{taskId}/executions")]
    [ProducesResponseType(typeof(ApiResponse<TaskExecutionListResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetExecutions(string taskId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _taskExecutionService.GetByTaskIdAsync(taskId, pageIndex, pageSize);
        return Ok(ApiResponse<TaskExecutionListResponse>.Success(result));
    }

    /// <summary>
    /// 获取指定任务的配置版本历史
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <returns>配置版本列表</returns>
    [HttpGet("{taskId}/config/versions")]
    [ProducesResponseType(typeof(ApiResponse<List<ConfigVersionDto>>), 200)]
    public async Task<IActionResult> GetConfigVersions(string taskId)
    {
        var result = await _configVersionService.GetVersionsAsync(taskId);
        return Ok(ApiResponse<List<ConfigVersionDto>>.Success(result));
    }

    /// <summary>
    /// 将任务配置回滚到指定版本
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="version">目标配置版本号</param>
    /// <returns>回滚成功返回确认，版本不存在返回 404 状态码</returns>
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
