using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

/// <summary>
/// 统计服务接口，提供 Agent、任务和系统级别的统计数据查询功能
/// </summary>
public interface IStatisticsService
{
    /// <summary>
    /// 获取所有 Agent 的统计信息列表
    /// </summary>
    /// <returns>Agent 统计 DTO 列表</returns>
    Task<List<AgentStatisticsDto>> GetAgentStatisticsAsync();

    /// <summary>
    /// 获取指定任务的统计信息
    /// </summary>
    /// <param name="taskId">任务唯一标识</param>
    /// <returns>任务统计 DTO，若任务不存在返回 null</returns>
    Task<TaskStatisticsDto?> GetTaskStatisticsAsync(string taskId);

    /// <summary>
    /// 获取系统级别的汇总统计信息
    /// </summary>
    /// <returns>系统统计 DTO</returns>
    Task<SystemStatisticsDto> GetSystemStatisticsAsync();

    /// <summary>
    /// 获取指定时间范围内的趋势数据
    /// </summary>
    /// <param name="startDate">起始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>趋势数据点列表</returns>
    Task<List<TrendDataPoint>> GetTrendDataAsync(DateTime startDate, DateTime endDate);
}

/// <summary>
/// 统计服务实现，聚合 Agent、任务和系统级别的统计数据
/// </summary>
public class StatisticsService : IStatisticsService
{
    /// <summary>
    /// 统计数据仓库，用于系统趋势数据的查询
    /// </summary>
    private readonly IStatisticRepository _statisticRepository;

    /// <summary>
    /// 任务数据仓库，用于任务统计数据的查询
    /// </summary>
    private readonly ITaskRepository _taskRepository;

    /// <summary>
    /// Agent 数据仓库，用于 Agent 统计数据的查询
    /// </summary>
    private readonly IAgentRepository _agentRepository;

    /// <summary>
    /// 初始化统计服务实例
    /// </summary>
    /// <param name="statisticRepository">统计数据仓库</param>
    /// <param name="taskRepository">任务数据仓库</param>
    /// <param name="agentRepository">Agent 数据仓库</param>
    public StatisticsService(
        IStatisticRepository statisticRepository,
        ITaskRepository taskRepository,
        IAgentRepository agentRepository)
    {
        _statisticRepository = statisticRepository;
        _taskRepository = taskRepository;
        _agentRepository = agentRepository;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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
