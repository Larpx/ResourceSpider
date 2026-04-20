using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ResourceSpider.Infrastructure.Downloader;

namespace ResourceSpider.Agent.Services;

public interface ITaskExecutor
{
    Task<ExecutionResult> ExecuteAsync(Core.Models.SpiderTask task, CancellationToken ct = default);
}

public class TaskExecutor : ITaskExecutor
{
    private readonly IDownloader _downloader;
    private readonly IScheduler _scheduler;
    private readonly IParserFactory _parserFactory;
    private readonly ILogger<TaskExecutor> _logger;

    public TaskExecutor(
        IDownloader downloader,
        IScheduler scheduler,
        IParserFactory parserFactory,
        ILogger<TaskExecutor> logger)
    {
        _downloader = downloader;
        _scheduler = scheduler;
        _parserFactory = parserFactory;
        _logger = logger;
    }

    public async Task<ExecutionResult> ExecuteAsync(Core.Models.SpiderTask task, CancellationToken ct = default)
    {
        var result = new ExecutionResult
        {
            TaskId = task.TaskId,
            StartTime = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Executing task {TaskId}: {TaskName}", task.TaskId, task.TaskName);

            var requests = ExtractRequestsFromTask(task);
            foreach (var request in requests)
            {
                request.TaskId = task.TaskId;
                await _scheduler.AddRequestAsync(request, ct);
            }

            var requestsToProcess = await _scheduler.GetRequestsAsync(requests.Count, ct);
            
            foreach (var request in requestsToProcess)
            {
                if (ct.IsCancellationRequested) break;

                var response = await _downloader.DownloadAsync(request, ct);
                result.TotalRequests++;

                if (response.Status == Core.Enums.RequestStatus.Success)
                {
                    result.SuccessRequests++;
                    
                    var dataContext = new DataContext
                    {
                        Response = response,
                        TaskId = task.TaskId,
                        RequestId = request.RequestId
                    };

                    var parser = _parserFactory.CreateParser(ParserType.JsonPath);
                    await parser.HandleAsync(dataContext, ct);
                    result.DataRecords.AddRange(dataContext.DataRecords);
                }
                else
                {
                    result.FailedRequests++;
                    result.Errors.Add($"{request.Url}: {response.Error}");
                }

                result.Progress = (decimal)(result.TotalRequests) / requests.Count * 100;
            }

            result.Status = "Success";
        }
        catch (Exception ex)
        {
            result.Status = "Failed";
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Task {TaskId} execution failed", task.TaskId);
        }

        result.EndTime = DateTime.UtcNow;
        result.Duration = (int)(result.EndTime.Value - result.StartTime).TotalMilliseconds;
        return result;
    }

    private static List<Request> ExtractRequestsFromTask(Core.Models.SpiderTask task)
    {
        var requests = new List<Request>();
        
        if (task.RequestConfig.TryGetValue("Urls", out var urlsObj) && urlsObj is System.Text.Json.JsonElement urlsElement)
        {
            foreach (var url in urlsElement.EnumerateArray())
            {
                requests.Add(new Request
                {
                    Url = url.GetString() ?? string.Empty,
                    Method = task.RequestConfig.TryGetValue("Method", out var methodObj) 
                        ? methodObj?.ToString() ?? "GET" 
                        : "GET"
                });
            }
        }
        
        if (!requests.Any())
        {
            requests.Add(new Request
            {
                Url = task.RequestConfig.TryGetValue("Url", out var urlObj) 
                    ? urlObj?.ToString() ?? string.Empty 
                    : string.Empty
            });
        }

        return requests;
    }
}

public class ExecutionResult
{
    public string TaskId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int SuccessRequests { get; set; }
    public int FailedRequests { get; set; }
    public List<DataRecord> DataRecords { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public decimal Progress { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Duration { get; set; }
    public string? ErrorMessage { get; set; }
}
