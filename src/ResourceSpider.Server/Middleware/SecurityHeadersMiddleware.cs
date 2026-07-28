namespace Larpx.PersonalTools.ResourceSpider.Server.Middleware;

/// <summary>
/// 安全头中间件，为每个 HTTP 响应添加安全相关的响应头
/// 包括 XSS 防护、内容类型嗅探防护、点击劫持防护等
/// </summary>
public class SecurityHeadersMiddleware
{
    /// <summary>
    /// 下一个中间件委托
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// 初始化安全头中间件
    /// </summary>
    /// <param name="next">下一个中间件委托</param>
    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// 处理 HTTP 请求，在响应中添加安全头信息
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        var path = context.Request.Path;
        var isSwaggerPath = path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase);

        context.Response.Headers["Content-Security-Policy"] = isSwaggerPath
            ? "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self' data:"
            : "default-src 'self'; script-src 'self'; style-src 'self'";

        await _next(context);
    }
}
