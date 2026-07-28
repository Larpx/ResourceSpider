using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Repositories;

/// <summary>
/// 统计数据仓储接口，定义统计实体的数据访问操作
/// 支持按代理节点和日期维度查询统计数据，以及系统级趋势分析
/// </summary>
public interface IStatisticRepository
{
    /// <summary>
    /// 根据代理节点 ID 和日期获取统计实体
    /// </summary>
    /// <param name="agentId">代理节点唯一标识符</param>
    /// <param name="date">统计日期</param>
    /// <returns>统计实体，未找到时返回 null</returns>
    Task<StatisticEntity?> GetByAgentAndDateAsync(string agentId, DateTime date);

    /// <summary>
    /// 根据代理节点 ID 和日期范围获取统计列表
    /// </summary>
    /// <param name="agentId">代理节点唯一标识符</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>统计列表，按日期正序排列</returns>
    Task<List<StatisticEntity>> GetByAgentAsync(string agentId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// 新增统计实体
    /// </summary>
    /// <param name="entity">统计实体</param>
    Task AddAsync(StatisticEntity entity);

    /// <summary>
    /// 更新统计实体
    /// </summary>
    /// <param name="entity">统计实体</param>
    Task UpdateAsync(StatisticEntity entity);

    /// <summary>
    /// 获取系统级趋势统计数据，按日期范围汇总所有代理节点
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>统计列表，按日期正序排列</returns>
    Task<List<StatisticEntity>> GetSystemTrendAsync(DateTime startDate, DateTime endDate);
}

/// <summary>
/// 统计数据仓储实现，基于 SQLSugar 提供统计数据的增删改查操作
/// </summary>
public class StatisticRepository : IStatisticRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly SqlSugarClient _db;

    /// <summary>
    /// 初始化统计数据仓储实例
    /// </summary>
    /// <param name="sqlSugarClient">SQLSugar 数据库客户端</param>
    public StatisticRepository(ISqlSugarClient sqlSugarClient)
    {
        _db = (SqlSugarClient)sqlSugarClient;
    }

    /// <inheritdoc/>
    public async Task<StatisticEntity?> GetByAgentAndDateAsync(string agentId, DateTime date)
    {
        return await _db.Queryable<StatisticEntity>()
            .FirstAsync(x => x.AgentId == agentId && x.StatDate == date);
    }

    /// <inheritdoc/>
    public async Task<List<StatisticEntity>> GetByAgentAsync(
        string agentId, DateTime startDate, DateTime endDate)
    {
        return await _db.Queryable<StatisticEntity>()
            .Where(x => x.AgentId == agentId 
                && x.StatDate >= startDate 
                && x.StatDate <= endDate)
            .OrderBy(x => x.StatDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task AddAsync(StatisticEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(StatisticEntity entity)
    {
        await _db.Updateable(entity).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task<List<StatisticEntity>> GetSystemTrendAsync(
        DateTime startDate, DateTime endDate)
    {
        return await _db.Queryable<StatisticEntity>()
            .Where(x => x.StatDate >= startDate && x.StatDate <= endDate)
            .OrderBy(x => x.StatDate)
            .ToListAsync();
    }
}
