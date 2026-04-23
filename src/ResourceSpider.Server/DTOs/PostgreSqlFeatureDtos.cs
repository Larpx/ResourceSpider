namespace ResourceSpider.Server.DTOs;

public record PostgreSqlResultStorageStatusDto(
    bool Enabled,
    bool Configured,
    bool Connected,
    string Status,
    string? LastError = null,
    string? LastConfigWriteError = null,
    string? EffectiveConfigFile = null
);

public record UpdatePostgreSqlResultStorageRequest(bool Enabled);
