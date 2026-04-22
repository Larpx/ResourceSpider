using SqlSugar;
using ResourceSpider.Server.Entities;

namespace ResourceSpider.Server.Repositories;

/// <summary>
/// 配置版本数据仓储接口，定义配置版本实体的数据访问操作
/// 支持按任务查询配置版本历史和按版本号获取特定版本
/// </summary>
public interface IConfigVersionRepository
{
    /// <summary>
    /// 根据任务 ID 获取配置版本列表，按版本号倒序排列
    /// </summary>
    /// <param name="taskId">任务唯一标识符</param>
    /// <returns>配置版本列表</returns>
    Task<List<ConfigVersionEntity>> GetByTaskIdAsync(string taskId);

    /// <summary>
    /// 根据任务 ID 和版本号获取特定配置版本
    /// </summary>
    /// <param name="taskId">任务唯一标识符</param>
    /// <param name="version">版本号</param>
    /// <returns>配置版本实体，未找到时返回 null</returns>
    Task<ConfigVersionEntity?> GetByVersionAsync(string taskId, int version);

    /// <summary>
    /// 新增配置版本实体
    /// </summary>
    /// <param name="entity">配置版本实体</param>
    Task AddAsync(ConfigVersionEntity entity);
}

/// <summary>
/// 配置版本数据仓储实现，基于 SQLSugar 提供配置版本的查询和写入操作
/// </summary>
public class ConfigVersionRepository : IConfigVersionRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 初始化配置版本仓储实例
    /// </summary>
    /// <param name="db">SQLSugar 数据库客户端</param>
    public ConfigVersionRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<List<ConfigVersionEntity>> GetByTaskIdAsync(string taskId)
    {
        return await _db.Queryable<ConfigVersionEntity>()
            .Where(v => v.TaskId == taskId)
            .OrderByDescending(v => v.Version)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<ConfigVersionEntity?> GetByVersionAsync(string taskId, int version)
    {
        return await _db.Queryable<ConfigVersionEntity>()
            .FirstAsync(v => v.TaskId == taskId && v.Version == version);
    }

    /// <inheritdoc/>
    public async Task AddAsync(ConfigVersionEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }
}
