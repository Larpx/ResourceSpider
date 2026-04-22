using SqlSugar;
using ResourceSpider.Server.Entities;

namespace ResourceSpider.Server.Repositories;

/// <summary>
/// 系统日志数据仓储接口，定义系统日志实体的数据访问操作
/// 支持按级别、分类和时间范围进行筛选查询
/// </summary>
public interface ISystemLogRepository
{
    /// <summary>
    /// 分页获取系统日志列表，支持按级别、分类和时间范围筛选
    /// </summary>
    /// <param name="pageIndex">页码，从 1 开始</param>
    /// <param name="pageSize">每页记录数</param>
    /// <param name="level">日志级别筛选，为 null 时不筛选</param>
    /// <param name="category">日志分类筛选，为 null 时不筛选</param>
    /// <param name="startDate">开始时间筛选，为 null 时不筛选</param>
    /// <param name="endDate">结束时间筛选，为 null 时不筛选</param>
    /// <returns>系统日志列表，按创建时间倒序排列</returns>
    Task<List<SystemLogEntity>> GetListAsync(int pageIndex, int pageSize, string? level = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// 统计符合条件的日志记录数
    /// </summary>
    /// <param name="level">日志级别筛选，为 null 时不筛选</param>
    /// <param name="category">日志分类筛选，为 null 时不筛选</param>
    /// <param name="startDate">开始时间筛选，为 null 时不筛选</param>
    /// <param name="endDate">结束时间筛选，为 null 时不筛选</param>
    /// <returns>符合条件的日志记录总数</returns>
    Task<long> CountAsync(string? level = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// 新增系统日志实体
    /// </summary>
    /// <param name="entity">系统日志实体</param>
    Task AddAsync(SystemLogEntity entity);
}

/// <summary>
/// 系统日志数据仓储实现，基于 SQLSugar 提供系统日志的查询和写入操作
/// </summary>
public class SystemLogRepository : ISystemLogRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 初始化系统日志仓储实例
    /// </summary>
    /// <param name="db">SQLSugar 数据库客户端</param>
    public SystemLogRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<List<SystemLogEntity>> GetListAsync(int pageIndex, int pageSize, string? level = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _db.Queryable<SystemLogEntity>();
        if (!string.IsNullOrEmpty(level))
            query = query.Where(l => l.Level == level);
        if (!string.IsNullOrEmpty(category))
            query = query.Where(l => l.Category == category);
        if (startDate.HasValue)
            query = query.Where(l => l.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(l => l.CreatedAt <= endDate.Value);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .ToPageListAsync(pageIndex, pageSize);
    }

    /// <inheritdoc/>
    public async Task<long> CountAsync(string? level = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _db.Queryable<SystemLogEntity>();
        if (!string.IsNullOrEmpty(level))
            query = query.Where(l => l.Level == level);
        if (!string.IsNullOrEmpty(category))
            query = query.Where(l => l.Category == category);
        if (startDate.HasValue)
            query = query.Where(l => l.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(l => l.CreatedAt <= endDate.Value);

        return await query.CountAsync();
    }

    /// <inheritdoc/>
    public async Task AddAsync(SystemLogEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }
}
