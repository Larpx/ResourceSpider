using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using Larpx.PersonalTools.ResourceSpider.Server.Repositories;

namespace Larpx.PersonalTools.ResourceSpider.Server.Services;

public interface IAuditLogService
{
    Task LogAsync(string action, string category, string userId, string? details = null, string? targetId = null);
    Task<List<AuditLogEntry>> GetAuditLogsAsync(string? userId = null, string? category = null, int pageIndex = 0, int pageSize = 20);
}

public class AuditLogService : IAuditLogService
{
    private readonly ISystemLogRepository _systemLogRepository;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        ISystemLogRepository systemLogRepository,
        ILogger<AuditLogService> logger)
    {
        _systemLogRepository = systemLogRepository;
        _logger = logger;
    }

    public async Task LogAsync(string action, string category, string userId, string? details = null, string? targetId = null)
    {
        _logger.LogInformation("审计日志: {Action} [{Category}] by {UserId}, Target: {TargetId}", action, category, userId, targetId);

        await _systemLogRepository.AddAsync(new SystemLogEntity
        {
            Level = "Info",
            Category = $"Audit:{category}",
            Message = action,
            Detail = details,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<List<AuditLogEntry>> GetAuditLogsAsync(string? userId = null, string? category = null, int pageIndex = 0, int pageSize = 20)
    {
        var logs = await _systemLogRepository.GetPagedAsync(pageIndex, pageSize, category ?? "Audit:");
        return logs.Select(l => new AuditLogEntry
        {
            Id = l.Id,
            Action = l.Message,
            Category = l.Category,
            UserId = l.UserId,
            Details = l.Detail,
            CreatedAt = l.CreatedAt
        }).ToList();
    }
}

public class AuditLogEntry
{
    public long Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
