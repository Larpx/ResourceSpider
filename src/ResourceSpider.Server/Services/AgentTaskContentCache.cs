using System.Text.Json;
using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using StackExchange.Redis;

namespace Larpx.PersonalTools.ResourceSpider.Server.Services;

public interface IAgentTaskContentCache
{
    Task<TaskDto?> GetAsync(string taskId, CancellationToken cancellationToken = default);
    Task SetAsync(TaskDto task, CancellationToken cancellationToken = default);
    Task RemoveAsync(string taskId, CancellationToken cancellationToken = default);
}

public sealed class RedisAgentTaskContentCache : IAgentTaskContentCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer _redis;
    private readonly IRedisFeatureService _redisFeatureService;
    private readonly ILogger<RedisAgentTaskContentCache> _logger;

    public RedisAgentTaskContentCache(
        IConnectionMultiplexer redis,
        IRedisFeatureService redisFeatureService,
        ILogger<RedisAgentTaskContentCache> logger)
    {
        _redis = redis;
        _redisFeatureService = redisFeatureService;
        _logger = logger;
    }

    public async Task<TaskDto?> GetAsync(string taskId, CancellationToken cancellationToken = default)
    {
        if (!_redisFeatureService.IsEnabled || !_redisFeatureService.IsConnected)
        {
            return null;
        }

        try
        {
            var value = await _redis.GetDatabase().StringGetAsync(BuildKey(taskId));
            if (!value.HasValue)
            {
                return null;
            }

            return JsonSerializer.Deserialize<TaskDto>(value.ToString(), JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "从 Redis 读取任务缓存失败: {TaskId}", taskId);
            return null;
        }
    }

    public async Task SetAsync(TaskDto task, CancellationToken cancellationToken = default)
    {
        if (!_redisFeatureService.IsEnabled || !_redisFeatureService.IsConnected)
        {
            return;
        }

        try
        {
            var payload = JsonSerializer.Serialize(task, JsonOptions);
            await _redis.GetDatabase().StringSetAsync(
                BuildKey(task.TaskId),
                payload,
                TimeSpan.FromSeconds(_redisFeatureService.TaskContentTtlSeconds));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "写入 Redis 任务缓存失败: {TaskId}", task.TaskId);
        }
    }

    public async Task RemoveAsync(string taskId, CancellationToken cancellationToken = default)
    {
        if (!_redisFeatureService.IsConnected)
        {
            return;
        }

        try
        {
            await _redis.GetDatabase().KeyDeleteAsync(BuildKey(taskId));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "删除 Redis 任务缓存失败: {TaskId}", taskId);
        }
    }

    private static string BuildKey(string taskId) => $"agent:task-content:{taskId}";
}

public sealed class NoOpAgentTaskContentCache : IAgentTaskContentCache
{
    public Task<TaskDto?> GetAsync(string taskId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<TaskDto?>(null);
    }

    public Task SetAsync(TaskDto task, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string taskId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
