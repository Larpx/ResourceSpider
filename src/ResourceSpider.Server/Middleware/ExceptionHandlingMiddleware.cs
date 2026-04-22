using System.Net;
using System.Text.Json;

namespace ResourceSpider.Server.Middleware;

/// <summary>
/// 全局异常处理中间件，捕获未处理的异常并返回统一的错误响应
/// 避免向客户端暴露异常详细信息
/// </summary>
public class ExceptionHandlingMiddleware
{
    /// <summary>
    /// 下一个中间件委托
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// 初始化异常处理中间件
    /// </summary>
    /// <param name="next">下一个中间件委托</param>
    /// <param name="logger">日志记录器</param>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// 处理 HTTP 请求，捕获异常并返回统一格式的错误响应
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// 生成统一的错误响应，不暴露异常详细信息
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    /// <param name="exception">捕获的异常</param>
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            code = 500,
            message = "服务器内部错误",
            data = (object?)null,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
