using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Models;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

/// <summary>
/// 采集结果服务接口，提供采集结果的存储、查询和统计功能
/// </summary>
public interface ICollectionResultService
{
    /// <summary>
    /// 根据结果标识获取单条采集结果
    /// </summary>
    /// <param name="resultId">结果唯一标识</param>
    /// <returns>采集结果 DTO，若不存在返回 null</returns>
    Task<CollectionResultDto?> GetByIdAsync(string resultId);

    /// <summary>
    /// 根据任务标识分页查询采集结果
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>采集结果列表响应</returns>
    Task<CollectionResultListResponse> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize);

    /// <summary>
    /// 根据表达式标识分页查询采集结果
    /// </summary>
    /// <param name="expressionId">表达式唯一标识</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>采集结果列表响应</returns>
    Task<CollectionResultListResponse> GetByExpressionIdAsync(string expressionId, int pageIndex, int pageSize);

    /// <summary>
    /// 批量存储 Agent 上报的采集结果数据
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <param name="expressionId">关联的表达式标识</param>
    /// <param name="agentId">上报结果的 Agent 标识</param>
    /// <param name="results">采集结果项列表</param>
    Task StoreResultsAsync(string taskId, string? expressionId, string agentId, List<CollectionResultItemDto> results);

    /// <summary>
    /// 获取指定任务的采集结果总数
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <returns>结果总数</returns>
    Task<int> GetResultCountByTaskIdAsync(string taskId);

    /// <summary>
    /// 按综合条件分页查询结果。
    /// </summary>
    Task<CollectionResultListResponse> QueryAsync(CollectionResultQuery query);

    /// <summary>
    /// 导入本地结果文件内容。
    /// </summary>
    Task<ImportCollectionResultsResponse> ImportAsync(ImportCollectionResultsRequest request);
}

/// <summary>
/// 采集结果服务实现，管理采集结果的持久化存储、查询和统计
/// </summary>
public class CollectionResultService : ICollectionResultService
{
    private readonly ICollectionResultRepository _resultRepository;
    private readonly IExpressionRepository _expressionRepository;
    private readonly IStepResourceRepository _stepResourceRepository;
    private readonly IStorageStrategyService _storageStrategyService;
    private readonly ITaskRepository _taskRepository;
    private readonly IAgentRepository _agentRepository;
    private readonly ILogger<CollectionResultService> _logger;

    public CollectionResultService(
        ICollectionResultRepository resultRepository,
        IExpressionRepository expressionRepository,
        IStepResourceRepository stepResourceRepository,
        IStorageStrategyService storageStrategyService,
        ITaskRepository taskRepository,
        IAgentRepository agentRepository,
        ILogger<CollectionResultService> logger)
    {
        _resultRepository = resultRepository;
        _expressionRepository = expressionRepository;
        _stepResourceRepository = stepResourceRepository;
        _storageStrategyService = storageStrategyService;
        _taskRepository = taskRepository;
        _agentRepository = agentRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CollectionResultDto?> GetByIdAsync(string resultId)
    {
        var entity = await _resultRepository.GetByIdAsync(resultId);
        return entity != null ? MapToDto(entity) : null;
    }

    /// <inheritdoc />
    public async Task<CollectionResultListResponse> GetByTaskIdAsync(string taskId, int pageIndex, int pageSize)
    {
        var results = await _resultRepository.GetByTaskIdAsync(taskId, pageIndex, pageSize);
        var total = await _resultRepository.CountByTaskIdAsync(taskId);
        return new CollectionResultListResponse(
            results.Select(MapToDto).ToList(),
            (int)total, pageIndex, pageSize);
    }

    /// <inheritdoc />
    public async Task<CollectionResultListResponse> GetByExpressionIdAsync(string expressionId, int pageIndex, int pageSize)
    {
        var results = await _resultRepository.GetByExpressionIdAsync(expressionId, pageIndex, pageSize);
        var total = await _resultRepository.CountByExpressionIdAsync(expressionId);
        return new CollectionResultListResponse(
            results.Select(MapToDto).ToList(),
            (int)total, pageIndex, pageSize);
    }

    /// <inheritdoc />
    public async Task StoreResultsAsync(string taskId, string? expressionId, string agentId, List<CollectionResultItemDto> results)
    {
        var storageEngine = _storageStrategyService.GetCurrentEngine().ToString();
        var task = await _taskRepository.GetByIdAsync(taskId);
        var taskName = task?.TaskName;
        var taskStatus = task == null ? null : task.Status.ToString();
        var deduplication = task?.GlobalConfig != null
            ? DeserializeGlobalConfig(task.GlobalConfig)?.Deduplication
            : null;

        var entities = new List<CollectionResultEntity>(results.Count);
        foreach (var result in results)
        {
            var normalizedFields = NormalizeFields(result.Fields);
            var fingerprint = ComputeFingerprint(taskId, agentId, normalizedFields, result.SourceUrl, deduplication);
            var isDuplicate = deduplication?.Strategy != DeduplicationStrategy.None
                && await _resultRepository.ExistsByFingerprintAsync(taskId, agentId, fingerprint);

            entities.Add(new CollectionResultEntity
            {
                ResultId = result.ResultId ?? Guid.NewGuid().ToString("N"),
                TaskId = taskId,
                TaskName = taskName,
                TaskStatus = taskStatus,
                StepId = result.StepId,
                ExpressionId = expressionId,
                AgentId = agentId,
                SourceUrl = result.SourceUrl,
                Fields = JsonSerializer.Serialize(normalizedFields),
                FieldExpressionMap = result.FieldExpressionMap != null
                    ? JsonSerializer.Serialize(result.FieldExpressionMap)
                    : null,
                DataFingerprint = fingerprint,
                IsDuplicate = isDuplicate,
                StorageEngine = storageEngine,
                CollectedAt = result.CollectedAt ?? DateTime.UtcNow
            });
        }

        await _storageStrategyService.StoreResultsAsync(entities);

        var stepResources = entities
            .Where(e => !string.IsNullOrWhiteSpace(e.StepId))
            .Select(e => new StepResourceEntity
            {
                ResourceId = Guid.NewGuid().ToString("N"),
                TaskId = e.TaskId,
                StepId = e.StepId!,
                SourceStepId = e.StepId,
                ResourceType = "CollectionResult",
                Payload = e.Fields,
                ContentHash = ComputeHash(e.Fields),
                SourceUrl = e.SourceUrl,
                Status = 0
            })
            .ToList();

        await _stepResourceRepository.AddRangeAsync(stepResources);

        _logger.LogInformation(
            "Stored {Count} results for task {TaskId}, expression {ExpressionId}",
            entities.Count, taskId, expressionId);
    }

    /// <inheritdoc />
    public async Task<int> GetResultCountByTaskIdAsync(string taskId)
    {
        return (int)await _resultRepository.CountByTaskIdAsync(taskId);
    }

    /// <inheritdoc />
    public async Task<CollectionResultListResponse> QueryAsync(CollectionResultQuery query)
    {
        var results = await _resultRepository.QueryAsync(
            query.TaskId,
            query.StepId,
            query.AgentId,
            query.Keyword,
            query.StartTime,
            query.EndTime,
            query.IsDuplicate,
            query.PageIndex,
            query.PageSize);

        var total = await _resultRepository.CountAsync(
            query.TaskId,
            query.StepId,
            query.AgentId,
            query.Keyword,
            query.StartTime,
            query.EndTime,
            query.IsDuplicate);

        return new CollectionResultListResponse(results.Select(MapToDto).ToList(), (int)total, query.PageIndex, query.PageSize);
    }

    /// <inheritdoc />
    public async Task<ImportCollectionResultsResponse> ImportAsync(ImportCollectionResultsRequest request)
    {
        var errors = new List<string>();
        var items = new List<CollectionResultItemDto>();
        string taskId = "local-import";
        string agentId = "local-import";
        string? expressionId = null;

        try
        {
            var normalizedContent = request.Content?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedContent))
            {
                return new ImportCollectionResultsResponse(0, 0, 0, 1, ["导入内容为空。"]);
            }

            if (request.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                ParseJsonImport(normalizedContent, items, ref taskId, ref agentId, ref expressionId);
            }
            else if (request.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ParseCsvImport(normalizedContent, items, ref taskId, ref agentId);
            }
            else if (request.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                ParseTxtImport(normalizedContent, items, ref taskId, ref agentId);
            }
            else
            {
                return new ImportCollectionResultsResponse(0, 0, 0, 1, ["仅支持 TXT、CSV、JSON 导入。"]);
            }

            if (request.ValidateAgent)
            {
                var agent = await _agentRepository.GetByIdAsync(agentId);
                if (agent == null)
                {
                    errors.Add($"Agent {agentId} 未注册。");
                    return new ImportCollectionResultsResponse(items.Count, 0, 0, items.Count, errors);
                }
            }

            await StoreResultsAsync(taskId, expressionId, agentId, items);
            var duplicates = items.Count == 0
                ? 0
                : (await _resultRepository.QueryAsync(taskId, null, agentId, null, null, null, true, 1, items.Count)).Count;

            return new ImportCollectionResultsResponse(items.Count, items.Count, duplicates, 0, errors);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "导入本地结果失败：{FileName}", request.FileName);
            errors.Add(ex.Message);
            return new ImportCollectionResultsResponse(items.Count, 0, 0, Math.Max(1, items.Count), errors);
        }
    }

    /// <summary>
    /// 将采集结果实体映射为采集结果 DTO
    /// </summary>
    /// <param name="entity">采集结果实体</param>
    /// <returns>采集结果 DTO</returns>
    private static CollectionResultDto MapToDto(CollectionResultEntity entity)
    {
        var fields = JsonSerializer.Deserialize<Dictionary<string, object?>>(entity.Fields)
            ?? new Dictionary<string, object?>();
        var fieldExpressionMap = entity.FieldExpressionMap != null
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(entity.FieldExpressionMap)
            : new Dictionary<string, string>();

        return new CollectionResultDto(
            entity.ResultId,
            entity.TaskId,
            entity.ExpressionId ?? string.Empty,
            entity.AgentId ?? string.Empty,
            entity.SourceUrl ?? string.Empty,
            fields,
            fieldExpressionMap ?? new Dictionary<string, string>(),
            entity.StepId,
            entity.TaskName,
            entity.TaskStatus,
            entity.DataFingerprint,
            entity.IsDuplicate,
            entity.StorageEngine,
            entity.CollectedAt,
            entity.CreatedAt
        );
    }

    /// <summary>
    /// 标准化字段，将输入的领域模型转换为一致的存储格式
    /// </summary>
    /// <param name="fields">输入的字段字典</param>
    /// <returns>标准化后的字段字典</returns>
    private static Dictionary<string, object?> NormalizeFields(Dictionary<string, object?> fields)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in fields)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            result[key.Trim()] = value switch
            {
                null => null,
                JsonElement el => NormalizeJsonElement(el),
                _ => value
            };
        }

        return result;
    }

    /// <summary>
    /// 标准化 JSON 元素的值，将其转换为可存储的基本类型
    /// </summary>
    /// <param name="element">输入的 JSON 元素</param>
    /// <returns>标准化后的元素值</returns>
    private static object? NormalizeJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.TryGetDecimal(out var d) ? d : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Object => element.ToString(),
            JsonValueKind.Array => element.ToString(),
            _ => element.ToString()
        };
    }

    /// <summary>
    /// 计算给定负载的 SHA256 哈希值
    /// </summary>
    /// <param name="payload">输入的字符串负载</param>
    /// <returns>计算得到的哈希值（十六进制字符串）</returns>
    private static string ComputeHash(string payload)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ComputeFingerprint(string taskId, string agentId, Dictionary<string, object?> fields, string? sourceUrl, DeduplicationConfig? deduplication)
    {
        var strategy = deduplication?.Strategy ?? DeduplicationStrategy.FullFingerprint;

        var payload = strategy switch
        {
            DeduplicationStrategy.None => $"{taskId}:{agentId}:{Guid.NewGuid():N}",
            DeduplicationStrategy.Url => $"{taskId}:{sourceUrl ?? string.Empty}",
            DeduplicationStrategy.FieldCombination => BuildFieldCombinationPayload(taskId, fields, deduplication?.DeduplicationFields),
            DeduplicationStrategy.PrimaryKey => BuildFieldCombinationPayload(taskId, fields, deduplication?.PrimaryKeyFields),
            _ => JsonSerializer.Serialize(new { taskId, agentId, fields })
        };

        return ComputeHash(payload);
    }

    /// <summary>
    /// 根据指定字段列表构建去重指纹的负载字符串
    /// </summary>
    /// <param name="taskId">任务标识</param>
    /// <param name="fields">数据字段字典</param>
    /// <param name="fieldNames">参与去重的字段名列表</param>
    /// <returns>拼接后的负载字符串</returns>
    private static string BuildFieldCombinationPayload(string taskId, Dictionary<string, object?> fields, List<string>? fieldNames)
    {
        if (fieldNames == null || fieldNames.Count == 0)
        {
            return JsonSerializer.Serialize(new { taskId, fields });
        }

        var selectedFields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var fieldName in fieldNames)
        {
            if (fields.TryGetValue(fieldName, out var value))
            {
                selectedFields[fieldName] = value;
            }
        }

        return $"{taskId}:{JsonSerializer.Serialize(selectedFields)}";
    }

    /// <summary>
    /// 反序列化任务全局配置 JSON 字符串
    /// </summary>
    /// <param name="globalConfigJson">全局配置 JSON 字符串</param>
    /// <returns>反序列化后的 TaskGlobalConfig 对象</returns>
    private static TaskGlobalConfig? DeserializeGlobalConfig(string globalConfigJson)
    {
        if (string.IsNullOrWhiteSpace(globalConfigJson)) return null;
        try
        {
            return JsonSerializer.Deserialize<TaskGlobalConfig>(globalConfigJson);
        }
        catch
        {
            return null;
        }
    }

    private static void ParseJsonImport(string content, List<CollectionResultItemDto> items, ref string taskId, ref string agentId, ref string? expressionId)
    {
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        if (root.TryGetProperty("TaskInfo", out var taskInfo))
        {
            taskId = taskInfo.TryGetProperty("TaskId", out var taskIdProperty) ? taskIdProperty.GetString() ?? taskId : taskId;
        }

        if (root.TryGetProperty("AgentInfo", out var agentInfo))
        {
            agentId = agentInfo.TryGetProperty("AgentId", out var agentIdProperty) ? agentIdProperty.GetString() ?? agentId : agentId;
        }

        if (!root.TryGetProperty("Data", out var dataArray) || dataArray.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var item in dataArray.EnumerateArray())
        {
            var fields = item.TryGetProperty("Fields", out var fieldsElement)
                ? JsonSerializer.Deserialize<Dictionary<string, object?>>(fieldsElement.GetRawText()) ?? new Dictionary<string, object?>()
                : new Dictionary<string, object?>();

            items.Add(new CollectionResultItemDto(
                item.TryGetProperty("ResultId", out var resultId) ? resultId.GetString() : null,
                item.TryGetProperty("SourceUrl", out var sourceUrl) ? sourceUrl.GetString() : null,
                fields,
                null,
                item.TryGetProperty("StepId", out var stepId) ? stepId.GetString() : null,
                item.TryGetProperty("CollectedAt", out var collectedAt) && collectedAt.ValueKind == JsonValueKind.String ? collectedAt.GetDateTime() : null));
        }
    }

    private static void ParseCsvImport(string content, List<CollectionResultItemDto> items, ref string taskId, ref string agentId)
    {
        var lines = content.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            return;
        }

        var headers = lines[0].Split(',');
        var fieldStartIndex = Array.IndexOf(headers, "Status") + 1;
        fieldStartIndex = fieldStartIndex <= 0 ? Math.Min(headers.Length, 8) : fieldStartIndex;

        for (var i = 1; i < lines.Length; i++)
        {
            var cells = lines[i].Split(',');
            if (cells.Length == 0)
            {
                continue;
            }

            if (cells.Length > 0) agentId = cells[0];
            if (cells.Length > 4) taskId = cells[4];

            var fields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var cellIndex = fieldStartIndex; cellIndex < Math.Min(headers.Length, cells.Length); cellIndex++)
            {
                fields[headers[cellIndex]] = cells[cellIndex];
            }

            items.Add(new CollectionResultItemDto(null, cells.Length > 6 ? cells[6] : null, fields, null, null, DateTime.UtcNow));
        }
    }

    private static void ParseTxtImport(string content, List<CollectionResultItemDto> items, ref string taskId, ref string agentId)
    {
        var fields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        string? sourceUrl = null;

        foreach (var line in content.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            if (line.StartsWith("AgentId:", StringComparison.OrdinalIgnoreCase))
            {
                agentId = line[8..].Trim();
                continue;
            }

            if (line.StartsWith("TaskId:", StringComparison.OrdinalIgnoreCase))
            {
                taskId = line[7..].Trim();
                continue;
            }

            if (line.StartsWith("Url:", StringComparison.OrdinalIgnoreCase))
            {
                sourceUrl = line[4..].Trim();
                continue;
            }

            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();
            if (key.Equals("AgentName", StringComparison.OrdinalIgnoreCase)
                || key.Equals("HostName", StringComparison.OrdinalIgnoreCase)
                || key.Equals("Mode", StringComparison.OrdinalIgnoreCase)
                || key.Equals("CollectTime", StringComparison.OrdinalIgnoreCase)
                || key.Equals("TaskName", StringComparison.OrdinalIgnoreCase)
                || key.Equals("Status", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            fields[key] = value;
        }

        if (fields.Count > 0)
        {
            items.Add(new CollectionResultItemDto(null, sourceUrl, fields, null, null, DateTime.UtcNow));
        }
    }
}
