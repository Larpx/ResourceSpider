using ResourceSpider.Core;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Agent.Services;

/// <summary>
/// 结果上报器接口，定义采集结果的上报和本地存储方法
/// </summary>
public interface IResultReporter
{
    /// <summary>
    /// 向服务端上报采集结果
    /// </summary>
    Task ReportAsync(ExecutionResult result, CancellationToken ct = default);

    /// <summary>
    /// 将采集结果存储到本地文件
    /// </summary>
    Task StoreLocalAsync(ExecutionResult result, CancellationToken ct = default);
}

/// <summary>
/// 结果上报器实现，在线模式上报服务端，本地模式写入文件存储
/// </summary>
public class ResultReporter : IResultReporter
{
    private readonly IServerApiClient? _serverApiClient;
    private readonly IStorage _storage;
    private readonly ILogger<ResultReporter> _logger;
    private readonly string _agentId;
    private readonly string _agentToken;

    public ResultReporter(
        IServerApiClient? serverApiClient,
        IStorage storage,
        ILogger<ResultReporter> logger,
        Agent.Config.LocalModeOptions? localConfig = null,
        Agent.Config.OnlineModeOptions? serverConfig = null)
    {
        _serverApiClient = serverApiClient;
        _storage = storage;
        _logger = logger;
        _agentId = serverConfig?.AgentId ?? localConfig?.TaskFilePath ?? "local-agent";
        _agentToken = serverConfig?.AgentToken ?? string.Empty;
    }

    /// <summary>
    /// 向服务端上报采集结果，包括任务状态和采集数据
    /// </summary>
    public async Task ReportAsync(ExecutionResult result, CancellationToken ct = default)
    {
        if (_serverApiClient == null)
        {
            _logger.LogWarning("未配置服务端 API 客户端，跳过上报");
            return;
        }

        try
        {
            var status = result.Status == Constants.ExecutionStatus.Success ? 2 : 3;
            await _serverApiClient.ReportTaskAsync(new ReportTaskRequest(
                AgentId: _agentId,
                AgentToken: _agentToken,
                TaskId: result.TaskId,
                Status: status,
                DataCount: result.DataRecords.Count,
                Duration: result.Duration
            ));

            if (result.DataRecords.Count > 0)
            {
                var storeRequest = new StoreResultsRequest
                {
                    AgentId = _agentId,
                    AgentToken = _agentToken,
                    TaskId = result.TaskId,
                    ExpressionId = result.ExpressionId,
                    Results = result.DataRecords.Select(r => new ResultItemDto
                    {
                        ResultId = r.RecordId,
                        SourceUrl = r.SourceUrl,
                        Fields = r.Fields,
                        FieldExpressionMap = r.FieldExpressionMap,
                        CollectedAt = r.CreatedAt
                    }).ToList()
                };

                await _serverApiClient.StoreResultsAsync(storeRequest);
            }

            _logger.LogInformation("已上报任务 {TaskId} 结果，共 {Count} 条记录",
                result.TaskId, result.DataRecords.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上报任务 {TaskId} 结果失败", result.TaskId);
        }
    }

    /// <summary>
    /// 将采集结果存储到本地文件
    /// </summary>
    public async Task StoreLocalAsync(ExecutionResult result, CancellationToken ct = default)
    {
        if (result.DataRecords.Count == 0)
        {
            _logger.LogInformation("任务 {TaskId} 无数据记录需要存储", result.TaskId);
            return;
        }

        try
        {
            await _storage.StoreAsync(result.DataRecords, ct);
            _logger.LogInformation("已本地存储任务 {TaskId} 的 {Count} 条记录",
                result.TaskId, result.DataRecords.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "本地存储任务 {TaskId} 结果失败", result.TaskId);
        }
    }
}
