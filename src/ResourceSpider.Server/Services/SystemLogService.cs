using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface ISystemLogService
{
    Task<SystemLogListResponse> GetListAsync(int pageIndex, int pageSize, string? level = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null);
    Task LogAsync(string level, string category, string message, Dictionary<string, object?>? detail = null, string? userId = null);
}

public class SystemLogService : ISystemLogService
{
    private readonly ISystemLogRepository _repository;

    public SystemLogService(ISystemLogRepository repository)
    {
        _repository = repository;
    }

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
