namespace Larpx.PersonalTools.ResourceSpider.Core.Interfaces;

/// <summary>
/// 去重器接口，用于判断请求是否重复以避免重复爬取
/// </summary>
public interface IDuplicateRemover
{
    /// <summary>
    /// 判断指定指纹的请求是否已存在（重复）
    /// </summary>
    /// <param name="fingerprint">请求指纹</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>如果请求已存在返回 true，否则返回 false</returns>
    Task<bool> IsDuplicateAsync(string fingerprint, CancellationToken ct = default);

    /// <summary>
    /// 将请求指纹添加到去重集合中
    /// </summary>
    /// <param name="fingerprint">请求指纹</param>
    /// <param name="ct">取消令牌</param>
    Task AddAsync(string fingerprint, CancellationToken ct = default);

    /// <summary>
    /// 获取当前去重集合中的请求数量
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>已记录的请求数量</returns>
    Task<long> GetCountAsync(CancellationToken ct = default);
}
