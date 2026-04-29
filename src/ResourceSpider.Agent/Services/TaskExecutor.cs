using ResourceSpider.Core;
using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ResourceSpider.Infrastructure.Downloader;
using ResourceSpider.Infrastructure.Parser;

namespace ResourceSpider.Agent.Services;

public interface ITaskExecutor
{
    Task<ExecutionResult> ExecuteAsync(SpiderTask task, CancellationToken ct = default);
}

public class TaskExecutor : ITaskExecutor
{
    private readonly IDownloader _downloader;
    private readonly IDownloaderFactory _downloaderFactory;
    private readonly IScheduler _scheduler;
    private readonly IParserFactory _parserFactory;
    private readonly IVariableResolver _variableResolver;
    private readonly ILogger<TaskExecutor> _logger;

    public TaskExecutor(
        IDownloader downloader,
        IDownloaderFactory downloaderFactory,
        IScheduler scheduler,
        IParserFactory parserFactory,
        IVariableResolver variableResolver,
        ILogger<TaskExecutor> logger)
    {
        _downloader = downloader;
        _downloaderFactory = downloaderFactory;
        _scheduler = scheduler;
        _parserFactory = parserFactory;
        _variableResolver = variableResolver;
        _logger = logger;
    }

    public async Task<ExecutionResult> ExecuteAsync(SpiderTask task, CancellationToken ct = default)
    {
        var result = new ExecutionResult
        {
            TaskId = task.TaskId,
            TaskName = task.TaskName,
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
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            result.Status = Constants.ExecutionStatus.Failed;
            result.ErrorMessage = "任务被取消";
            _logger.LogWarning("任务 {TaskId} 被取消", task.TaskId);
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

    private async Task ExecuteSingleTaskAsync(SpiderTask task, ExecutionResult result, CancellationToken ct)
    {
        var request = BuildRequestFromConfig(task.RequestConfig, task, null);
        if (request == null)
        {
            result.ErrorMessage = "无法从任务配置构建请求";
            return;
        }

        var retryPolicy = task.RetryPolicy ?? new StepRetryPolicy();
        var timeout = task.RequestConfig?.Timeout > 0 ? task.RequestConfig.Timeout : 60000;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        var response = await DownloadWithRetryAsync(_downloader, request, retryPolicy, cts.Token);
        result.TotalRequests++;

        if (response.Status == RequestStatus.Success)
        {
            result.SuccessRequests++;
            var expressionParser = task.ExpressionConfig != null
                ? _parserFactory.CreateFromExpressionConfig(task.ExpressionConfig)
                : null;
            var records = ProcessResponse(response, task, expressionParser);
            result.DataRecords.AddRange(records);
        }
        else
        {
            result.FailedRequests++;
            result.Errors.Add($"{request.Url}: {response.Error}");
        }

        result.Progress = 100;
    }

    private async Task ExecuteMultiStepTaskAsync(SpiderTask task, ExecutionResult result, CancellationToken ct)
    {
        var stepVariables = new Dictionary<string, object?>();
        var stepDataCounts = new Dictionary<string, int>();

        foreach (var step in task.Steps!.OrderBy(s => s.StepOrder))
        {
            if (ct.IsCancellationRequested) break;

            if (!EvaluateStartCondition(step, stepVariables, stepDataCounts))
            {
                _logger.LogInformation("步骤 {StepOrder}: {StepName} 不满足开始条件，跳过", step.StepOrder, step.StepName);
                step.State = StepState.Skipped;
                result.StepResults.Add(new StepExecutionResult
                {
                    StepId = step.StepId,
                    StepName = step.StepName,
                    State = StepState.Skipped,
                    DataCount = 0
                });
                continue;
            }

            _logger.LogInformation("执行步骤 {StepOrder}: {StepName}", step.StepOrder, step.StepName);
            step.State = StepState.Running;

            var stepResult = new StepExecutionResult
            {
                StepId = step.StepId,
                StepName = step.StepName,
                State = StepState.Running
            };

            var stepRequests = BuildRequestsFromStep(step, stepVariables, task);
            var downloader = GetDownloaderForStep(step);
            var retryPolicy = step.RetryPolicy ?? task.RetryPolicy ?? new StepRetryPolicy();
            var stepTimeout = step.Timeout > 0 ? step.Timeout : (step.RequestConfig?.Timeout > 0 ? step.RequestConfig.Timeout : 0);

            foreach (var request in stepRequests)
            {
                if (ct.IsCancellationRequested) break;

                Response response;
                if (stepTimeout > 0)
                {
                    using var stepCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    stepCts.CancelAfter(stepTimeout);
                    response = await DownloadWithRetryAsync(downloader, request, retryPolicy, stepCts.Token);
                }
                else
                {
                    response = await DownloadWithRetryAsync(downloader, request, retryPolicy, ct);
                }

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
                stepResult.DataCount += records.Count;

                if (step.PaginationConfig != null)
                {
                    var paginatedRecords = await HandlePaginationAsync(downloader, step, request, result, stepVariables, task, retryPolicy, ct);
                    ApplyVariableMappings(step, paginatedRecords, stepVariables);
                    result.DataRecords.AddRange(paginatedRecords);
                    stepResult.DataCount += paginatedRecords.Count;
                }

                if (CheckEndCondition(step, stepResult.DataCount))
                {
                    _logger.LogInformation("步骤 {StepOrder}: {StepName} 满足结束条件，停止采集（数据量: {Count}）",
                        step.StepOrder, step.StepName, stepResult.DataCount);
                    break;
                }
            }

            step.State = StepState.Completed;
            stepDataCounts[step.StepId] = stepResult.DataCount;
            stepResult.State = StepState.Completed;
            result.StepResults.Add(stepResult);

            _logger.LogInformation("步骤 {StepOrder}: {StepName} 完成，采集数据量: {DataCount}",
                step.StepOrder, step.StepName, stepResult.DataCount);
        }
    }

    private async Task<Response> DownloadWithRetryAsync(IDownloader downloader, Request request, StepRetryPolicy retryPolicy, CancellationToken ct)
    {
        var retries = 0;
        while (true)
        {
            try
            {
                var response = await downloader.DownloadAsync(request, ct);
                if (response.Status == RequestStatus.Success) return response;

                if (!ShouldRetry(response, retryPolicy)) return response;

                retries++;
                if (retries > retryPolicy.MaxRetries) return response;

                var delay = retryPolicy.RetryIntervalMs * (int)Math.Pow(2, retries - 1);
                delay = Math.Min(delay, 60000);
                _logger.LogWarning("请求 {Url} 失败，第 {Retry}/{Max} 次重试，{Delay}ms 后重试",
                    request.Url, retries, retryPolicy.MaxRetries, delay);
                await Task.Delay(delay, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                retries++;
                if (retries > retryPolicy.MaxRetries)
                {
                    return new Response
                    {
                        RequestId = request.RequestId,
                        Url = request.Url,
                        Status = RequestStatus.Failed,
                        Error = ex.Message,
                        ErrorType = ErrorType.NetworkError
                    };
                }

                var delay = retryPolicy.RetryIntervalMs * (int)Math.Pow(2, retries - 1);
                delay = Math.Min(delay, 60000);
                _logger.LogWarning(ex, "请求 {Url} 异常，第 {Retry}/{Max} 次重试", request.Url, retries, retryPolicy.MaxRetries);
                await Task.Delay(delay, ct);
            }
        }
    }

    private static bool ShouldRetry(Response response, StepRetryPolicy retryPolicy)
    {
        if (response.Status == RequestStatus.Success) return false;

        if (response.ErrorType == ErrorType.Timeout && !retryPolicy.RetryOnTimeout) return false;
        if (response.ErrorType == ErrorType.NetworkError && !retryPolicy.RetryOnNetworkError) return false;

        if (retryPolicy.RetryOnHttpStatusCodes != null && response.StatusCode > 0)
        {
            return retryPolicy.RetryOnHttpStatusCodes.Contains(response.StatusCode);
        }

        return response.StatusCode >= 500 || response.StatusCode == 0;
    }

    private bool EvaluateStartCondition(TaskStep step, Dictionary<string, object?> stepVariables, Dictionary<string, int> stepDataCounts)
    {
        if (step.StartCondition == null)
        {
            return step.StepOrder == 1;
        }

        var context = new Dictionary<string, object?>();
        foreach (var kvp in stepVariables) context[kvp.Key] = kvp.Value;
        foreach (var kvp in stepDataCounts) context[$"resource_{kvp.Key}_count"] = kvp.Value;

        if (step.DependsOnStepIds != null)
        {
            foreach (var depStepId in step.DependsOnStepIds)
            {
                context[$"step_{depStepId}_state"] = stepDataCounts.ContainsKey(depStepId)
                    ? (int)StepState.Completed
                    : (int)StepState.Waiting;
            }
        }

        return step.StartCondition.Evaluate(context);
    }

    private static bool CheckEndCondition(TaskStep step, int currentDataCount)
    {
        if (step.EndCondition == null) return false;
        var context = new Dictionary<string, object?> { ["current_data_count"] = currentDataCount };
        return step.EndCondition.IsSatisfied(currentDataCount, context);
    }

    private IDownloader GetDownloaderForStep(TaskStep step)
    {
        return step.CollectionMode switch
        {
            CollectionMode.Playwright => _downloaderFactory.CreateDownloader(DownloadType.Playwright),
            CollectionMode.BrowserAutomation => _downloaderFactory.CreateDownloader(DownloadType.Playwright),
            _ => _downloader
        };
    }

    private Request? BuildRequestFromConfig(StepRequestConfig? config, SpiderTask task, Dictionary<string, object?>? variables)
    {
        if (config == null) return null;

        var url = config.UrlTemplate;
        if (string.IsNullOrEmpty(url)) return null;

        var systemVars = _variableResolver.GetSystemVariables(task.TaskId, null, task.AssignedAgentId);
        url = _variableResolver.Resolve(url, systemVars);

        if (variables != null)
        {
            url = _variableResolver.Resolve(url, variables);
        }

        var request = new Request
        {
            Url = url,
            Method = config.Method ?? "GET"
        };

        if (config.Headers != null)
        {
            foreach (var header in config.Headers)
            {
                var resolvedValue = _variableResolver.Resolve(header.Value, systemVars);
                if (variables != null) resolvedValue = _variableResolver.Resolve(resolvedValue, variables);
                request.Headers[header.Key] = resolvedValue;
            }
        }

        if (config.Cookies != null)
        {
            var cookieHeader = string.Join("; ", config.Cookies.Select(c => $"{c.Key}={c.Value}"));
            request.Headers["Cookie"] = cookieHeader;
        }

        if (!string.IsNullOrEmpty(config.Body))
        {
            var body = _variableResolver.Resolve(config.Body, systemVars);
            if (variables != null) body = _variableResolver.Resolve(body, variables);
            request.Body = System.Text.Encoding.UTF8.GetBytes(body);
        }

        if (config.PlaywrightConfig != null)
        {
            request.Metadata["PlaywrightConfig"] = config.PlaywrightConfig;
        }

        return request;
    }

    private List<Request> BuildRequestsFromStep(TaskStep step, Dictionary<string, object?> variables, SpiderTask task)
    {
        var requests = new List<Request>();

        if (step.RequestConfig == null) return requests;

        var request = BuildRequestFromConfig(step.RequestConfig, task, variables);
        if (request != null)
        {
            request.TaskId = task.TaskId;
            request.Metadata["StepId"] = step.StepId;
            requests.Add(request);
        }

        return requests;
    }

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
                        Fields = new Dictionary<string, object?>
                        {
                            ["TaskName"] = task.TaskName,
                            [rule.FieldName] = value
                        }
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

    private static void ApplyVariableMappings(TaskStep step, List<DataRecord> records, Dictionary<string, object?> stepVariables)
    {
        if (step.VariableMappings.Count == 0 || records.Count == 0) return;

        foreach (var mapping in step.VariableMappings)
        {
            foreach (var record in records)
            {
                if (record.Fields.TryGetValue(mapping.SourceField, out var value))
                {
                    var key = string.IsNullOrEmpty(mapping.Transform)
                        ? mapping.TargetVariable
                        : mapping.TargetVariable;
                    stepVariables[key] = value;
                }
            }
        }
    }

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

        if (rule.IsRequired && results.Count == 0 && rule.DefaultValue != null)
        {
            results.Add(rule.DefaultValue);
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

        return rule.IsArray ? results : results.Take(1).ToList();
    }

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

    private async Task<List<DataRecord>> HandlePaginationAsync(
        IDownloader downloader,
        TaskStep step,
        Request originalRequest,
        ExecutionResult result,
        Dictionary<string, object?> stepVariables,
        SpiderTask task,
        StepRetryPolicy retryPolicy,
        CancellationToken ct)
    {
        var allRecords = new List<DataRecord>();
        var pagination = step.PaginationConfig!;
        var currentPage = pagination.StartPage;
        var maxPages = pagination.MaxPages ?? int.MaxValue;
        var pageCount = 0;

        while (pageCount < maxPages && !ct.IsCancellationRequested)
        {
            string? nextUrl;

            switch (pagination.PaginationType)
            {
                case PaginationType.PageNumber:
                    currentPage = pagination.StartPage + pageCount * pagination.PageIncrement;
                    nextUrl = BuildPageUrl(originalRequest.Url, pagination, currentPage);
                    break;

                case PaginationType.Offset:
                    var offset = pageCount * (pagination.OffsetIncrement ?? 20);
                    nextUrl = BuildOffsetUrl(originalRequest.Url, pagination, offset);
                    break;

                case PaginationType.NextPageUrl:
                    if (pageCount == 0)
                    {
                        nextUrl = originalRequest.Url;
                    }
                    else
                    {
                        nextUrl = null;
                    }
                    break;

                case PaginationType.ClickNext:
                case PaginationType.InfiniteScroll:
                    nextUrl = pageCount == 0 ? originalRequest.Url : null;
                    break;

                default:
                    nextUrl = BuildPageUrl(originalRequest.Url, pagination, currentPage + pageCount);
                    break;
            }

            if (string.IsNullOrEmpty(nextUrl) && pageCount > 0) break;

            var pageRequest = new Request
            {
                Url = nextUrl!,
                Method = originalRequest.Method,
                Headers = originalRequest.Headers,
                Body = originalRequest.Body,
                Metadata = originalRequest.Metadata
            };

            var systemVars = _variableResolver.GetSystemVariables(task.TaskId, step.StepId, task.AssignedAgentId, currentPage);
            pageRequest.Url = _variableResolver.Resolve(pageRequest.Url, systemVars);
            pageRequest.Url = _variableResolver.Resolve(pageRequest.Url, stepVariables);

            var response = await DownloadWithRetryAsync(downloader, pageRequest, retryPolicy, ct);
            result.TotalRequests++;

            if (response.Status != RequestStatus.Success)
            {
                result.FailedRequests++;
                break;
            }

            result.SuccessRequests++;
            var records = ProcessStepResponse(response, step, new SpiderTask { TaskId = result.TaskId });

            if (records.Count == 0 && pageCount > 0) break;

            allRecords.AddRange(records);
            pageCount++;

            if (pagination.PaginationType == PaginationType.NextPageUrl)
            {
                var nextLink = ExtractByRule(response.TextContent, new ExtractionRule
                {
                    ExpressionType = ExpressionType.CssSelector,
                    Expression = pagination.NextPageSelector ?? "a.next"
                });
                if (nextLink.Count == 0) break;
            }

            if (pagination.PaginationType == PaginationType.InfiniteScroll && step.RequestConfig?.PlaywrightConfig != null)
            {
                await Task.Delay(pagination.ScrollWaitTime, ct);
            }
        }

        return allRecords;
    }

    private static string? BuildPageUrl(string baseUrl, PaginationConfig pagination, int pageNum)
    {
        if (!string.IsNullOrEmpty(pagination.UrlPattern))
        {
            return pagination.UrlPattern
                .Replace(Constants.Pagination.PagePlaceholder, pageNum.ToString())
                .Replace("{{PAGE_NUM}}", pageNum.ToString());
        }

        if (!string.IsNullOrEmpty(pagination.PageParamName))
        {
            var separator = baseUrl.Contains('?') ? "&" : "?";
            return $"{baseUrl}{separator}{pagination.PageParamName}={pageNum}";
        }

        return baseUrl;
    }

    private static string? BuildOffsetUrl(string baseUrl, PaginationConfig pagination, int offset)
    {
        if (!string.IsNullOrEmpty(pagination.UrlPattern))
        {
            return pagination.UrlPattern
                .Replace("{offset}", offset.ToString())
                .Replace("{{OFFSET}}", offset.ToString());
        }

        if (!string.IsNullOrEmpty(pagination.PageParamName))
        {
            var separator = baseUrl.Contains('?') ? "&" : "?";
            return $"{baseUrl}{separator}{pagination.PageParamName}={offset}";
        }

        return baseUrl;
    }
}

public class ExecutionResult
{
    public string TaskId { get; set; } = string.Empty;
    public string? TaskName { get; set; }
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
    public List<StepExecutionResult> StepResults { get; set; } = [];
}

public class StepExecutionResult
{
    public string StepId { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public StepState State { get; set; } = StepState.Waiting;
    public int DataCount { get; set; }
    public string? ErrorMessage { get; set; }
}
