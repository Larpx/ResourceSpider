namespace Larpx.PersonalTools.ResourceSpider.Server.DTOs;

public record RedisFeatureStatusDto(
    bool Enabled,
    bool Configured,
    bool Connected,
    int TaskContentTtlSeconds,
    string Status
);

public record UpdateRedisFeatureRequest(bool Enabled);
