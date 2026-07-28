using SqlSugar;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;

namespace Larpx.PersonalTools.ResourceSpider.Server.Repositories;

/// <summary>
/// 代理分组数据仓储接口，定义代理分组实体的数据访问操作
/// </summary>
public interface IAgentGroupRepository
{
    /// <summary>
    /// 根据分组 ID 获取代理分组实体
    /// </summary>
    /// <param name="groupId">分组唯一标识符</param>
    /// <returns>代理分组实体，未找到时返回 null</returns>
    Task<AgentGroupEntity?> GetByIdAsync(string groupId);

    /// <summary>
    /// 获取所有代理分组，按分组名称排序
    /// </summary>
    /// <returns>代理分组列表</returns>
    Task<List<AgentGroupEntity>> GetAllAsync();

    /// <summary>
    /// 新增代理分组实体
    /// </summary>
    /// <param name="entity">代理分组实体</param>
    Task AddAsync(AgentGroupEntity entity);

    /// <summary>
    /// 更新代理分组实体，忽略 CreatedAt 字段
    /// </summary>
    /// <param name="entity">代理分组实体</param>
    Task UpdateAsync(AgentGroupEntity entity);

    /// <summary>
    /// 根据分组 ID 删除代理分组
    /// </summary>
    /// <param name="groupId">分组唯一标识符</param>
    Task DeleteAsync(string groupId);
}

/// <summary>
/// 代理分组数据仓储实现，基于 SQLSugar 提供代理分组数据的增删改查操作
/// </summary>
public class AgentGroupRepository : IAgentGroupRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 初始化代理分组仓储实例
    /// </summary>
    /// <param name="db">SQLSugar 数据库客户端</param>
    public AgentGroupRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<AgentGroupEntity?> GetByIdAsync(string groupId)
    {
        return await _db.Queryable<AgentGroupEntity>()
            .FirstAsync(g => g.GroupId == groupId);
    }

    /// <inheritdoc/>
    public async Task<List<AgentGroupEntity>> GetAllAsync()
    {
        return await _db.Queryable<AgentGroupEntity>()
            .OrderBy(g => g.GroupName)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task AddAsync(AgentGroupEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(AgentGroupEntity entity)
    {
        await _db.Updateable(entity)
            .IgnoreColumns(g => g.CreatedAt)
            .ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string groupId)
    {
        await _db.Deleteable<AgentGroupEntity>()
            .Where(g => g.GroupId == groupId)
            .ExecuteCommandAsync();
    }
}
