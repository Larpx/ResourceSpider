using Larpx.PersonalTools.ResourceSpider.Core.Models;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using Larpx.PersonalTools.ResourceSpider.Server.Repositories;

namespace Larpx.PersonalTools.ResourceSpider.Server.Services;

public interface INotificationService
{
    Task SendAlertAsync(string level, string title, string message, string? details = null);
    Task SendTaskAlertAsync(string taskId, string taskName, string alertType, string message);
    Task SendAgentAlertAsync(string agentId, string agentName, string alertType, string message);
    Task<List<NotificationRecord>> GetRecentNotificationsAsync(int count = 50);
    Task MarkAsReadAsync(string notificationId);
}

public class NotificationService : INotificationService
{
    private readonly ISystemLogRepository _systemLogRepository;
    private readonly ILogger<NotificationService> _logger;
    private readonly List<NotificationRecord> _recentNotifications = new();
    private readonly object _notificationLock = new();
    private const int MaxRecentNotifications = 200;

    public NotificationService(
        ISystemLogRepository systemLogRepository,
        ILogger<NotificationService> logger)
    {
        _systemLogRepository = systemLogRepository;
        _logger = logger;
    }

    public async Task SendAlertAsync(string level, string title, string message, string? details = null)
    {
        _logger.LogInformation("发送告警: [{Level}] {Title} - {Message}", level, title, message);

        var notification = new NotificationRecord
        {
            NotificationId = Guid.NewGuid().ToString("N"),
            Level = level,
            Title = title,
            Message = message,
            Details = details,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        lock (_notificationLock)
        {
            _recentNotifications.Insert(0, notification);
            if (_recentNotifications.Count > MaxRecentNotifications)
            {
                _recentNotifications.RemoveAt(_recentNotifications.Count - 1);
            }
        }

        await _systemLogRepository.AddAsync(new SystemLogEntity
        {
            Level = level,
            Category = "Notification",
            Message = $"[{title}] {message}",
            Detail = details,
            UserId = null,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task SendTaskAlertAsync(string taskId, string taskName, string alertType, string message)
    {
        await SendAlertAsync("Warning", $"任务告警: {taskName}",
            $"任务 {taskId} 触发 {alertType} 告警: {message}",
            $"TaskId: {taskId}, TaskName: {taskName}, AlertType: {alertType}");
    }

    public async Task SendAgentAlertAsync(string agentId, string agentName, string alertType, string message)
    {
        await SendAlertAsync("Warning", $"Agent 告警: {agentName}",
            $"Agent {agentId} 触发 {alertType} 告警: {message}",
            $"AgentId: {agentId}, AgentName: {agentName}, AlertType: {alertType}");
    }

    public Task<List<NotificationRecord>> GetRecentNotificationsAsync(int count = 50)
    {
        lock (_notificationLock)
        {
            return Task.FromResult(_recentNotifications.Take(count).ToList());
        }
    }

    public Task MarkAsReadAsync(string notificationId)
    {
        lock (_notificationLock)
        {
            var notification = _recentNotifications.FirstOrDefault(n => n.NotificationId == notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
            }
        }
        return Task.CompletedTask;
    }
}

public class NotificationRecord
{
    public string NotificationId { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
