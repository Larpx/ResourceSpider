using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ResourceSpider.Infrastructure.Downloader;
using ResourceSpider.Infrastructure.Parser;

namespace ResourceSpider.Agent.Services;

public interface ITaskExecutor
{
    Task<ExecutionResult> ExecuteAsync(Core.Models.SpiderTask task, CancellationToken ct = default);
}

public class TaskExecutor : ITaskExecutor
{
    private readonly IDownloader _downloader;
    private readonly IDownloaderFactory _downloaderFactory;
    private readonly IScheduler _scheduler;
    private readonly IParserFactory _parserFactory;
    private readonly ILogger<TaskExecutor> _logger;

    public TaskExecutor(
        IDownloader downloader,
        IDownloaderFactory downloaderFactory,
        IScheduler scheduler,
        IParserFactory parserFactory,
        ILogger<TaskExecutor> logger)
    {
        _downloader = downloader;
        _downloaderFactory = downloaderFactory;
        _scheduler = scheduler;
        _parserFactory = parserFactory;
        _logger = logger;
    }

    public async Task<ExecutionResult> ExecuteAsync(Core.Models.SpiderTask task, CancellationToken ct = default)
    {
        var result = new ExecutionResult
        {
            TaskId = task.TaskId,
            ExpressionId = task.ExpressionId,
            StartTime = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("执行任务 {TaskId}: {TaskName}, 类型: {TaskType}", task.TaskId, task.TaskName, task.TaskType);

            if (task.Steps != null && task.Steps.Count > 0)
            {
                await ExecuteMultiStepTaskAsync(task, result, ct);
            }
            else
            {
                await ExecuteSingleTaskAsync(task, result, ct);
            }

            result.Status = "Success";
        }
        catch (Exception ex)
        {
            result.Status = "Failed";
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "任务 {TaskId} 执行失败", task.TaskId);
        }

        result.EndTime = DateTime.UtcNow;
        result.Duration = (int)(result.EndTime.Value - result.StartTime).TotalMilliseconds;
        return result;
    }

    private async Task ExecuteSingleTaskAsync(Core.Models.SpiderTask task, ExecutionResult result, CancellationToken ct)
    {
        var requests = ExtractRequestsFromTask(task);
        await _scheduler.EnqueueAsync(requests, ct);

        var requestsToProcess = await _scheduler.DequeueAsync(requests.Count, ct);

        IParser? expressionParser = null;
        if (task.ExpressionConfig != null)
        {
            expressionParser = _parserFactory.CreateFromExpressionConfig(task.ExpressionConfig);
        }

        foreach (var request in requestsToProcess)
        {
            if (ct.IsCancellationRequested) break;

            var response = await _downloader.DownloadAsync(request, ct);
            result.TotalRequests++;

            if (response.Status == Core.Enums.RequestStatus.Success)
            {
                result.SuccessRequests++;
                var records = await ProcessResponseAsync(response, task, expressionParser);
                result.DataRecords.AddRange(records);
            }
            else
            {
                result.FailedRequests++;
                result.Errors.Add($"{request.Url}: {response.Error}");
            }

            result.Progress = requests.Count > 0
                ? (decimal)result.TotalRequests / requests.Count * 100
                : 0;
        }
    }

    private async Task ExecuteMultiStepTaskAsync(Core.Models.SpiderTask task, ExecutionResult result, CancellationToken ct)
    {
        var stepVariables = new Dictionary<string, object?>();

        foreach (var step in task.Steps.OrderBy(s => s.StepOrder))
        {
            if (ct.IsCancellationRequested) break;

            _logger.LogInformation("执行步骤 {StepOrder}: {StepName}", step.StepOrder, step.StepName);

            var stepRequests = ExtractRequestsFromStep(step, stepVariables);
            var downloader = GetDownloaderForStep(step);

            foreach (var request in stepRequests)
            {
                if (ct.IsCancellationRequested) break;

                var response = await downloader.DownloadAsync(request, ct);
                result.TotalRequests++;

                if (response.Status == Core.Enums.RequestStatus.Success)
                {
                    result.SuccessRequests++;
                    var records = await ProcessStepResponseAsync(response, step, task);

                    if (step.VariableMappings.Count > 0)
                    {
                        foreach (var mapping in step.VariableMappings)
                        {
                            if (records.Count > 0 && records[0].Fields.TryGetValue(mapping.SourceField, out var value))
                            {
                                stepVariables[mapping.TargetVariable] = value;
                            }
                        }
                    }

                    result.DataRecords.AddRange(records);

                    if (step.PaginationConfig != null)
                    {
                        var paginatedRecords = await HandlePaginationAsync(downloader, step, request, records, result, ct);
                        result.DataRecords.AddRange(paginatedRecords);
                    }
                }
                else
                {
                    result.FailedRequests++;
                    result.Errors.Add($"{request.Url}: {response.Error}");
                }
            }
        }
    }

    private IDownloader GetDownloaderForStep(Core.Models.TaskStep step)
    {
        return step.CollectionMode switch
        {
            Core.Enums.CollectionMode.Playwright => _downloaderFactory.CreateDownloader(DownloadType.Playwright),
            Core.Enums.CollectionMode.BrowserAutomation => _downloaderFactory.CreateDownloader(DownloadType.Playwright),
            _ => _downloader
        };
    }

    private async Task<List<DataRecord>> ProcessResponseAsync(Response response, Core.Models.SpiderTask task, IParser? expressionParser)
    {
        var records = new List<DataRecord>();

        if (expressionParser != null)
        {
            var parsedRecords = expressionParser.Parse(response);
            foreach (var record in parsedRecords)
            {
                record.ExpressionId = task.ExpressionId;
                record.AgentId = task.AssignedAgentId;
                if (task.ExpressionConfig != null)
                {
                    record.FieldExpressionMap = task.ExpressionConfig.Fields
                        .Where(f => record.Fields.ContainsKey(f.FieldName))
                        .ToDictionary(f => f.FieldName, f => f.Expression);
                }
                records.Add(record);
            }
        }
        else
        {
            var parser = _parserFactory.CreateParser(ParserType.JsonPath);
            records.AddRange(parser.Parse(response));
        }

        return records;
    }

    private async Task<List<DataRecord>> ProcessStepResponseAsync(Response response, Core.Models.TaskStep step, Core.Models.SpiderTask task)
    {
        var records = new List<DataRecord>();

        if (step.ExtractionRules.Count > 0)
        {
            var content = response.TextContent;
            foreach (var rule in step.ExtractionRules)
            {
                var values = ExtractByRule(content, rule);
                if (records.Count == 0)
                {
                    foreach (var value in values)
                    {
                        records.Add(new DataRecord
                        {
                            TaskId = task.TaskId,
                            StepId = step.StepId,
                            SourceUrl = response.Url,
                            Fields = new Dictionary<string, object?> { [rule.FieldName] = value }
                        });
                    }
                }
                else
                {
                    for (int i = 0; i < Math.Min(records.Count, values.Count); i++)
                    {
                        records[i].Fields[rule.FieldName] = values[i];
                    }
                }
            }
        }
        else if (task.ExpressionConfig != null)
        {
            var parser = _parserFactory.CreateFromExpressionConfig(task.ExpressionConfig);
            records.AddRange(parser.Parse(response));
        }

        return records;
    }

    private List<string> ExtractByRule(string content, Core.Models.ExtractionRule rule)
    {
        var results = new List<string>();
        if (string.IsNullOrEmpty(content)) return results;

        switch (rule.ExpressionType)
        {
            case Core.Enums.ExpressionType.XPath:
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(content);
                var xpathNodes = htmlDoc.DocumentNode.SelectNodes(rule.Expression);
                if (xpathNodes != null)
                    results.AddRange(xpathNodes.Select(n => n.InnerText.Trim()));
                break;

            case Core.Enums.ExpressionType.CssSelector:
                results = Infrastructure.Parser.AngleSharpCssParser.Extract(content, rule.Expression);
                break;

            case Core.Enums.ExpressionType.JsonPath:
                results = Infrastructure.Parser.JsonPathNetParser.Extract(content, rule.Expression);
                break;

            case Core.Enums.ExpressionType.Regex:
                var matches = System.Text.RegularExpressions.Regex.Matches(content, rule.Expression);
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    results.Add(match.Groups.Count > 1 ? match.Groups[1].Value : match.Value);
                }
                break;
        }

        if (rule.TransformRules != null)
        {
            foreach (var transform in rule.TransformRules)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    results[i] = ApplyTransform(results[i], transform);
                }
            }
        }

        return results;
    }

    private static string ApplyTransform(string value, Core.Models.TransformRule transform)
    {
        return transform.Type.ToLower() switch
        {
            "trim" => value.Trim(),
            "replace" => !string.IsNullOrEmpty(transform.Pattern)
                ? value.Replace(transform.Pattern, transform.Replacement ?? "")
                : value,
            "regexreplace" => !string.IsNullOrEmpty(transform.Pattern)
                ? System.Text.RegularExpressions.Regex.Replace(value, transform.Pattern, transform.Replacement ?? "")
                : value,
            "lowercase" => value.ToLower(),
            "uppercase" => value.ToUpper(),
            _ => value
        };
    }

    private async Task<List<DataRecord>> HandlePaginationAsync(
        IDownloader downloader,
        Core.Models.TaskStep step,
        Request originalRequest,
        List<DataRecord> initialRecords,
        ExecutionResult result,
        CancellationToken ct)
    {
        var allRecords = new List<DataRecord>();
        var pagination = step.PaginationConfig;
        if (pagination == null) return allRecords;

        var currentPage = pagination.StartPage;
        var maxPages = pagination.MaxPages ?? int.MaxValue;

        while (currentPage < maxPages && !ct.IsCancellationRequested)
        {
            var nextUrl = GetNextPageUrl(originalRequest.Url, pagination, currentPage + 1);
            if (string.IsNullOrEmpty(nextUrl)) break;

            var pageRequest = new Request
            {
                Url = nextUrl,
                Method = originalRequest.Method,
                Headers = originalRequest.Headers,
                Body = originalRequest.Body
            };

            var response = await downloader.DownloadAsync(pageRequest, ct);
            result.TotalRequests++;

            if (response.Status != Core.Enums.RequestStatus.Success)
            {
                result.FailedRequests++;
                break;
            }

            result.SuccessRequests++;
            var records = await ProcessStepResponseAsync(response, step, new Core.Models.SpiderTask { TaskId = result.TaskId });

            if (records.Count == 0) break;

            allRecords.AddRange(records);
            currentPage++;

            if (pagination.PaginationType == Core.Enums.PaginationType.NextPageUrl)
            {
                var nextLink = ExtractByRule(response.TextContent, new Core.Models.ExtractionRule
                {
                    ExpressionType = Core.Enums.ExpressionType.CssSelector,
                    Expression = pagination.NextPageSelector ?? "a.next"
                });

                if (nextLink.Count == 0) break;
            }
        }

        return allRecords;
    }

    private static string? GetNextPageUrl(string baseUrl, Core.Models.PaginationConfig pagination, int nextPage)
    {
        if (!string.IsNullOrEmpty(pagination.UrlPattern))
        {
            return pagination.UrlPattern.Replace("{page}", nextPage.ToString());
        }

        if (!string.IsNullOrEmpty(pagination.PageParamName))
        {
            var separator = baseUrl.Contains('?') ? "&" : "?";
            return $"{baseUrl}{separator}{pagination.PageParamName}={nextPage}";
        }

        return null;
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

    private static List<Request> ExtractRequestsFromStep(Core.Models.TaskStep step, Dictionary<string, object?> variables)
    {
        var requests = new List<Request>();

        string? url = null;
        if (step.RequestConfig.TryGetValue("Url", out var urlObj))
        {
            url = urlObj?.ToString();
        }

        if (!string.IsNullOrEmpty(url))
        {
            foreach (var kvp in variables)
            {
                url = url.Replace($"{{{{{kvp.Key}}}}}", kvp.Value?.ToString() ?? "");
            }

            requests.Add(new Request
            {
                Url = url,
                Method = step.RequestConfig.TryGetValue("Method", out var methodObj)
                    ? methodObj?.ToString() ?? "GET"
                    : "GET"
            });
        }

        return requests;
    }
}

public class ExecutionResult
{
    public string TaskId { get; set; } = string.Empty;
    public string? ExpressionId { get; set; }
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
