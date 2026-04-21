using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

public record LoginRequest(
    [Required, StringLength(128)] string Username,
    [Required, StringLength(128)] string Password
);

public record RegisterRequest(
    [Required, StringLength(128)] string Username,
    [Required, StringLength(128)] string Password,
    [StringLength(64)] string Role = "Operator"
);

public record AuthResponse(
    string Token,
    string Username,
    string Role,
    DateTime ExpiresAt
);

public record UserInfoDto(
    string UserId,
    string Username,
    string Role,
    int Status,
    DateTime CreatedAt
);
