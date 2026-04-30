using ResourceSpider.Server.DTOs;

namespace ResourceSpider.Server.Components.Services;

/// <summary>
/// 后台管理会话状态
/// </summary>
public class AdminSessionState
{
    public string? Token { get; private set; }

    public string? Username { get; private set; }

    public string? Role { get; private set; }

    public DateTime? ExpiresAt { get; private set; }

    public bool IsAuthenticated =>
        !string.IsNullOrWhiteSpace(Token) &&
        ExpiresAt.HasValue &&
        ExpiresAt.Value > DateTime.UtcNow;

    public event Action? OnChange;

    public void SetSession(AuthResponse auth)
    {
        Token = auth.Token;
        Username = auth.Username;
        Role = auth.Role;
        ExpiresAt = auth.ExpiresAt;
        OnChange?.Invoke();
    }

    public void Clear()
    {
        Token = null;
        Username = null;
        Role = null;
        ExpiresAt = null;
        OnChange?.Invoke();
    }

    /// <summary>
    /// 更新当前会话的用户名
    /// </summary>
    /// <param name="username">新用户名</param>
    public void UpdateUsername(string username)
    {
        Username = username;
        OnChange?.Invoke();
    }
}
