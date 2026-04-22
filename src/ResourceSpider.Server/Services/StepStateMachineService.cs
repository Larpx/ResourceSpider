using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Models;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface IStepStateMachineService
{
    Task<bool> TryTransitionStepStateAsync(string taskId, string stepId, StepState targetState, Dictionary<string, object?>? context = null);

    Task<List<TaskStepEntity>> EvaluateStepTransitionsAsync(string taskId);

    Task<StepState> GetStepStateAsync(string stepId);

    Task<bool> CheckStartConditionAsync(string taskId, string stepId);

    Task<bool> CheckEndConditionAsync(string taskId, string stepId, int currentDataCount);

    Task ResetStepsForTaskAsync(string taskId);

    Task AdvanceToNextReadyStepAsync(string taskId);
}

public class StepStateMachineService : IStepStateMachineService
{
    private readonly ITaskStepRepository _stepRepository;
    private readonly IStepResourceRepository _stepResourceRepository;
    private readonly ILogger<StepStateMachineService> _logger;

    private static readonly Dictionary<StepState, HashSet<StepState>> _allowedTransitions = new()
    {
        [StepState.Waiting] = [StepState.Ready, StepState.Skipped],
        [StepState.Ready] = [StepState.Running, StepState.Failed, StepState.Skipped],
        [StepState.Running] = [StepState.Completed, StepState.Failed],
        [StepState.Completed] = [],
        [StepState.Failed] = [StepState.Ready, StepState.Waiting],
        [StepState.Skipped] = []
    };

    public StepStateMachineService(
        ITaskStepRepository stepRepository,
        IStepResourceRepository stepResourceRepository,
        ILogger<StepStateMachineService> logger)
    {
        _stepRepository = stepRepository;
        _stepResourceRepository = stepResourceRepository;
        _logger = logger;
    }

    public async Task<bool> TryTransitionStepStateAsync(string taskId, string stepId, StepState targetState, Dictionary<string, object?>? context = null)
    {
        var step = await _stepRepository.GetByIdAsync(stepId);
        if (step == null)
        {
            _logger.LogWarning("步骤 {StepId} 不存在", stepId);
            return false;
        }

        var currentState = (StepState)step.State;

        if (!_allowedTransitions.TryGetValue(currentState, out var allowed) || !allowed.Contains(targetState))
        {
            _logger.LogWarning("步骤 {StepId} 不允许从 {CurrentState} 转换到 {TargetState}", stepId, currentState, targetState);
            return false;
        }

        step.State = (int)targetState;
        await _stepRepository.UpdateAsync(step);

        _logger.LogInformation("步骤 {StepId} 状态从 {CurrentState} 转换到 {TargetState}", stepId, currentState, targetState);

        if (targetState == StepState.Completed)
        {
            await AdvanceToNextReadyStepAsync(taskId);
        }

        return true;
    }

    public async Task<List<TaskStepEntity>> EvaluateStepTransitionsAsync(string taskId)
    {
        var steps = await _stepRepository.GetByTaskIdAsync(taskId);
        var context = await BuildEvaluationContextAsync(taskId, steps);
        var transitioned = new List<TaskStepEntity>();

        foreach (var step in steps.OrderBy(s => s.StepOrder))
        {
            var currentState = (StepState)step.State;

            if (currentState == StepState.Waiting)
            {
                if (await CheckStartConditionWithStepAsync(step, context))
                {
                    step.State = (int)StepState.Ready;
                    await _stepRepository.UpdateAsync(step);
                    transitioned.Add(step);
                    _logger.LogInformation("步骤 {StepId} 满足开始条件，状态从 Waiting 转换到 Ready", step.StepId);
                }
            }
        }

        return transitioned;
    }

    public async Task<StepState> GetStepStateAsync(string stepId)
    {
        var step = await _stepRepository.GetByIdAsync(stepId);
        return step != null ? (StepState)step.State : StepState.Waiting;
    }

    public async Task<bool> CheckStartConditionAsync(string taskId, string stepId)
    {
        var step = await _stepRepository.GetByIdAsync(stepId);
        if (step == null) return false;

        var steps = await _stepRepository.GetByTaskIdAsync(taskId);
        var context = await BuildEvaluationContextAsync(taskId, steps);
        return await CheckStartConditionWithStepAsync(step, context);
    }

    public async Task<bool> CheckEndConditionAsync(string taskId, string stepId, int currentDataCount)
    {
        var step = await _stepRepository.GetByIdAsync(stepId);
        if (step == null) return false;

        var endCondition = DeserializeEndCondition(step.EndCondition);
        if (endCondition == null) return false;

        var context = new Dictionary<string, object?>
        {
            ["current_data_count"] = currentDataCount
        };

        return endCondition.IsSatisfied(currentDataCount, context);
    }

    public async Task ResetStepsForTaskAsync(string taskId)
    {
        var steps = await _stepRepository.GetByTaskIdAsync(taskId);
        var orderedSteps = steps.OrderBy(s => s.StepOrder).ToList();

        foreach (var step in orderedSteps)
        {
            step.State = (int)StepState.Waiting;
            await _stepRepository.UpdateAsync(step);
        }

        if (orderedSteps.Count > 0)
        {
            orderedSteps[0].State = (int)StepState.Ready;
            await _stepRepository.UpdateAsync(orderedSteps[0]);
        }
    }

    public async Task AdvanceToNextReadyStepAsync(string taskId)
    {
        var steps = await _stepRepository.GetByTaskIdAsync(taskId);
        var context = await BuildEvaluationContextAsync(taskId, steps);

        foreach (var step in steps.OrderBy(s => s.StepOrder))
        {
            if ((StepState)step.State == StepState.Waiting)
            {
                if (await CheckStartConditionWithStepAsync(step, context))
                {
                    step.State = (int)StepState.Ready;
                    await _stepRepository.UpdateAsync(step);
                    _logger.LogInformation("步骤 {StepId} 自动推进到就绪状态", step.StepId);
                }
                break;
            }
        }
    }

    private async Task<bool> CheckStartConditionWithStepAsync(TaskStepEntity step, Dictionary<string, object?> context)
    {
        var startCondition = DeserializeStartCondition(step.StartCondition);
        if (startCondition == null)
        {
            return step.StepOrder == 1 || (StepState)step.State == StepState.Waiting;
        }

        return startCondition.Evaluate(context);
    }

    private async Task<Dictionary<string, object?>> BuildEvaluationContextAsync(string taskId, List<TaskStepEntity> steps)
    {
        var context = new Dictionary<string, object?>();

        foreach (var step in steps)
        {
            context[$"step_{step.StepId}_state"] = step.State;
            context[$"step_{step.StepId}_order"] = step.StepOrder;
        }

        var stepIds = steps.Select(s => s.StepId).ToList();
        foreach (var stepId in stepIds)
        {
            var resources = await _stepResourceRepository.GetAvailableByStepIdsAsync(taskId, [stepId], 1);
            context[$"resource_{stepId}_count"] = resources.Count;
        }

        return context;
    }

    private static StepStartCondition? DeserializeStartCondition(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<StepStartCondition>(json);
        }
        catch
        {
            return null;
        }
    }

    private static StepEndCondition? DeserializeEndCondition(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<StepEndCondition>(json);
        }
        catch
        {
            return null;
        }
    }
}
