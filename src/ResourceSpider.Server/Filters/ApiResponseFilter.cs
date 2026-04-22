using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ResourceSpider.Server.DTOs;

namespace ResourceSpider.Server.Filters;

/// <summary>
/// API 响应过滤器，自动将控制器返回值包装为统一的 ApiResponse 格式
/// 确保所有 API 响应都遵循统一的响应结构
/// </summary>
public class ApiResponseFilter : IAsyncResultFilter
{
    /// <summary>
    /// 处理结果执行，将非 ApiResponse 类型的返回值自动包装
    /// 如果返回值已经是 ApiResponse 类型则跳过包装
    /// </summary>
    /// <param name="context">结果执行上下文</param>
    /// <param name="next">下一个执行委托</param>
    /// <returns>异步任务</returns>
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.Value != null && 
                objectResult.Value.GetType().IsGenericType && 
                objectResult.Value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                await next();
                return;
            }

            var statusCode = objectResult.StatusCode ?? 200;
            var response = statusCode >= 200 && statusCode < 300
                ? ApiResponse<object>.Success(objectResult.Value!, "操作成功")
                : ApiResponse<object>.Error(statusCode, "操作失败");

            objectResult.Value = response;
        }

        await next();
    }
}
