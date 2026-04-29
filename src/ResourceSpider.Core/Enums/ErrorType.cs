namespace ResourceSpider.Core.Enums;

/// <summary>
/// 错误类型枚举，对系统运行时产生的错误进行分类
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// 网络连接错误，如超时、DNS 解析失败等
    /// </summary>
    NetworkError,

    /// <summary>
    /// HTTP 协议错误，如 404、500 等状态码
    /// </summary>
    HttpError,

    /// <summary>
    /// 数据解析错误，如 XPath/CSS 选择器匹配失败
    /// </summary>
    ParseError,

    /// <summary>
    /// 代理节点错误，如 Agent 崩溃、断连等
    /// </summary>
    AgentError,

    /// <summary>
    /// 业务逻辑错误，如配置无效、参数校验失败等
    /// </summary>
    BusinessError,

    /// <summary>
    /// 请求超时错误
    /// </summary>
    Timeout
}
