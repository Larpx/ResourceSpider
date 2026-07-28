using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Larpx.PersonalTools.ResourceSpider.Core.Enums;
using Larpx.PersonalTools.ResourceSpider.Core.Models;

namespace Larpx.PersonalTools.ResourceSpider.Server.Services;

public class TaskSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaskSchedulerService> _logger;
    private readonly Dictionary<string, Timer> _scheduledTasks = new();

    public TaskSchedulerService(
        IServiceProvider serviceProvider,
        ILogger<TaskSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("任务调度服务启动");

        using var periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CheckAndScheduleTasksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查定时任务出错");
            }
        }
    }

    private async Task CheckAndScheduleTasksAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
        var taskDispatchService = scope.ServiceProvider.GetRequiredService<TaskDispatchService>();

        var scheduledTasks = await taskService.GetScheduledTasksAsync();

        foreach (var task in scheduledTasks)
        {
            if (task.ScheduleConfig == null || !task.ScheduleConfig.Enabled) continue;

            var shouldRun = ShouldRunNow(task.ScheduleConfig);
            if (!shouldRun) continue;

            _logger.LogInformation("定时触发任务: {TaskId} - {TaskName}", task.TaskId, task.TaskName);

            await taskDispatchService.DispatchTaskAsync(task.TaskId);
        }
    }

    private static bool ShouldRunNow(TaskScheduleConfig schedule)
    {
        var now = DateTime.UtcNow;

        if (schedule.StartAt.HasValue && now < schedule.StartAt.Value) return false;
        if (schedule.EndAt.HasValue && now > schedule.EndAt.Value) return false;

        return schedule.ScheduleType.ToLowerInvariant() switch
        {
            "cron" => EvaluateCron(schedule.CronExpression, now),
            "interval" => EvaluateInterval(schedule, now),
            "once" => EvaluateOnce(schedule, now),
            _ => false
        };
    }

    private static bool EvaluateCron(string? cronExpression, DateTime now)
    {
        if (string.IsNullOrEmpty(cronExpression)) return false;

        try
        {
            var parts = cronExpression.Split(' ');
            if (parts.Length < 5) return false;

            if (parts[0] != "*" && !MatchCronPart(parts[0], now.Minute)) return false;
            if (parts[1] != "*" && !MatchCronPart(parts[1], now.Hour)) return false;
            if (parts[2] != "*" && !MatchCronPart(parts[2], now.Day)) return false;
            if (parts[3] != "*" && !MatchCronPart(parts[3], now.Month)) return false;
            if (parts.Length > 4 && parts[4] != "*" && !MatchCronDayOfWeek(parts[4], (int)now.DayOfWeek)) return false;

            return now.Second == 0;
        }
        catch { return false; }
    }

    private static bool MatchCronPart(string part, int value)
    {
        if (part == "*") return true;

        foreach (var item in part.Split(','))
        {
            if (item.Contains('/'))
            {
                var stepParts = item.Split('/');
                if (stepParts.Length == 2 && int.TryParse(stepParts[1], out var step))
                {
                    var start = stepParts[0] == "*" ? 0 : int.Parse(stepParts[0]);
                    if (value >= start && (value - start) % step == 0) return true;
                }
            }
            else if (item.Contains('-'))
            {
                var range = item.Split('-');
                if (range.Length == 2 && int.TryParse(range[0], out var from) && int.TryParse(range[1], out var to))
                {
                    if (value >= from && value <= to) return true;
                }
            }
            else if (int.TryParse(item, out var exact) && value == exact)
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchCronDayOfWeek(string part, int dayOfWeek)
    {
        return MatchCronPart(part, dayOfWeek == 0 ? 7 : dayOfWeek);
    }

    private static bool EvaluateInterval(TaskScheduleConfig schedule, DateTime now)
    {
        if (!schedule.IntervalSeconds.HasValue || schedule.IntervalSeconds.Value <= 0) return false;

        var interval = schedule.IntervalSeconds.Value;
        var minutesAligned = now.Minute * 60 + now.Second;
        return minutesAligned % interval == 0;
    }

    private static bool EvaluateOnce(TaskScheduleConfig schedule, DateTime now)
    {
        if (!schedule.StartAt.HasValue) return false;
        var diff = (now - schedule.StartAt.Value).TotalMinutes;
        return diff >= 0 && diff < 1;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var timer in _scheduledTasks.Values)
        {
            await timer.DisposeAsync();
        }
        _scheduledTasks.Clear();

        await base.StopAsync(cancellationToken);
    }
}
