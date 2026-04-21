using ResourceSpider.Core;
using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ResourceSpider.Infrastructure.Downloader;
using ResourceSpider.Infrastructure.Parser;

namespace ResourceSpider.Agent.Services;

/// <summary>
/// 任务执行器接口，定义爬虫任务的执行方�?/// </summary>
public interface ITaskExecutor
{
    /// <summary>
    /// 执行爬虫任务，返回执行结�?    /// </summary>
    Task<ExecutionResult> ExecuteAsync(SpiderTask task, CancellationToken ct = default);
}

/// <summary>
/// 任务执行器实现，支持单步任务和多步任务的执行
/// 包含请求调度、下载、解析、分页处理等完整流程
/// </summary>
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

    /// <summary>
    /// 执行爬虫任务，根据是否包含步骤自动选择单步或多步执行模�?    /// </summary>
    public async Task<ExecutionResult> ExecuteAsync(SpiderTask task, CancellationToken ct = default)
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

            if (task.Steps is { Count: > 0 })
            {
                await ExecuteMultiStepTaskAsync(task, result, ct);
            }
            else
            {
                await ExecuteSingleTaskAsync(task, result, ct);
            }

            result.Status = Constants.ExecutionStatus.Success;
        }
        catch (Exception ex)
        {
            result.Status = Constants.ExecutionStatus.Failed;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "任务 {TaskId} 执行失败", task.TaskId);
        }

        result.EndTime = DateTime.UtcNow;
        result.Duration = (int)(result.EndTime.Value - result.StartTime).TotalMilliseconds;
        return result;
    }

    /// <summary>
    /// 执行单步任务：提取请�?�?调度 �?下载 �?解析
    /// </summary>
    private async Task ExecuteSingleTaskAsync(SpiderTask task, ExecutionResult result, CancellationToken ct)
    {
        var requests = ExtractRequestsFromTask(task);
        await _scheduler.EnqueueAsync(requests, ct);
        var requestsToProcess = await _scheduler.DequeueAsync(requests.Count, ct);

        var expressionParser = task.ExpressionConfig != null
            ? _parserFactory.CreateFromExpressionConfig(task.ExpressionConfig)
            : null;

        foreach (var request in requestsToProcess)
        {
            if (ct.IsCancellationRequested) break;

            var response = await _downloader.DownloadAsync(request, ct);
            result.TotalRequests++;

            if (response.Status == RequestStatus.Success)
            {
                result.SuccessRequests++;
                var records = ProcessResponse(response, task, expressionParser);
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

    /// <summary>
    /// 执行多步任务：按步骤顺序执行，支持步骤间变量传递和分页
    /// </summary>
    private async Task ExecuteMultiStepTaskAsync(SpiderTask task, ExecutionResult result, CancellationToken ct)
    {
        var stepVariables = new Dictionary<string, object?>();

        foreach (var step in task.Steps!.OrderBy(s => s.StepOrder))
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

                if (response.Status != RequestStatus.Success)
                {
                    result.FailedRequests++;
                    result.Errors.Add($"{request.Url}: {response.Error}");
                    continue;
                }

                result.SuccessRequests++;
                var records = ProcessStepResponse(response, step, task);

                ApplyVariableMappings(step, records, stepVariables);
                result.DataRecords.AddRange(records);

                if (step.PaginationConfig != null)
                {
                    var paginatedRecords = await HandlePaginationAsync(downloader, step, request, result, ct);
                    result.DataRecords.AddRange(paginatedRecords);
                }
            }
        }
    }

    /// <summary>
    /// 根据步骤的采集模式选择对应的下载器
    /// </summary>
    private IDownloader GetDownloaderForStep(TaskStep step)
    {
        return step.CollectionMode switch
        {
            CollectionMode.Playwright => _downloaderFactory.CreateDownloader(DownloadType.Playwright),
            CollectionMode.BrowserAutomation => _downloaderFactory.CreateDownloader(DownloadType.Playwright),
            _ => _downloader
        };
    }

    /// <summary>
    /// 处理单步任务的响应，使用表达式配置或默认解析器提取数�?    /// </summary>
    private List<DataRecord> ProcessResponse(Response response, SpiderTask task, IParser? expressionParser)
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

    /// <summary>
    /// 处理多步任务中某个步骤的响应，根据提取规则或表达式配置提取数�?    /// </summary>
    private List<DataRecord> ProcessStepResponse(Response response, TaskStep step, SpiderTask task)
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
                    records.AddRange(values.Select(value => new DataRecord
                    {
                        TaskId = task.TaskId,
                        StepId = step.StepId,
                        SourceUrl = response.Url,
                        Fields = new Dictionary<string, object?> { [rule.FieldName] = value }
                    }));
                }
                else
                {
                    for (var i = 0; i < Math.Min(records.Count, values.Count); i++)
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

    /// <summary>
    /// 将步骤提取的数据映射到变量，供后续步骤使�?    /// </summary>
    private static void ApplyVariableMappings(TaskStep step, List<DataRecord> records, Dictionary<string, object?> stepVariables)
    {
        if (step.VariableMappings.Count == 0 || records.Count == 0) return;

        foreach (var mapping in step.VariableMappings)
        {
            if (records[0].Fields.TryGetValue(mapping.SourceField, out var value))
            {
                stepVariables[mapping.TargetVariable] = value;
            }
        }
    }

    /// <summary>
    /// 根据提取规则从内容中提取数据，支�?XPath、CSS、JSONPath、Regex 四种方式
    /// </summary>
    private List<string> ExtractByRule(string? content, ExtractionRule rule)
    {
        var results = new List<string>();
        if (string.IsNullOrEmpty(content)) return results;

        switch (rule.ExpressionType)
        {
            case ExpressionType.XPath:
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(content);
                var xpathNodes = htmlDoc.DocumentNode.SelectNodes(rule.Expression);
                if (xpathNodes != null)
                    results.AddRange(xpathNodes.Select(n => n.InnerText.Trim()));
                break;

            case ExpressionType.CssSelector:
                results = AngleSharpCssParser.Extract(content, rule.Expression);
                break;

            case ExpressionType.JsonPath:
                results = JsonPathNetParser.Extract(content, rule.Expression);
                break;

            case ExpressionType.Regex:
                var matches = System.Text.RegularExpressions.Regex.Matches(content, rule.Expression);
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    results.Add(match.Groups.Count > 1 ? match.Groups[1].Value : match.Value);
                }
                break;
        }

        if (rule.TransformRules != null)
        {
            for (var i = 0; i < results.Count; i++)
            {
                foreach (var transform in rule.TransformRules)
                {
                    results[i] = ApplyTransform(results[i], transform);
                }
            }
        }

        return results;
    }

    /// <summary>
    /// 对提取的值应用转换规则（trim、replace、regexreplace、lowercase、uppercase�?    /// </summary>
    private static string ApplyTransform(string value, TransformRule transform)
    {
        return transform.Type.ToLowerInvariant() switch
        {
            Constants.TransformTypes.Trim => value.Trim(),
            Constants.TransformTypes.Replace => !string.IsNullOrEmpty(transform.Pattern)
                ? value.Replace(transform.Pattern, transform.Replacement ?? string.Empty)
                : value,
            Constants.TransformTypes.RegexReplace => !string.IsNullOrEmpty(transform.Pattern)
                ? System.Text.RegularExpressions.Regex.Replace(value, transform.Pattern, transform.Replacement ?? string.Empty)
                : value,
            Constants.TransformTypes.LowerCase => value.ToLowerInvariant(),
            Constants.TransformTypes.UpperCase => value.ToUpperInvariant(),
            _ => value
        };
    }

    /// <summary>
    /// 处理分页请求，自动翻页直到无更多数据或达到最大页�?    /// </summary>
    private async Task<List<DataRecord>> HandlePaginationAsync(
        IDownloader downloader,
        TaskStep step,
        Request originalRequest,
        ExecutionResult result,
        CancellationToken ct)
    {
        var allRecords = new List<DataRecord>();
        var pagination = step.PaginationConfig!;
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

            if (response.Status != RequestStatus.Success)
            {
                result.FailedRequests++;
                break;
            }

            result.SuccessRequests++;
            var records = ProcessStepResponse(response, step, new SpiderTask { TaskId = result.TaskId });

            if (records.Count == 0) break;

            allRecords.AddRange(records);
            currentPage++;

            if (pagination.PaginationType == PaginationType.NextPageUrl)
            {
                var nextLink = ExtractByRule(response.TextContent, new ExtractionRule
                {
                    ExpressionType = ExpressionType.CssSelector,
                    Expression = pagination.NextPageSelector ?? "a.next"
                });

                if (nextLink.Count == 0) break;
            }
        }

        return allRecords;
    }

    /// <summary>
    /// 根据分页配置生成下一�?URL
    /// </summary>
    private static string? GetNextPageUrl(string baseUrl, PaginationConfig pagination, int nextPage)
    {
        if (!string.IsNullOrEmpty(pagination.UrlPattern))
        {
            return pagination.UrlPattern.Replace(Constants.Pagination.PagePlaceholder, nextPage.ToString());
        }

        if (!string.IsNullOrEmpty(pagination.PageParamName))
        {
            var separator = baseUrl.Contains('?') ? "&" : "?";
            return $"{baseUrl}{separator}{pagination.PageParamName}={nextPage}";
        }

        return null;
    }

    /// <summary>
    /// 从任务配置中提取请求列表，支持单 URL 和多 URL 两种格式
    /// </summary>
    private static List<Request> ExtractRequestsFromTask(SpiderTask task)
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
                        ? methodObj?.ToString() ?? Constants.Defaults.DefaultHttpMethod
                        : Constants.Defaults.DefaultHttpMethod
                });
            }
        }

        if (requests.Count == 0)
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

    /// <summary>
    /// 从步骤配置中提取请求，支持变量替换（�?{{变量名}}�?    /// </summary>
    private static List<Request> ExtractRequestsFromStep(TaskStep step, Dictionary<string, object?> variables)
    {
        var requests = new List<Request>();

        if (!step.RequestConfig.TryGetValue("Url", out var urlObj)) return requests;
        var url = urlObj?.ToString();

        if (string.IsNullOrEmpty(url)) return requests;

        foreach (var kvp in variables)
        {
            url = url.Replace($"{{{{{kvp.Key}}}}}", kvp.Value?.ToString() ?? string.Empty);
        }

        requests.Add(new Request
        {
            Url = url,
            Method = step.RequestConfig.TryGetValue("Method", out var methodObj)
                ? methodObj?.ToString() ?? Constants.Defaults.DefaultHttpMethod
                : Constants.Defaults.DefaultHttpMethod
        });

        return requests;
    }
}

/// <summary>
/// 任务执行结果，包含执行状态、请求数统计、采集数据记录和错误信息
/// </summary>
public class ExecutionResult
{
    public string TaskId { get; set; } = string.Empty;
    public string? ExpressionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int SuccessRequests { get; set; }
    public int FailedRequests { get; set; }
    public List<DataRecord> DataRecords { get; set; } = [];
    public List<string> Errors { get; set; } = [];
    public decimal Progress { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Duration { get; set; }
    public string? ErrorMessage { get; set; }
}
