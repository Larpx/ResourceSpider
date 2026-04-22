namespace ResourceSpider.Core.Interfaces;

/// <summary>
/// 并发控制器接口，用于管理爬虫任务的并发执行
/// </summary>
public interface IConcurrentController
{
    /// <summary>
    /// 启动并发控制器
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>
    /// 停止并发控制器
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task StopAsync(CancellationToken ct = default);

    /// <summary>
    /// 获取当前并发数
    /// </summary>
    /// <returns>当前正在执行的任务数量</returns>
    int GetCurrentConcurrency();

    /// <summary>
    /// 最大并发数，控制同时执行的任务上限
    /// </summary>
    int MaxConcurrency { get; set; }
}
