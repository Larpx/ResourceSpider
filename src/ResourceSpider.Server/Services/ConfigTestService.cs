using ResourceSpider.Core.Enums;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Server.Services;

public interface IConfigTestService
{
    Task<ConfigTestResult> TestExpressionAsync(ExpressionConfig expression);
    Task<ConfigTestResult> TestProxyAsync(Proxy proxy);
    Task<ConfigTestResult> TestTaskConfigAsync(SpiderTask task);
}

public class ConfigTestService : IConfigTestService
{
    private readonly IDownloaderFactory _downloaderFactory;
    private readonly IParserFactory _parserFactory;
    private readonly IProxyPool _proxyPool;
    private readonly ILogger<ConfigTestService> _logger;

    public ConfigTestService(
        IDownloaderFactory downloaderFactory,
        IParserFactory parserFactory,
        IProxyPool proxyPool,
        ILogger<ConfigTestService> logger)
    {
        _downloaderFactory = downloaderFactory;
        _parserFactory = parserFactory;
        _proxyPool = proxyPool;
        _logger = logger;
    }

    public async Task<ConfigTestResult> TestExpressionAsync(ExpressionConfig expression)
    {
        var result = new ConfigTestResult { TestType = "Expression" };

        try
        {
            if (string.IsNullOrEmpty(expression.ContainerExpression))
            {
                result.Success = false;
                result.ErrorMessage = "容器表达式不能为空";
                return result;
            }

            if (expression.Fields.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "至少需要一个字段表达式";
                return result;
            }

            var parser = _parserFactory.CreateFromExpressionConfig(expression);
            result.Success = true;
            result.Message = "表达式配置验证通过";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogWarning(ex, "表达式配置测试失败");
        }

        return result;
    }

    public async Task<ConfigTestResult> TestProxyAsync(Proxy proxy)
    {
        var result = new ConfigTestResult { TestType = "Proxy" };

        try
        {
            var isAvailable = await _proxyPool.IsAvailableAsync(proxy);
            result.Success = isAvailable;
            result.Message = isAvailable ? "代理可用" : "代理不可用";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogWarning(ex, "代理测试失败");
        }

        return result;
    }

    public async Task<ConfigTestResult> TestTaskConfigAsync(SpiderTask task)
    {
        var result = new ConfigTestResult { TestType = "TaskConfig" };

        try
        {
            if (string.IsNullOrEmpty(task.TaskName))
            {
                result.Success = false;
                result.ErrorMessage = "任务名称不能为空";
                return result;
            }

            if (task.RequestConfig == null && (task.Steps == null || task.Steps.Count == 0))
            {
                result.Success = false;
                result.ErrorMessage = "任务必须配置请求配置或步骤";
                return result;
            }

            if (task.RequestConfig != null)
            {
                if (string.IsNullOrEmpty(task.RequestConfig.UrlTemplate))
                {
                    result.Success = false;
                    result.ErrorMessage = "请求 URL 模板不能为空";
                    return result;
                }

                var downloader = task.RequestConfig.PlaywrightConfig != null
                    ? _downloaderFactory.CreateDownloader(DownloadType.Playwright)
                    : _downloaderFactory.CreateDownloader(DownloadType.HttpClient);

                var testRequest = new Request
                {
                    Url = task.RequestConfig.UrlTemplate,
                    Method = task.RequestConfig.Method ?? "GET"
                };

                using var cts = new CancellationTokenSource(10000);
                var response = await downloader.DownloadAsync(testRequest, cts.Token);

                result.Success = response.Status == RequestStatus.Success;
                result.Message = result.Success
                    ? $"请求成功，状态码: {response.StatusCode}，耗时: {response.Duration}ms"
                    : $"请求失败: {response.Error}";
                result.Details = new Dictionary<string, object?>
                {
                    ["StatusCode"] = response.StatusCode,
                    ["Duration"] = response.Duration,
                    ["ContentLength"] = response.ContentLength
                };
            }
            else
            {
                result.Success = true;
                result.Message = "多步骤任务配置验证通过";
            }
        }
        catch (OperationCanceledException)
        {
            result.Success = false;
            result.ErrorMessage = "请求超时（10秒）";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogWarning(ex, "任务配置测试失败");
        }

        return result;
    }
}

public class ConfigTestResult
{
    public string TestType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object?>? Details { get; set; }
}
