using ResourceSpider.Agent.Config;
using ResourceSpider.Agent.Services;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ResourceSpider.Infrastructure.Downloader;

namespace ResourceSpider.Agent.Modes;

public class LocalModeRunner : IHostedService, IDisposable
{
    private readonly LocalModeOptions _options;
    private readonly ITaskExecutor _taskExecutor;
    private readonly IResultReporter _resultReporter;
    private readonly ILogger<LocalModeRunner> _logger;
    private Timer? _timer;
    private bool _disposed;

    public LocalModeRunner(
        LocalModeOptions options,
        ITaskExecutor taskExecutor,
        IResultReporter resultReporter,
        ILogger<LocalModeRunner> logger)
    {
        _options = options;
        _taskExecutor = taskExecutor;
        _resultReporter = resultReporter;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Local Mode Agent");
        _timer = new Timer(ProcessTasks, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    private async void ProcessTasks(object? state)
    {
        try
        {
            if (!Directory.Exists(_options.TaskFilePath))
            {
                _logger.LogWarning("Task directory not found: {Path}", _options.TaskFilePath);
                return;
            }

            var taskFiles = Directory.GetFiles(_options.TaskFilePath, "*.json");
            
            foreach (var file in taskFiles)
            {
                var json = await File.ReadAllTextAsync(file);
                var task = System.Text.Json.JsonSerializer.Deserialize<Core.Models.SpiderTask>(json);
                
                if (task == null) continue;

                _logger.LogInformation("Processing local task: {TaskName}", task.TaskName);
                var result = await _taskExecutor.ExecuteAsync(task);
                await _resultReporter.StoreLocalAsync(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing local tasks");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Local Mode Agent");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _timer?.Dispose();
        _disposed = true;
    }
}
