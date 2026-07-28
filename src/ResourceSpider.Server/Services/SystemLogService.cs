using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using Larpx.PersonalTools.ResourceSpider.Server.Repositories;

namespace Larpx.PersonalTools.ResourceSpider.Server.Services;

/// <summary>
/// 系统日志服务接口，提供日志记录和查询功能
/// </summary>
public interface ISystemLogService
{
    /// <summary>
    /// 分页查询系统日志，支持按级别、类别和时间范围筛选
    /// </summary>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="level">日志级别筛选（如 Error、Warning、Information）</param>
    /// <param name="category">日志类别筛选</param>
    /// <param name="startDate">起始时间筛选</param>
    /// <param name="endDate">结束时间筛选</param>
    /// <returns>系统日志列表响应</returns>
    Task<SystemLogListResponse> GetListAsync(int pageIndex, int pageSize, string? level = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// 记录一条系统日志
    /// </summary>
    /// <param name="level">日志级别（如 Error、Warning、Information）</param>
    /// <param name="category">日志类别</param>
    /// <param name="message">日志消息</param>
    /// <param name="detail">附加详情字典，序列化为 JSON 存储</param>
    /// <param name="userId">关联的用户标识</param>
    Task LogAsync(string level, string category, string message, Dictionary<string, object?>? detail = null, string? userId = null);
}

/// <summary>
/// 系统日志服务实现，提供日志的持久化存储和分页查询功能
/// </summary>
public class SystemLogService : ISystemLogService
{
    /// <summary>
    /// 系统日志数据仓库，用于日志实体的持久化操作
    /// </summary>
    private readonly ISystemLogRepository _repository;

    /// <summary>
    /// 初始化系统日志服务实例
    /// </summary>
    /// <param name="repository">系统日志数据仓库</param>
    public SystemLogService(ISystemLogRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<SystemLogListResponse> GetListAsync(int pageIndex, int pageSize, string? level = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var logs = await _repository.GetListAsync(pageIndex, pageSize, level, category, startDate, endDate);
        var total = await _repository.CountAsync(level, category, startDate, endDate);

        return new SystemLogListResponse(
            logs.Select(MapToDto).ToList(),
            (int)total,
            pageIndex,
            pageSize);
    }

    /// <inheritdoc />
    public async Task LogAsync(string level, string category, string message, Dictionary<string, object?>? detail = null, string? userId = null)
    {
        var entity = new SystemLogEntity
        {
            Level = level,
            Category = category,
            Message = message,
            Detail = detail != null ? Newtonsoft.Json.JsonConvert.SerializeObject(detail) : null,
            UserId = userId
        };

        await _repository.AddAsync(entity);
    }

    /// <summary>
    /// 将系统日志实体映射为系统日志 DTO
    /// </summary>
    /// <param name="entity">系统日志实体</param>
    /// <returns>系统日志 DTO</returns>
    private static SystemLogDto MapToDto(SystemLogEntity entity)
    {
        return new SystemLogDto(
            entity.Id.ToString(),
            entity.Level,
            entity.Category,
            entity.Message,
            entity.Detail,
            entity.UserId,
            entity.CreatedAt);
    }
}
