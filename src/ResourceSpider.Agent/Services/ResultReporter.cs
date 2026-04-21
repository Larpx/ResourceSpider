using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Agent.Services;

public interface IResultReporter
{
    Task ReportAsync(ExecutionResult result, CancellationToken ct = default);
    Task StoreLocalAsync(ExecutionResult result, CancellationToken ct = default);
}

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

    public async Task ReportAsync(ExecutionResult result, CancellationToken ct = default)
    {
        if (_serverApiClient == null)
        {
            _logger.LogWarning("No server API client configured, skipping report");
            return;
        }

        try
        {
            var status = result.Status == "Success" ? 2 : 3;
            await _serverApiClient.ReportTaskAsync(new Services.ReportTaskRequest(
                AgentId: _agentId,
                AgentToken: _agentToken,
                TaskId: result.TaskId,
                Status: status,
                DataCount: result.DataRecords.Count,
                Duration: result.Duration
            ));
            
            _logger.LogInformation("Reported task {TaskId} result", result.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to report task {TaskId} result", result.TaskId);
        }
    }

    public async Task StoreLocalAsync(ExecutionResult result, CancellationToken ct = default)
    {
        if (!result.DataRecords.Any())
        {
            _logger.LogInformation("No data records to store for task {TaskId}", result.TaskId);
            return;
        }

        try
        {
            var context = new DataContext
            {
                DataRecords = result.DataRecords,
                TaskId = result.TaskId
            };

            await _storage.HandleAsync(context, ct);
            _logger.LogInformation("Stored {Count} records locally for task {TaskId}", 
                result.DataRecords.Count, result.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store results locally for task {TaskId}", result.TaskId);
        }
    }
}
