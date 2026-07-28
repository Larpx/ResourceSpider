using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using Larpx.PersonalTools.ResourceSpider.Server.Repositories;
using Larpx.PersonalTools.ResourceSpider.Server.Services;

namespace Larpx.PersonalTools.ResourceSpider.Server.Controllers;

/// <summary>
/// 任务步骤控制器，提供任务步骤的增删改查接口。
/// </summary>
[ApiController]
[Route("api/admin/tasks/{taskId}/steps")]
[Authorize]
public class TaskStepController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ITaskStepRepository _taskStepRepository;

    public TaskStepController(
        ITaskService taskService,
        ITaskStepRepository taskStepRepository)
    {
        _taskService = taskService;
        _taskStepRepository = taskStepRepository;
    }

    /// <summary>
    /// 获取指定任务下的步骤列表。
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TaskStepDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetList(string taskId)
    {
        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null)
        {
            return NotFound(ApiResponse<object>.Error(10001, "任务不存在"));
        }

        var steps = await _taskStepRepository.GetByTaskIdAsync(taskId);
        return Ok(ApiResponse<List<TaskStepDto>>.Success(steps.Select(MapToDto).ToList()));
    }

    /// <summary>
    /// 获取指定任务下的单个步骤详情。
    /// </summary>
    [HttpGet("{stepId}")]
    [ProducesResponseType(typeof(ApiResponse<TaskStepDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(string taskId, string stepId)
    {
        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null)
        {
            return NotFound(ApiResponse<object>.Error(10001, "任务不存在"));
        }

        var step = await _taskStepRepository.GetByIdAsync(stepId);
        if (step == null || !string.Equals(step.TaskId, taskId, StringComparison.Ordinal))
        {
            return NotFound(ApiResponse<object>.Error(10002, "任务步骤不存在"));
        }

        return Ok(ApiResponse<TaskStepDto>.Success(MapToDto(step)));
    }

    /// <summary>
    /// 为指定任务新增步骤。
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskStepDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Create(string taskId, [FromBody] CreateTaskStepRequest request)
    {
        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null)
        {
            return NotFound(ApiResponse<object>.Error(10001, "任务不存在"));
        }

        var existingSteps = await _taskStepRepository.GetByTaskIdAsync(taskId);

        var entity = new TaskStepEntity
        {
            StepId = Guid.NewGuid().ToString("N"),
            TaskId = taskId,
            StepName = request.StepName,
            StepOrder = request.StepOrder,
            CollectionMode = request.CollectionMode,
            AgentGroupId = request.AgentGroupId,
            RequestConfig = string.IsNullOrWhiteSpace(request.RequestConfig) ? "{}" : request.RequestConfig,
            ExtractionRules = string.IsNullOrWhiteSpace(request.ExtractionRules) ? "[]" : request.ExtractionRules,
            VariableMappings = request.VariableMappings,
            PaginationConfig = request.PaginationConfig,
            OutputConfig = request.OutputConfig,
            StartCondition = request.StartCondition,
            EndCondition = request.EndCondition,
            DependsOnStepIds = request.DependsOnStepIds == null ? null : JsonSerializer.Serialize(request.DependsOnStepIds),
            StepConfig = request.StepConfig,
            State = existingSteps.Count == 0 ? 1 : 0
        };

        await _taskStepRepository.AddAsync(entity);
        return Ok(ApiResponse<TaskStepDto>.Success(MapToDto(entity), "步骤创建成功"));
    }

    /// <summary>
    /// 更新指定任务步骤。
    /// </summary>
    [HttpPut("{stepId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Update(string taskId, string stepId, [FromBody] UpdateTaskStepRequest request)
    {
        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null)
        {
            return NotFound(ApiResponse<object>.Error(10001, "任务不存在"));
        }

        var entity = await _taskStepRepository.GetByIdAsync(stepId);
        if (entity == null || !string.Equals(entity.TaskId, taskId, StringComparison.Ordinal))
        {
            return NotFound(ApiResponse<object>.Error(10002, "任务步骤不存在"));
        }

        if (request.StepName != null) entity.StepName = request.StepName;
        if (request.StepOrder.HasValue) entity.StepOrder = request.StepOrder.Value;
        if (request.CollectionMode != null) entity.CollectionMode = request.CollectionMode;
        if (request.AgentGroupId != null) entity.AgentGroupId = request.AgentGroupId;
        if (request.RequestConfig != null) entity.RequestConfig = request.RequestConfig;
        if (request.ExtractionRules != null) entity.ExtractionRules = request.ExtractionRules;
        if (request.VariableMappings != null) entity.VariableMappings = request.VariableMappings;
        if (request.PaginationConfig != null) entity.PaginationConfig = request.PaginationConfig;
        if (request.OutputConfig != null) entity.OutputConfig = request.OutputConfig;
        if (request.StartCondition != null) entity.StartCondition = request.StartCondition;
        if (request.EndCondition != null) entity.EndCondition = request.EndCondition;
        if (request.DependsOnStepIds != null) entity.DependsOnStepIds = JsonSerializer.Serialize(request.DependsOnStepIds);
        if (request.StepConfig != null) entity.StepConfig = request.StepConfig;
        if (request.State.HasValue) entity.State = request.State.Value;

        await _taskStepRepository.UpdateAsync(entity);
        return Ok(ApiResponse<object>.Success(new { }, "步骤更新成功"));
    }

    /// <summary>
    /// 删除指定任务步骤。
    /// </summary>
    [HttpDelete("{stepId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(string taskId, string stepId)
    {
        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null)
        {
            return NotFound(ApiResponse<object>.Error(10001, "任务不存在"));
        }

        var entity = await _taskStepRepository.GetByIdAsync(stepId);
        if (entity == null || !string.Equals(entity.TaskId, taskId, StringComparison.Ordinal))
        {
            return NotFound(ApiResponse<object>.Error(10002, "任务步骤不存在"));
        }

        await _taskStepRepository.DeleteAsync(stepId);
        return Ok(ApiResponse<object>.Success(new { }, "步骤删除成功"));
    }

    private static TaskStepDto MapToDto(TaskStepEntity entity)
    {
        return new TaskStepDto(
            entity.StepId,
            entity.TaskId,
            entity.StepOrder,
            entity.StepName,
            entity.CollectionMode,
            entity.AgentGroupId,
            entity.RequestConfig,
            entity.ExtractionRules,
            entity.VariableMappings,
            entity.PaginationConfig,
            entity.OutputConfig,
            entity.StartCondition,
            entity.EndCondition,
            string.IsNullOrWhiteSpace(entity.DependsOnStepIds)
                ? null
                : JsonSerializer.Deserialize<List<string>>(entity.DependsOnStepIds),
            entity.StepConfig,
            entity.State,
            entity.CreatedAt);
    }
}
