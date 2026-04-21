namespace ResourceSpider.Server.DTOs;

/// <summary>
/// 服务端 API 响应模型，继承自 Core 层的统一响应模型
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class ApiResponse<T> : Core.Models.ApiResponse<T>
{
}

/// <summary>
/// 无数据的 API 响应模型
/// </summary>
public class ApiResponse : Core.Models.ApiResponse
{
}
