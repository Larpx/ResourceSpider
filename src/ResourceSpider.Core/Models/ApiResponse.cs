namespace ResourceSpider.Core.Models;

/// <summary>
/// 统一 API 响应模型，所有 API 接口统一使用此格式返回数据
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 响应状态码，200 表示成功
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 响应时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static ApiResponse<T> Success(T data, string message = "操作成功")
    {
        return new ApiResponse<T> { Code = 200, Message = message, Data = data };
    }

    /// <summary>
    /// 创建错误响应
    /// </summary>
    public static ApiResponse<T> Error(int code, string message)
    {
        return new ApiResponse<T> { Code = code, Message = message };
    }
}

/// <summary>
/// 无数据的 API 响应模型
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// 创建成功响应（无数据）
    /// </summary>
    public static ApiResponse Success(string message = "操作成功")
    {
        return new ApiResponse { Code = 200, Message = message };
    }

    /// <summary>
    /// 创建错误响应（无数据）
    /// </summary>
    public static new ApiResponse Error(int code, string message)
    {
        return new ApiResponse { Code = code, Message = message };
    }
}
