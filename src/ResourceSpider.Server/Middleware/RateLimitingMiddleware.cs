using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace ResourceSpider.Server.Middleware;

/// <summary>
/// 请求限流中间件，基于客户端标识（IP 或 Agent ID）进行滑动窗口限流
/// 防止客户端发送过多请求导致服务器过载
/// </summary>
public class RateLimitingMiddleware
{
    /// <summary>
    /// 下一个中间件委托
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// 限流器字典，键为客户端标识，值为对应的滑动窗口限流器
    /// </summary>
    private readonly ConcurrentDictionary<string, SlidingWindowRateLimiter> _limiters = new();

    /// <summary>
    /// 每个时间窗口允许的最大请求数
    /// </summary>
    private readonly int _permitLimit;

    /// <summary>
    /// 滑动窗口的时间长度（秒）
    /// </summary>
    private readonly int _windowSeconds;

    /// <summary>
    /// 初始化请求限流中间件
    /// </summary>
    /// <param name="next">下一个中间件委托</param>
    /// <param name="permitLimit">每个窗口最大请求数，默认 100</param>
    /// <param name="windowSeconds">窗口时间（秒），默认 60</param>
    public RateLimitingMiddleware(RequestDelegate next, int permitLimit = 100, int windowSeconds = 60)
    {
        _next = next;
        _permitLimit = permitLimit;
        _windowSeconds = windowSeconds;
    }

    /// <summary>
    /// 获取或创建指定客户端标识的滑动窗口限流器
    /// </summary>
    /// <param name="key">客户端标识</param>
    /// <returns>滑动窗口限流器实例</returns>
    private SlidingWindowRateLimiter GetLimiter(string key)
    {
        return _limiters.GetOrAdd(key, _ => new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
        {
            PermitLimit = _permitLimit,
            Window = TimeSpan.FromSeconds(_windowSeconds),
            SegmentsPerWindow = 10,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        }));
    }

    /// <summary>
    /// 处理 HTTP 请求，根据客户端标识进行限流判断
    /// 超出限制时返回 429 状态码
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var clientKey = GetClientKey(context);
        var limiter = GetLimiter(clientKey);
        
        using var lease = await limiter.AcquireAsync(1);
        
        if (!lease.IsAcquired)
        {
            context.Response.StatusCode = 429;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                code = 429,
                message = "请求过于频繁",
                data = (object?)null,
                timestamp = DateTime.UtcNow
            };
            
            var json = System.Text.Json.JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
            return;
        }

        await _next(context);
    }

    /// <summary>
    /// 生成客户端唯一标识，优先使用请求头中的 Agent ID，否则使用客户端 IP
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    /// <returns>客户端标识字符串</returns>
    private static string GetClientKey(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var agentId = context.Request.Headers.TryGetValue("X-Agent-Id", out var agentIdValue) 
            ? agentIdValue.ToString() 
            : string.Empty;
        return string.IsNullOrEmpty(agentId) ? ip : $"{ip}:{agentId}";
    }
}
