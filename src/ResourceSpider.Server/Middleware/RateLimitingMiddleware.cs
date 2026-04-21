using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace ResourceSpider.Server.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ConcurrentDictionary<string, SlidingWindowRateLimiter> _limiters = new();
    private readonly int _permitLimit;
    private readonly int _windowSeconds;

    public RateLimitingMiddleware(RequestDelegate next, int permitLimit = 100, int windowSeconds = 60)
    {
        _next = next;
        _permitLimit = permitLimit;
        _windowSeconds = windowSeconds;
    }

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

    private static string GetClientKey(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var agentId = context.Request.Headers.TryGetValue("X-Agent-Id", out var agentIdValue) 
            ? agentIdValue.ToString() 
            : string.Empty;
        return string.IsNullOrEmpty(agentId) ? ip : $"{ip}:{agentId}";
    }
}
