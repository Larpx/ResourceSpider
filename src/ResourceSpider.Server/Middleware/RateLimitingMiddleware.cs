using System.Threading.RateLimiting;

namespace ResourceSpider.Server.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SlidingWindowRateLimiter _rateLimiter;

    public RateLimitingMiddleware(RequestDelegate next, int permitLimit = 100, int windowSeconds = 60)
    {
        _next = next;
        _rateLimiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromSeconds(windowSeconds),
            SegmentsPerWindow = 10,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var lease = await _rateLimiter.AcquireAsync(1);
        
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
}
