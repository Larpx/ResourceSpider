using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

/// <summary>
/// Agent 注册服务接口，提供 Agent 节点的注册、心跳、注销及令牌验证功能
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
    /// </summary>
    /// <param name="request">心跳请求，包含 Agent 标识、令牌和运行状态</param>
    /// <returns>心跳响应，包含确认标识及可能的任务/配置更新</returns>
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
}

/// <summary>
/// Agent 注册服务实现，管理 Agent 节点的注册、心跳、注销及安全令牌验证
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
    /// 初始化 Agent 注册服务实例
    /// </summary>
    /// <param name="agentRepository">Agent 数据仓库</param>
    /// <param name="configuration">应用配置，用于读取 JWT 密钥</param>
    /// <param name="logger">日志记录器</param>
    public AgentRegisterService(
        IAgentRepository agentRepository,
        IConfiguration configuration,
        ILogger<AgentRegisterService> logger)
    {
        _agentRepository = agentRepository;
        _logger = logger;
        _jwtSecret = configuration["Jwt:Secret"] ?? "default-secret-key-change-in-production";
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
            return new HeartbeatResponse(Ack: false, NewTasks: null, ConfigUpdate: null);
        }

        if (!BCrypt.Net.BCrypt.Verify(request.AgentToken, agent.AgentToken))
        {
            return new HeartbeatResponse(Ack: false, NewTasks: null, ConfigUpdate: null);
        }

        agent.CpuUsage = request.CpuUsage;
        agent.MemoryUsage = request.MemoryUsage;
        agent.TaskCount = request.TaskCount;
        agent.Status = request.Status;
        agent.OS = request.OS ?? agent.OS;
        agent.Version = request.Version ?? agent.Version;
        agent.LastHeartbeat = DateTime.UtcNow;
        await _agentRepository.UpdateAsync(agent);

        return new HeartbeatResponse(
            Ack: true,
            NewTasks: null,
            ConfigUpdate: null
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
}
