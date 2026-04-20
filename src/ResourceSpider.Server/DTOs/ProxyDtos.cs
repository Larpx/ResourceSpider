using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

public record CreateProxyRequest(
    [Required, StringLength(255)] string Host,
    [Required, Range(1, 65535)] int Port,
    [StringLength(16)] string Protocol = "HTTP",
    string? Username = null,
    string? Password = null
);

public record ProxyDto(
    string ProxyId,
    string Host,
    int Port,
    string Protocol,
    string? Username,
    int Status,
    int SuccessCount,
    int FailureCount,
    DateTime? LastCheckedAt,
    DateTime? NextCheckAt
);

public record ProxyTestRequest(
    string? ProxyId = null,
    string? Host = null,
    int? Port = null
);

public record ProxyTestResponse(
    bool IsAvailable,
    int? DurationMs,
    string? Error
);
