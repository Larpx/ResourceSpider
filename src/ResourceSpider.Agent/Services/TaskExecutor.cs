using ResourceSpider.Core;
using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;
using ResourceSpider.Infrastructure.Downloader;
using ResourceSpider.Infrastructure.Parser;

namespace ResourceSpider.Agent.Services;

/// <summary>
/// 任务执行器接口，定义爬虫任务的执行方法
/// </summary>
public interface ITaskExecutor
{
    /// <summary>
    /// 执行爬虫任务，支持单步和多步骤任务
    /// </summary>
    /// <param name="task">待执行的爬虫任务</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>任务执行结果</returns>
    Task<ExecutionResult> ExecuteAsync(SpiderTask task, CancellationToken ct = default);
}

/// <summary>
/// 任务执行器实现，负责爬虫任务的核心执行逻辑
/// 支持单步任务和多步骤任务的执行，包含重试、分页、变量映射、条件判断等功能
/// </summary>
public class TaskExecutor : ITaskExecutor
{
    /// <summary>
    /// 默认 HTTP 下载器
    /// </summary>
    private readonly IDownloader _downloader;

    /// <summary>
    /// 下载器工厂，用于根据步骤类型创建对应的下载器
    /// </summary>
    private readonly IDownloaderFactory _downloaderFactory;

    /// <summary>
    /// URL 调度器，管理待采集的 URL 队列
    /// </summary>
    private readonly IScheduler _scheduler;

    /// <summary>
    /// 解析器工厂，用于创建不同类型的内容解析器
    /// </summary>
    private readonly IParserFactory _parserFactory;

    /// <summary>
    /// 变量解析器，用于解析模板中的变量占位符
    /// </summary>
    private readonly IVariableResolver _variableResolver;

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<TaskExecutor> _logger;

    /// <summary>
    /// 初始化任务执行器
    /// </summary>
    /// <param name="downloader">默认 HTTP 下载器</param>
    /// <param name="downloaderFactory">下载器工厂</param>
    /// <param name="scheduler">URL 调度器</param>
    /// <param name="parserFactory">解析器工厂</param>
    /// <param name="variableResolver">变量解析器</param>
    /// <param name="logger">日志记录器</param>
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

    /// <summary>
    /// 执行爬虫任务，根据任务是否包含步骤自动选择单步或多步骤执行模式
    /// </summary>
    /// <param name="task">待执行的爬虫任务</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>包含采集数据、请求统计和步骤结果的执行结果</returns>
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

    /// <summary>
    /// 执行单步任务，构建请求、下载内容并解析数据
    /// </summary>
    /// <param name="task">爬虫任务</param>
    /// <param name="result">执行结果容器</param>
    /// <param name="ct">取消令牌</param>
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

    /// <summary>
    /// 执行多步骤任务，按步骤顺序执行，支持条件判断、变量映射和分页采集
    /// </summary>
    /// <param name="task">爬虫任务</param>
    /// <param name="result">执行结果容器</param>
    /// <param name="ct">取消令牌</param>
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

    /// <summary>
    /// 带重试机制的下载方法，根据重试策略进行指数退避重试
    /// </summary>
    /// <param name="downloader">下载器实例</param>
    /// <param name="request">下载请求</param>
    /// <param name="retryPolicy">重试策略</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>下载响应结果</returns>
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

    /// <summary>
    /// 判断响应是否应该重试，根据重试策略和错误类型决定
    /// </summary>
    /// <param name="response">下载响应</param>
    /// <param name="retryPolicy">重试策略</param>
    /// <returns>需要重试返回 true，否则返回 false</returns>
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

    /// <summary>
    /// 评估步骤的开始条件，判断步骤是否应该执行
    /// </summary>
    /// <param name="step">任务步骤</param>
    /// <param name="stepVariables">步骤间传递的变量字典</param>
    /// <param name="stepDataCounts">各步骤已采集的数据量字典</param>
    /// <returns>条件满足返回 true，否则返回 false</returns>
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

    /// <summary>
    /// 检查步骤的结束条件，判断是否应该停止采集
    /// </summary>
    /// <param name="step">任务步骤</param>
    /// <param name="currentDataCount">当前已采集的数据量</param>
    /// <returns>满足结束条件返回 true，否则返回 false</returns>
    private static bool CheckEndCondition(TaskStep step, int currentDataCount)
    {
        if (step.EndCondition == null) return false;
        var context = new Dictionary<string, object?> { ["current_data_count"] = currentDataCount };
        return step.EndCondition.IsSatisfied(currentDataCount, context);
    }

    /// <summary>
    /// 根据步骤的采集模式获取对应的下载器
    /// Playwright 模式使用 Playwright 下载器，BrowserAutomation 模式使用 CDP 下载器，其他使用默认 HTTP 下载器
    /// </summary>
    /// <param name="step">任务步骤</param>
    /// <returns>对应的下载器实例</returns>
    private IDownloader GetDownloaderForStep(TaskStep step)
    {
        return step.CollectionMode switch
        {
            CollectionMode.Playwright => _downloaderFactory.CreateDownloader(DownloadType.Playwright),
            CollectionMode.BrowserAutomation => _downloaderFactory.CreateDownloader(DownloadType.Cdp),
            _ => _downloader
        };
    }

    /// <summary>
    /// 从请求配置构建下载请求，解析 URL 模板中的变量占位符
    /// </summary>
    /// <param name="config">步骤请求配置</param>
    /// <param name="task">所属爬虫任务</param>
    /// <param name="variables">步骤变量字典</param>
    /// <returns>构建完成的请求对象，配置无效时返回 null</returns>
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

    /// <summary>
    /// 从任务步骤构建下载请求列表
    /// </summary>
    /// <param name="step">任务步骤</param>
    /// <param name="variables">步骤变量字典</param>
    /// <param name="task">所属爬虫任务</param>
    /// <returns>下载请求列表</returns>
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

    /// <summary>
    /// 处理单步任务的响应数据，使用表达式解析器或默认解析器提取数据记录
    /// </summary>
    /// <param name="response">下载响应</param>
    /// <param name="task">爬虫任务</param>
    /// <param name="expressionParser">表达式解析器，可为 null</param>
    /// <returns>提取的数据记录列表</returns>
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
    /// 处理多步骤任务的响应数据，根据提取规则或表达式配置提取数据记录
    /// </summary>
    /// <param name="response">下载响应</param>
    /// <param name="step">任务步骤</param>
    /// <param name="task">爬虫任务</param>
    /// <returns>提取的数据记录列表</returns>
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

    /// <summary>
    /// 应用步骤的变量映射，将数据记录中的字段值映射到步骤变量
    /// </summary>
    /// <param name="step">任务步骤</param>
    /// <param name="records">数据记录列表</param>
    /// <param name="stepVariables">步骤变量字典，会被修改</param>
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

    /// <summary>
    /// 根据提取规则从内容中提取数据，支持 XPath、CSS Selector、JsonPath 和 Regex 四种表达式类型
    /// </summary>
    /// <param name="content">待解析的文本内容</param>
    /// <param name="rule">提取规则配置</param>
    /// <returns>提取的结果列表</returns>
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

    /// <summary>
    /// 对提取的值应用转换规则，如 Trim、Replace、RegexReplace、LowerCase、UpperCase
    /// </summary>
    /// <param name="value">原始值</param>
    /// <param name="transform">转换规则</param>
    /// <returns>转换后的值</returns>
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
    /// 处理分页采集，根据分页配置自动翻页直到满足条件
    /// 支持页码翻页、偏移量翻页、下一页链接、点击翻页和无限滚动
    /// </summary>
    /// <param name="downloader">下载器实例</param>
    /// <param name="step">任务步骤</param>
    /// <param name="originalRequest">原始请求</param>
    /// <param name="result">执行结果容器</param>
    /// <param name="stepVariables">步骤变量字典</param>
    /// <param name="task">爬虫任务</param>
    /// <param name="retryPolicy">重试策略</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>所有分页采集的数据记录</returns>
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

        if (step.ResumeFromPage.HasValue && step.ResumeFromPage.Value > 0)
        {
            var resumePage = step.ResumeFromPage.Value;
            var skippedPages = resumePage - pagination.StartPage;
            if (skippedPages > 0)
            {
                pageCount = skippedPages / Math.Max(pagination.PageIncrement, 1);
                currentPage = resumePage;
                _logger.LogInformation("步骤 {StepName} 从断点页码 {ResumePage} 恢复采集", step.StepName, resumePage);
            }
        }

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

    /// <summary>
    /// 构建页码翻页 URL，使用 URL 模板或参数方式
    /// </summary>
    /// <param name="baseUrl">基础 URL</param>
    /// <param name="pagination">分页配置</param>
    /// <param name="pageNum">目标页码</param>
    /// <returns>构建后的分页 URL</returns>
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

    /// <summary>
    /// 构建偏移量翻页 URL，使用 URL 模板或参数方式
    /// </summary>
    /// <param name="baseUrl">基础 URL</param>
    /// <param name="pagination">分页配置</param>
    /// <param name="offset">偏移量</param>
    /// <returns>构建后的分页 URL</returns>
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

/// <summary>
/// 任务执行结果模型，包含采集数据统计、错误信息和步骤执行详情
/// </summary>
public class ExecutionResult
{
    /// <summary>
    /// 任务 ID
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 任务名称
    /// </summary>
    public string? TaskName { get; set; }

    /// <summary>
    /// 关联的表达式 ID
    /// </summary>
    public string? ExpressionId { get; set; }

    /// <summary>
    /// 执行状态，如 Success、Failed
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 总请求数
    /// </summary>
    public int TotalRequests { get; set; }

    /// <summary>
    /// 成功请求数
    /// </summary>
    public int SuccessRequests { get; set; }

    /// <summary>
    /// 失败请求数
    /// </summary>
    public int FailedRequests { get; set; }

    /// <summary>
    /// 采集到的数据记录列表
    /// </summary>
    public List<DataRecord> DataRecords { get; set; } = [];

    /// <summary>
    /// 错误信息列表
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// 执行进度百分比
    /// </summary>
    public decimal Progress { get; set; }

    /// <summary>
    /// 开始时间（UTC）
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间（UTC）
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 各步骤的执行结果列表
    /// </summary>
    public List<StepExecutionResult> StepResults { get; set; } = [];
}

/// <summary>
/// 步骤执行结果模型，记录单个步骤的执行状态和数据量
/// </summary>
public class StepExecutionResult
{
    /// <summary>
    /// 步骤 ID
    /// </summary>
    public string StepId { get; set; } = string.Empty;

    /// <summary>
    /// 步骤名称
    /// </summary>
    public string StepName { get; set; } = string.Empty;

    /// <summary>
    /// 步骤状态，如 Waiting、Running、Completed、Skipped
    /// </summary>
    public StepState State { get; set; } = StepState.Waiting;

    /// <summary>
    /// 该步骤采集的数据量
    /// </summary>
    public int DataCount { get; set; }

    /// <summary>
    /// 步骤错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }
}
