using ResourceSpider.Agent.Config;
using ResourceSpider.Agent.Services;
using ResourceSpider.Core;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Agent.Modes;

/// <summary>
/// 本地模式运行器，定时扫描本地任务目录中的 JSON 文件并执行采集任务
/// 适用于无需服务端的独立运行场景
/// </summary>
public class LocalModeRunner : IHostedService, IDisposable
{
    private readonly LocalModeOptions _options;
    private readonly ITaskExecutor _taskExecutor;
    private readonly IResultReporter _resultReporter;
    private readonly ILogger<LocalModeRunner> _logger;
    private Timer? _timer;
    private bool _disposed;
    private readonly SemaphoreSlim _executionLock = new(1, 1);

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
        _logger.LogInformation("启动本地模式 Agent");
        _timer = new Timer(ProcessTasks, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    /// <summary>
    /// 定时处理本地任务文件，使用信号量防止并发执行
    /// </summary>
    private async void ProcessTasks(object? state)
    {
        if (!await _executionLock.WaitAsync(0)) return;

        try
        {
            if (!Directory.Exists(_options.TaskFilePath))
            {
                _logger.LogWarning("任务目录不存在: {Path}", _options.TaskFilePath);
                return;
            }

            var taskFiles = Directory.GetFiles(_options.TaskFilePath, $"*{Constants.FileExtensions.Json}");

            foreach (var file in taskFiles)
            {
                var json = await File.ReadAllTextAsync(file);
                var task = System.Text.Json.JsonSerializer.Deserialize<SpiderTask>(json);

                if (task == null) continue;

                _logger.LogInformation("处理本地任务: {TaskName}", task.TaskName);
                var result = await _taskExecutor.ExecuteAsync(task);
                await _resultReporter.StoreLocalAsync(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理本地任务时出错");
        }
        finally
        {
            _executionLock.Release();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("停止本地模式 Agent");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _timer?.Dispose();
        _executionLock.Dispose();
        _disposed = true;
    }
}
