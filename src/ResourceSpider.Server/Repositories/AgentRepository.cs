using ResourceSpider.Server.Entities;
using SqlSugar;

namespace ResourceSpider.Server.Repositories;

/// <summary>
/// 代理节点数据仓储接口，定义代理节点实体的数据访问操作
/// 支持代理节点的增删改查、在线节点查询和统计
/// </summary>
public interface IAgentRepository
{
    /// <summary>
    /// 根据代理节点 ID 获取代理节点实体
    /// </summary>
    /// <param name="agentId">代理节点唯一标识符</param>
    /// <returns>代理节点实体，未找到时返回 null</returns>
    Task<AgentEntity?> GetByIdAsync(string agentId);

    /// <summary>
    /// 根据认证令牌获取代理节点实体，用于代理节点身份验证
    /// </summary>
    /// <param name="token">代理节点认证令牌</param>
    /// <returns>代理节点实体，未找到时返回 null</returns>
    Task<AgentEntity?> GetByTokenAsync(string token);

    /// <summary>
    /// 获取所有代理节点
    /// </summary>
    /// <returns>代理节点列表</returns>
    Task<List<AgentEntity>> GetAllAsync();

    /// <summary>
    /// 获取所有在线状态的代理节点
    /// </summary>
    /// <returns>在线代理节点列表</returns>
    Task<List<AgentEntity>> GetOnlineAsync();

    Task<List<AgentEntity>> GetOnlineAgentsAsync();

    /// <summary>
    /// 新增代理节点实体
    /// </summary>
    /// <param name="entity">代理节点实体</param>
    Task AddAsync(AgentEntity entity);

    /// <summary>
    /// 更新代理节点实体，忽略 CreatedAt 字段
    /// </summary>
    /// <param name="entity">代理节点实体</param>
    Task UpdateAsync(AgentEntity entity);

    /// <summary>
    /// 根据代理节点 ID 删除代理节点
    /// </summary>
    /// <param name="agentId">代理节点唯一标识符</param>
    Task DeleteAsync(string agentId);

    /// <summary>
    /// 统计代理节点总数
    /// </summary>
    /// <returns>代理节点总数</returns>
    Task<long> CountAsync();

    /// <summary>
    /// 统计在线代理节点数量
    /// </summary>
    /// <returns>在线代理节点数量</returns>
    Task<long> CountOnlineAsync();
}

/// <summary>
/// 代理节点数据仓储实现，基于 SQLSugar 提供代理节点的增删改查操作
/// </summary>
public class AgentRepository : IAgentRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly SqlSugarClient _db;

    /// <summary>
    /// 初始化代理节点仓储实例
    /// </summary>
    /// <param name="sqlSugarClient">SQLSugar 数据库客户端</param>
    public AgentRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    /// <inheritdoc/>
    public async Task<AgentEntity?> GetByIdAsync(string agentId)
    {
        return await _db.Queryable<AgentEntity>()
            .FirstAsync(x => x.AgentId == agentId);
    }

    /// <inheritdoc/>
    public async Task<AgentEntity?> GetByTokenAsync(string token)
    {
        return await _db.Queryable<AgentEntity>()
            .FirstAsync(x => x.AgentToken == token);
    }

    /// <inheritdoc/>
    public async Task<List<AgentEntity>> GetAllAsync()
    {
        return await _db.Queryable<AgentEntity>().ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<AgentEntity>> GetOnlineAsync()
    {
        return await _db.Queryable<AgentEntity>()
            .Where(x => x.Status == 1)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<AgentEntity>> GetOnlineAgentsAsync()
    {
        return await GetOnlineAsync();
    }

    /// <inheritdoc/>
    public async Task AddAsync(AgentEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(AgentEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.Updateable(entity)
            .IgnoreColumns(x => x.CreatedAt)
            .ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string agentId)
    {
        await _db.Deleteable<AgentEntity>()
            .Where(x => x.AgentId == agentId)
            .ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task<long> CountAsync()
    {
        return await _db.Queryable<AgentEntity>().CountAsync();
    }

    /// <inheritdoc/>
    public async Task<long> CountOnlineAsync()
    {
        return await _db.Queryable<AgentEntity>()
            .Where(x => x.Status == 1)
            .CountAsync();
    }
}
