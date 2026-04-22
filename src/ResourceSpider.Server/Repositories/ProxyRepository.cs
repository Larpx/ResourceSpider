using ResourceSpider.Server.Entities;
using SqlSugar;

namespace ResourceSpider.Server.Repositories;

/// <summary>
/// 代理服务器数据仓储接口，定义代理服务器实体的数据访问操作
/// 支持代理的增删改查和可用代理查询
/// </summary>
public interface IProxyRepository
{
    /// <summary>
    /// 根据代理 ID 获取代理服务器实体
    /// </summary>
    /// <param name="proxyId">代理唯一标识符</param>
    /// <returns>代理服务器实体，未找到时返回 null</returns>
    Task<ProxyEntity?> GetByIdAsync(string proxyId);

    /// <summary>
    /// 分页获取代理服务器列表
    /// </summary>
    /// <param name="pageIndex">页码，从 1 开始</param>
    /// <param name="pageSize">每页记录数</param>
    /// <returns>代理服务器列表，按创建时间倒序排列</returns>
    Task<List<ProxyEntity>> GetAllAsync(int pageIndex, int pageSize);

    /// <summary>
    /// 统计代理服务器总数
    /// </summary>
    /// <returns>代理服务器总数</returns>
    Task<long> CountAsync();

    /// <summary>
    /// 获取所有可用状态的代理服务器
    /// </summary>
    /// <returns>可用代理服务器列表</returns>
    Task<List<ProxyEntity>> GetAvailableAsync();

    /// <summary>
    /// 新增代理服务器实体
    /// </summary>
    /// <param name="entity">代理服务器实体</param>
    Task AddAsync(ProxyEntity entity);

    /// <summary>
    /// 更新代理服务器实体，忽略 CreatedAt 字段
    /// </summary>
    /// <param name="entity">代理服务器实体</param>
    Task UpdateAsync(ProxyEntity entity);

    /// <summary>
    /// 根据代理 ID 删除代理服务器
    /// </summary>
    /// <param name="proxyId">代理唯一标识符</param>
    Task DeleteAsync(string proxyId);
}

/// <summary>
/// 代理服务器数据仓储实现，基于 SQLSugar 提供代理服务器的增删改查操作
/// </summary>
public class ProxyRepository : IProxyRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly SqlSugarClient _db;

    /// <summary>
    /// 初始化代理服务器仓储实例
    /// </summary>
    /// <param name="sqlSugarClient">SQLSugar 数据库客户端</param>
    public ProxyRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    /// <inheritdoc/>
    public async Task<ProxyEntity?> GetByIdAsync(string proxyId)
    {
        return await _db.Queryable<ProxyEntity>()
            .FirstAsync(x => x.ProxyId == proxyId);
    }

    /// <inheritdoc/>
    public async Task<List<ProxyEntity>> GetAllAsync(int pageIndex, int pageSize)
    {
        return await _db.Queryable<ProxyEntity>()
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<long> CountAsync()
    {
        return await _db.Queryable<ProxyEntity>().CountAsync();
    }

    /// <inheritdoc/>
    public async Task<List<ProxyEntity>> GetAvailableAsync()
    {
        return await _db.Queryable<ProxyEntity>()
            .Where(x => x.Status == 1)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task AddAsync(ProxyEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(ProxyEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.Updateable(entity)
            .IgnoreColumns(x => x.CreatedAt)
            .ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string proxyId)
    {
        await _db.Deleteable<ProxyEntity>()
            .Where(x => x.ProxyId == proxyId)
            .ExecuteCommandAsync();
    }
}
