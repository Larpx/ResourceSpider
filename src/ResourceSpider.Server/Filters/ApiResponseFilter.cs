using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ResourceSpider.Server.DTOs;

namespace ResourceSpider.Server.Filters;

public class ApiResponseFilter : IAsyncResultFilter
{
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
