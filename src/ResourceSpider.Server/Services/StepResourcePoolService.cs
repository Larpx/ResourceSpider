using Larpx.PersonalTools.ResourceSpider.Core.Models;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using Larpx.PersonalTools.ResourceSpider.Server.Repositories;

namespace Larpx.PersonalTools.ResourceSpider.Server.Services;

public interface IStepResourcePoolService
{
    Task AddResourcesAsync(string taskId, string stepId, List<Dictionary<string, object?>> payloads, string? sourceUrl = null);

    Task<List<StepResourceEntity>> GetAvailableResourcesAsync(string taskId, string stepId, int take);

    Task<List<StepResourceEntity>> GetResourcesBySourceStepAsync(string taskId, string sourceStepId, int take);

    Task<int> GetResourceCountAsync(string taskId, string stepId);

    Task MarkResourcesConsumedAsync(List<string> resourceIds);

    Task FeedToNextStepsAsync(string taskId, string sourceStepId, List<string>? targetStepIds = null);

    Task CleanupTaskResourcesAsync(string taskId);
}

public class StepResourcePoolService : IStepResourcePoolService
{
    private readonly IStepResourceRepository _stepResourceRepository;
    private readonly ITaskStepRepository _taskStepRepository;
    private readonly ILogger<StepResourcePoolService> _logger;

    public StepResourcePoolService(
        IStepResourceRepository stepResourceRepository,
        ITaskStepRepository taskStepRepository,
        ILogger<StepResourcePoolService> logger)
    {
        _stepResourceRepository = stepResourceRepository;
        _taskStepRepository = taskStepRepository;
        _logger = logger;
    }

    public async Task AddResourcesAsync(string taskId, string stepId, List<Dictionary<string, object?>> payloads, string? sourceUrl = null)
    {
        var entities = payloads.Select(p => new StepResourceEntity
        {
            ResourceId = Guid.NewGuid().ToString("N"),
            TaskId = taskId,
            StepId = stepId,
            SourceStepId = stepId,
            ResourceType = "Record",
            Payload = System.Text.Json.JsonSerializer.Serialize(p),
            ContentHash = ComputeHash(System.Text.Json.JsonSerializer.Serialize(p)),
            SourceUrl = sourceUrl,
            Status = 0
        }).ToList();

        await _stepResourceRepository.AddRangeAsync(entities);
        _logger.LogInformation("任务 {TaskId} 步骤 {StepId} 添加 {Count} 个资源到资源池", taskId, stepId, entities.Count);
    }

    public async Task<List<StepResourceEntity>> GetAvailableResourcesAsync(string taskId, string stepId, int take)
    {
        return await _stepResourceRepository.GetAvailableByStepIdsAsync(taskId, [stepId], take);
    }

    public async Task<List<StepResourceEntity>> GetResourcesBySourceStepAsync(string taskId, string sourceStepId, int take)
    {
        return await _stepResourceRepository.GetAvailableByStepIdsAsync(taskId, [sourceStepId], take);
    }

    public async Task<int> GetResourceCountAsync(string taskId, string stepId)
    {
        var resources = await _stepResourceRepository.GetAvailableByStepIdsAsync(taskId, [stepId], int.MaxValue);
        return resources.Count;
    }

    public async Task MarkResourcesConsumedAsync(List<string> resourceIds)
    {
        if (resourceIds.Count == 0) return;
        await _stepResourceRepository.MarkConsumedAsync(resourceIds);
        _logger.LogInformation("标记 {Count} 个资源为已消费", resourceIds.Count);
    }

    public async Task FeedToNextStepsAsync(string taskId, string sourceStepId, List<string>? targetStepIds = null)
    {
        var steps = await _taskStepRepository.GetByTaskIdAsync(taskId);
        var sourceStep = steps.FirstOrDefault(s => s.StepId == sourceStepId);
        if (sourceStep == null) return;

        var resourcePoolConfig = DeserializeResourcePoolConfig(sourceStep.StepConfig);
        if (resourcePoolConfig == null || !resourcePoolConfig.AutoFeedToNextStep) return;

        var feedStepIds = targetStepIds ?? resourcePoolConfig.FeedToStepIds;
        if (feedStepIds == null || feedStepIds.Count == 0)
        {
            var nextStep = steps
                .OrderBy(s => s.StepOrder)
                .FirstOrDefault(s => s.StepOrder > sourceStep.StepOrder);
            if (nextStep != null)
            {
                feedStepIds = [nextStep.StepId];
            }
        }

        if (feedStepIds == null || feedStepIds.Count == 0) return;

        var resources = await _stepResourceRepository.GetAvailableByStepIdsAsync(taskId, [sourceStepId], int.MaxValue);
        if (resources.Count == 0) return;

        var feedEntities = new List<StepResourceEntity>();
        foreach (var resource in resources)
        {
            foreach (var targetStepId in feedStepIds)
            {
                feedEntities.Add(new StepResourceEntity
                {
                    ResourceId = Guid.NewGuid().ToString("N"),
                    TaskId = taskId,
                    StepId = targetStepId,
                    SourceStepId = sourceStepId,
                    ResourceType = resource.ResourceType,
                    Payload = resource.Payload,
                    ContentHash = resource.ContentHash,
                    SourceUrl = resource.SourceUrl,
                    Status = 0
                });
            }
        }

        await _stepResourceRepository.AddRangeAsync(feedEntities);
        _logger.LogInformation("步骤 {SourceStepId} 向 {Count} 个下游步骤投递 {ResourceCount} 个资源",
            sourceStepId, feedStepIds.Count, resources.Count);
    }

    public async Task CleanupTaskResourcesAsync(string taskId)
    {
        await _stepResourceRepository.DeleteByTaskIdAsync(taskId);
        _logger.LogInformation("清理任务 {TaskId} 的所有资源池数据", taskId);
    }

    private static string ComputeHash(string payload)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static ResourcePoolConfig? DeserializeResourcePoolConfig(string? stepConfigJson)
    {
        if (string.IsNullOrWhiteSpace(stepConfigJson)) return null;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(stepConfigJson);
            if (doc.RootElement.TryGetProperty("ResourcePoolConfig", out var rpcElement))
            {
                return System.Text.Json.JsonSerializer.Deserialize<ResourcePoolConfig>(rpcElement.GetRawText());
            }
        }
        catch
        {
        }
        return null;
    }
}
