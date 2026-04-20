using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface IStatisticsService
{
    Task<List<AgentStatisticsDto>> GetAgentStatisticsAsync();
    Task<TaskStatisticsDto?> GetTaskStatisticsAsync(string taskId);
    Task<SystemStatisticsDto> GetSystemStatisticsAsync();
    Task<List<TrendDataPoint>> GetTrendDataAsync(DateTime startDate, DateTime endDate);
}

public class StatisticsService : IStatisticsService
{
    private readonly IStatisticRepository _statisticRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IAgentRepository _agentRepository;

    public StatisticsService(
        IStatisticRepository statisticRepository,
        ITaskRepository taskRepository,
        IAgentRepository agentRepository)
    {
        _statisticRepository = statisticRepository;
        _taskRepository = taskRepository;
        _agentRepository = agentRepository;
    }

    public async Task<List<AgentStatisticsDto>> GetAgentStatisticsAsync()
    {
        var agents = await _agentRepository.GetAllAsync();
        var result = new List<AgentStatisticsDto>();

        foreach (var agent in agents)
        {
            var stats = new AgentStatisticsDto(
                AgentId: agent.AgentId,
                AgentName: agent.AgentName,
                Status: agent.Status,
                TotalTasks: 0,
                CompletedTasks: 0,
                FailedTasks: 0,
                AvgDuration: null,
                LastHeartbeat: agent.LastHeartbeat
            );
            result.Add(stats);
        }

        return result;
    }

    public async Task<TaskStatisticsDto?> GetTaskStatisticsAsync(string taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null) return null;

        return new TaskStatisticsDto(
            TaskId: task.TaskId,
            TaskName: task.TaskName,
            TotalRequests: task.TotalRequests,
            SuccessRequests: task.CompletedRequests,
            FailedRequests: task.FailedRequests,
            Progress: task.Progress,
            StartTime: task.StartTime,
            EndTime: task.EndTime
        );
    }

    public async Task<SystemStatisticsDto> GetSystemStatisticsAsync()
    {
        var onlineAgents = await _agentRepository.CountOnlineAsync();
        var totalAgents = await _agentRepository.CountAsync();
        var totalTasks = await _taskRepository.CountAsync();

        return new SystemStatisticsDto(
            OnlineAgents: (int)onlineAgents,
            TotalAgents: (int)totalAgents,
            RunningTasks: 0,
            PendingTasks: 0,
            CompletedTasks: (int)totalTasks,
            TotalDataVolume: 0,
            AvgSuccessRate: 0
        );
    }

    public async Task<List<TrendDataPoint>> GetTrendDataAsync(DateTime startDate, DateTime endDate)
    {
        var stats = await _statisticRepository.GetSystemTrendAsync(startDate, endDate);
        
        return stats.Select(s => new TrendDataPoint(
            Date: s.StatDate,
            TotalRequests: s.TotalRequests,
            SuccessRequests: s.SuccessRequests,
            FailedRequests: s.FailedRequests,
            DataVolume: s.DataVolume
        )).ToList();
    }
}
