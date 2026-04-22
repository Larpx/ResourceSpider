namespace ResourceSpider.Core.Exceptions;

/// <summary>
/// 爬虫基础异常类，所有爬虫相关自定义异常的基类
/// </summary>
public class SpiderException : Exception
{
    /// <summary>
    /// 初始化爬虫异常
    /// </summary>
    /// <param name="message">异常消息</param>
    public SpiderException(string message) : base(message)
    {
    }

    /// <summary>
    /// 初始化爬虫异常，包含内部异常
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">导致当前异常的内部异常</param>
    public SpiderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
