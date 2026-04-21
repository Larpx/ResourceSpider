using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface IAgentRegisterService
{
    Task<RegisterAgentResponse> RegisterAsync(RegisterAgentRequest request);
    Task<HeartbeatResponse> HeartbeatAsync(HeartbeatRequest request);
    Task UnregisterAsync(UnregisterAgentRequest request);
    Task<bool> ValidateTokenAsync(string agentId, string token);
}

public class AgentRegisterService : IAgentRegisterService
{
    private readonly IAgentRepository _agentRepository;
    private readonly ILogger<AgentRegisterService> _logger;
    private readonly string _jwtSecret;

    public AgentRegisterService(
        IAgentRepository agentRepository,
        IConfiguration configuration,
        ILogger<AgentRegisterService> logger)
    {
        _agentRepository = agentRepository;
        _logger = logger;
        _jwtSecret = configuration["Jwt:Secret"] ?? "default-secret-key-change-in-production";
    }

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
        agent.LastHeartbeat = DateTime.UtcNow;
        await _agentRepository.UpdateAsync(agent);

        return new HeartbeatResponse(
            Ack: true,
            NewTasks: null,
            ConfigUpdate: null
        );
    }

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

    public async Task<bool> ValidateTokenAsync(string agentId, string token)
    {
        var agent = await _agentRepository.GetByIdAsync(agentId);
        if (agent == null) return false;
        return BCrypt.Net.BCrypt.Verify(token, agent.AgentToken);
    }

    private string GenerateToken(string agentId)
    {
        var payload = $"{agentId}:{DateTime.UtcNow:yyyyMMddHHmmss}:{Guid.NewGuid():N}";
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(payload + _jwtSecret));
        return Convert.ToHexString(hash).ToLowerInvariant().Substring(0, 32);
    }
}
