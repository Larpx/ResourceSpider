using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using Larpx.PersonalTools.ResourceSpider.Server.Repositories;

namespace Larpx.PersonalTools.ResourceSpider.Server.Services;

/// <summary>
/// Agent 注册服务接口，提供 Agent 节点的注册、心跳、注销及令牌验证功能
/// 支持 Token 定期轮换，可配置轮换周期
/// </summary>
public interface IAgentRegisterService
{
    /// <summary>
    /// 注册 Agent 节点，若已存在则更新信息并重新生成令牌
    /// </summary>
    /// <param name="request">注册请求，包含 Agent 标识、名称、地址和端口等信息</param>
    /// <returns>注册响应，包含 Agent 令牌、心跳间隔和服务器版本</returns>
    Task<RegisterAgentResponse> RegisterAsync(RegisterAgentRequest request);

    /// <summary>
    /// 处理 Agent 心跳请求，更新 Agent 状态和资源使用情况
    /// 当 Token 接近过期时自动轮换并返回新 Token
    /// </summary>
    /// <param name="request">心跳请求，包含 Agent 标识、令牌和运行状态</param>
    /// <returns>心跳响应，包含确认标识及可能的任务/配置更新和新 Token</returns>
    Task<HeartbeatResponse> HeartbeatAsync(HeartbeatRequest request);

    /// <summary>
    /// 注销 Agent 节点，将其状态标记为离线
    /// </summary>
    /// <param name="request">注销请求，包含 Agent 标识、令牌和注销原因</param>
    Task UnregisterAsync(UnregisterAgentRequest request);

    /// <summary>
    /// 验证 Agent 令牌是否有效
    /// </summary>
    /// <param name="agentId">Agent 唯一标识</param>
    /// <param name="token">待验证的令牌</param>
    /// <returns>令牌有效返回 true，否则返回 false</returns>
    Task<bool> ValidateTokenAsync(string agentId, string token);

    /// <summary>
    /// 手动轮换指定 Agent 的 Token
    /// </summary>
    /// <param name="agentId">Agent 唯一标识</param>
    /// <returns>新 Token 明文，Agent 需在后续通信中使用新 Token</returns>
    Task<string?> RotateTokenAsync(string agentId);
}

/// <summary>
/// Agent 注册服务实现，管理 Agent 节点的注册、心跳、注销及安全令牌验证
/// 支持 Token 定期轮换，轮换周期可通过配置 TokenRotationHours 设定
/// </summary>
public class AgentRegisterService : IAgentRegisterService
{
    /// <summary>
    /// Agent 数据仓库，用于 Agent 实体的持久化操作
    /// </summary>
    private readonly IAgentRepository _agentRepository;

    /// <summary>
    /// 日志记录器，用于记录 Agent 注册相关事件
    /// </summary>
    private readonly ILogger<AgentRegisterService> _logger;

    /// <summary>
    /// JWT 密钥，用于生成 Agent 令牌
    /// </summary>
    private readonly string _jwtSecret;

    /// <summary>
    /// Token 轮换周期（小时），默认 24 小时
    /// </summary>
    private readonly int _tokenRotationHours;

    /// <summary>
    /// OTA 最新版本号，为 null 表示无可用更新
    /// </summary>
    private readonly string? _otaLatestVersion;

    /// <summary>
    /// OTA 下载地址
    /// </summary>
    private readonly string? _otaDownloadUrl;

    /// <summary>
    /// OTA 安装包校验和
    /// </summary>
    private readonly string? _otaChecksum;

    /// <summary>
    /// OTA 更新说明
    /// </summary>
    private readonly string? _otaReleaseNotes;

    /// <summary>
    /// OTA 是否强制更新
    /// </summary>
    private readonly bool _otaForceUpdate;

    /// <summary>
    /// 初始化 Agent 注册服务实例
    /// </summary>
    /// <param name="agentRepository">Agent 数据仓库</param>
    /// <param name="configuration">应用配置，用于读取 JWT 密钥和轮换周期</param>
    /// <param name="logger">日志记录器</param>
    public AgentRegisterService(
        IAgentRepository agentRepository,
        IConfiguration configuration,
        ILogger<AgentRegisterService> logger)
    {
        _agentRepository = agentRepository;
        _logger = logger;
        _jwtSecret = configuration["Jwt:Secret"] ?? "default-secret-key-change-in-production";
        _tokenRotationHours = configuration.GetValue("Agent:TokenRotationHours", 24);
        _otaLatestVersion = configuration["Agent:Ota:LatestVersion"];
        _otaDownloadUrl = configuration["Agent:Ota:DownloadUrl"];
        _otaChecksum = configuration["Agent:Ota:Checksum"];
        _otaReleaseNotes = configuration["Agent:Ota:ReleaseNotes"];
        _otaForceUpdate = configuration.GetValue("Agent:Ota:ForceUpdate", false);
    }

    /// <inheritdoc />
    public async Task<RegisterAgentResponse> RegisterAsync(RegisterAgentRequest request)
    {
        var existing = await _agentRepository.GetByIdAsync(request.AgentId);

        var token = GenerateToken(request.AgentId);
        var hashedToken = BCrypt.Net.BCrypt.HashPassword(token);

        if (existing != null)
        {
            existing.AgentToken = hashedToken;
            existing.IpAddress = request.IpAddress;
            existing.Port = request.Port;
            existing.Capabilities = request.Capabilities != null
                ? System.Text.Json.JsonSerializer.Serialize(request.Capabilities)
                : null;
            existing.OS = request.OS;
            existing.Version = request.Version;
            existing.Status = 1;
            existing.LastHeartbeat = DateTime.UtcNow;
            await _agentRepository.UpdateAsync(existing);

            _logger.LogInformation("Agent {AgentId} re-registered", request.AgentId);
        }
        else
        {
            var entity = new AgentEntity
            {
                AgentId = request.AgentId,
                AgentName = request.AgentName,
                AgentToken = hashedToken,
                IpAddress = request.IpAddress,
                Port = request.Port,
                Capabilities = request.Capabilities != null
                    ? System.Text.Json.JsonSerializer.Serialize(request.Capabilities)
                    : null,
                OS = request.OS,
                Version = request.Version,
                Status = 1,
                LastHeartbeat = DateTime.UtcNow
            };
            await _agentRepository.AddAsync(entity);

            _logger.LogInformation("Agent {AgentId} registered", request.AgentId);
        }

        return new RegisterAgentResponse(
            AgentToken: token,
            HeartbeatInterval: 30,
            ServerVersion: "1.0.0"
        );
    }

    /// <inheritdoc />
    public async Task<HeartbeatResponse> HeartbeatAsync(HeartbeatRequest request)
    {
        var agent = await _agentRepository.GetByIdAsync(request.AgentId);
        if (agent == null)
        {
            return new HeartbeatResponse(Ack: false, NewTasks: null, ConfigUpdate: null, NewToken: null, OtaUpdate: null);
        }

        if (!BCrypt.Net.BCrypt.Verify(request.AgentToken, agent.AgentToken))
        {
            return new HeartbeatResponse(Ack: false, NewTasks: null, ConfigUpdate: null, NewToken: null, OtaUpdate: null);
        }

        agent.CpuUsage = request.CpuUsage;
        agent.MemoryUsage = request.MemoryUsage;
        agent.TaskCount = request.TaskCount;
        agent.Status = request.Status;
        agent.OS = request.OS ?? agent.OS;
        agent.Version = request.Version ?? agent.Version;
        agent.LastHeartbeat = DateTime.UtcNow;

        string? newToken = null;
        if (ShouldRotateToken(agent))
        {
            newToken = GenerateToken(request.AgentId);
            agent.AgentToken = BCrypt.Net.BCrypt.HashPassword(newToken);
            _logger.LogInformation("Agent {AgentId} Token 已自动轮换", request.AgentId);
        }

        await _agentRepository.UpdateAsync(agent);

        var otaUpdate = BuildOtaUpdateInfo(request.Version);
        if (otaUpdate != null)
        {
            _logger.LogInformation("Agent {AgentId} 有可用 OTA 更新: {LatestVersion}", request.AgentId, _otaLatestVersion);
        }

        return new HeartbeatResponse(
            Ack: true,
            NewTasks: null,
            ConfigUpdate: null,
            NewToken: newToken,
            OtaUpdate: otaUpdate
        );
    }

    /// <inheritdoc />
    public async Task UnregisterAsync(UnregisterAgentRequest request)
    {
        var agent = await _agentRepository.GetByIdAsync(request.AgentId);
        if (agent != null && BCrypt.Net.BCrypt.Verify(request.AgentToken, agent.AgentToken))
        {
            agent.Status = 0;
            await _agentRepository.UpdateAsync(agent);
            _logger.LogInformation("Agent {AgentId} unregistered. Reason: {Reason}",
                request.AgentId, request.Reason);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateTokenAsync(string agentId, string token)
    {
        var agent = await _agentRepository.GetByIdAsync(agentId);
        if (agent == null) return false;
        return BCrypt.Net.BCrypt.Verify(token, agent.AgentToken);
    }

    /// <summary>
    /// 根据 Agent 标识生成安全令牌，使用 SHA256 哈希算法
    /// </summary>
    /// <param name="agentId">Agent 唯一标识</param>
    /// <returns>32 字符的十六进制令牌字符串</returns>
    private string GenerateToken(string agentId)
    {
        var payload = $"{agentId}:{DateTime.UtcNow:yyyyMMddHHmmss}:{Guid.NewGuid():N}";
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(payload + _jwtSecret));
        return Convert.ToHexString(hash).ToLowerInvariant().Substring(0, 32);
    }

    /// <summary>
    /// 判断 Agent Token 是否需要轮换
    /// 当距离上次注册/轮换时间超过配置的轮换周期时触发
    /// </summary>
    /// <param name="agent">Agent 实体</param>
    /// <returns>需要轮换返回 true，否则返回 false</returns>
    private bool ShouldRotateToken(AgentEntity agent)
    {
        if (_tokenRotationHours <= 0) return false;
        if (agent.LastHeartbeat == null) return true;
        return (DateTime.UtcNow - agent.LastHeartbeat.Value).TotalHours >= _tokenRotationHours;
    }

    /// <inheritdoc />
    public async Task<string?> RotateTokenAsync(string agentId)
    {
        var agent = await _agentRepository.GetByIdAsync(agentId);
        if (agent == null) return null;

        var newToken = GenerateToken(agentId);
        agent.AgentToken = BCrypt.Net.BCrypt.HashPassword(newToken);
        await _agentRepository.UpdateAsync(agent);

        _logger.LogInformation("Agent {AgentId} Token 已手动轮换", agentId);
        return newToken;
    }

    /// <summary>
    /// 根据配置和 Agent 当前版本构建 OTA 更新信息
    /// 仅当配置了新版本且 Agent 版本低于最新版本时返回更新信息
    /// </summary>
    /// <param name="agentVersion">Agent 当前版本号</param>
    /// <returns>有可用更新时返回 OtaUpdateInfo，否则返回 null</returns>
    private OtaUpdateInfo? BuildOtaUpdateInfo(string? agentVersion)
    {
        if (string.IsNullOrWhiteSpace(_otaLatestVersion) || string.IsNullOrWhiteSpace(_otaDownloadUrl))
            return null;

        if (string.IsNullOrWhiteSpace(agentVersion))
            return new OtaUpdateInfo(_otaLatestVersion, _otaDownloadUrl, _otaChecksum, _otaReleaseNotes, _otaForceUpdate);

        if (Version.TryParse(_otaLatestVersion, out var latest) && Version.TryParse(agentVersion, out var current))
        {
            if (latest > current)
            {
                return new OtaUpdateInfo(_otaLatestVersion, _otaDownloadUrl, _otaChecksum, _otaReleaseNotes, _otaForceUpdate);
            }
        }
        else if (!string.Equals(agentVersion, _otaLatestVersion, StringComparison.OrdinalIgnoreCase))
        {
            return new OtaUpdateInfo(_otaLatestVersion, _otaDownloadUrl, _otaChecksum, _otaReleaseNotes, _otaForceUpdate);
        }

        return null;
    }
}
