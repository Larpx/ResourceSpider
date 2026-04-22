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
    /// <param name="result">任务执行结果</param>
    /// <param name="ct">取消令牌</param>
    Task ReportAsync(ExecutionResult result, CancellationToken ct = default);

    /// <summary>
    /// 将采集结果存储到本地文件
    /// </summary>
    /// <param name="result">任务执行结果</param>
    /// <param name="ct">取消令牌</param>
    Task StoreLocalAsync(ExecutionResult result, CancellationToken ct = default);
}

/// <summary>
/// 结果上报器实现，在线模式上报服务端，本地模式写入文件存储
/// </summary>
public class ResultReporter : IResultReporter
{
    /// <summary>
    /// 服务端 API 客户端（在线模式时使用）
    /// </summary>
    private readonly IServerApiClient? _serverApiClient;

    /// <summary>
    /// 存储服务实例
    /// </summary>
    private readonly IStorage _storage;

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<ResultReporter> _logger;

    /// <summary>
    /// Agent 唯一标识
    /// </summary>
    private readonly string _agentId;

    /// <summary>
    /// Agent 认证令牌
    /// </summary>
    private readonly string _agentToken;

    /// <summary>
    /// 初始化结果上报器实例
    /// </summary>
    /// <param name="serverApiClient">服务端 API 客户端（在线模式时提供）</param>
    /// <param name="storage">存储服务实例</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="localConfig">本地模式配置（本地模式时提供）</param>
    /// <param name="serverConfig">在线模式配置（在线模式时提供）</param>
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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
