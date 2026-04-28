namespace ResourceSpider.Server.Components.Services;

/// <summary>
/// 后台管理通知项。
/// </summary>
/// <param name="Id">通知唯一标识</param>
/// <param name="Title">标题</param>
/// <param name="Message">内容</param>
/// <param name="Level">级别（success/info/warning/error）</param>
/// <param name="CreatedAt">创建时间</param>
/// <param name="RepeatCount">重复次数，1 表示首次出现</param>
/// <param name="ToastExpiresAt">Toast 自动收起时间</param>
public record AdminNotificationItem(
    Guid Id,
    string Title,
    string Message,
    string Level,
    DateTime CreatedAt,
    int RepeatCount,
    DateTime ToastExpiresAt
)
{
    /// <summary>
    /// 当前 Toast 剩余进度百分比（0-100）。
    /// </summary>
    public double ToastRemainingPercent
    {
        get
        {
            var totalMs = Math.Max((ToastExpiresAt - CreatedAt).TotalMilliseconds, 1);
            var remainingMs = Math.Max((ToastExpiresAt - DateTime.Now).TotalMilliseconds, 0);
            return remainingMs / totalMs * 100d;
        }
    }
}

/// <summary>
/// 后台管理通知服务，统一提供右上角 Toast 与通知中心数据。
/// </summary>
public class AdminNotificationService
{
    private const int MaxNotifications = 50;
    private static readonly TimeSpan ToastLifetime = TimeSpan.FromSeconds(5);
    private readonly List<AdminNotificationItem> _items = [];

    public event Action? OnChange;

    /// <summary>
    /// 当前通知列表，按最新优先。
    /// </summary>
    public IReadOnlyList<AdminNotificationItem> Items => _items;

    /// <summary>
    /// 当前仍应显示为 Toast 的通知。
    /// </summary>
    public IReadOnlyList<AdminNotificationItem> VisibleToastItems => _items
        .Where(x => x.ToastExpiresAt > DateTime.Now)
        .Take(5)
        .ToList();

    /// <summary>
    /// 添加成功通知。
    /// </summary>
    public void Success(string title, string message) => Add(title, message, "success");

    /// <summary>
    /// 添加信息通知。
    /// </summary>
    public void Info(string title, string message) => Add(title, message, "info");

    /// <summary>
    /// 添加警告通知。
    /// </summary>
    public void Warning(string title, string message) => Add(title, message, "warning");

    /// <summary>
    /// 添加错误通知。
    /// </summary>
    public void Error(string title, string message) => Add(title, message, "error");

    /// <summary>
    /// 删除指定通知。
    /// </summary>
    public void Remove(Guid id)
    {
        var removed = _items.RemoveAll(x => x.Id == id) > 0;
        if (removed)
        {
            NotifyChanged();
        }
    }

    /// <summary>
    /// 清空通知。
    /// </summary>
    public void Clear()
    {
        if (_items.Count == 0)
        {
            return;
        }

        _items.Clear();
        NotifyChanged();
    }

    private void Add(string title, string message, string level)
    {
        var now = DateTime.Now;
        var existingIndex = _items.FindIndex(x => x.Title == title && x.Message == message && x.Level == level);
        if (existingIndex >= 0)
        {
            var existing = _items[existingIndex];
            var merged = existing with
            {
                CreatedAt = now,
                RepeatCount = existing.RepeatCount + 1,
                ToastExpiresAt = now.Add(ToastLifetime)
            };

            _items.RemoveAt(existingIndex);
            _items.Insert(0, merged);
            NotifyChanged();
            _ = NotifyToastExpiredAsync(merged.Id, merged.ToastExpiresAt);
            return;
        }

        var item = new AdminNotificationItem(
            Guid.NewGuid(),
            title,
            message,
            level,
            now,
            1,
            now.Add(ToastLifetime));

        _items.Insert(0, item);
        if (_items.Count > MaxNotifications)
        {
            _items.RemoveRange(MaxNotifications, _items.Count - MaxNotifications);
        }

        NotifyChanged();
        _ = NotifyToastExpiredAsync(item.Id, item.ToastExpiresAt);
    }

    private async Task NotifyToastExpiredAsync(Guid id, DateTime expiresAt)
    {
        var delay = expiresAt - DateTime.Now;
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay);
        }

        var stillExists = _items.Any(x => x.Id == id && x.ToastExpiresAt == expiresAt);
        if (stillExists)
        {
            NotifyChanged();
        }
    }

    private void NotifyChanged()
    {
        OnChange?.Invoke();
    }
}
