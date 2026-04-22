namespace ResourceSpider.Infrastructure.Utils;

/// <summary>
/// 哈希时间轮定时器，使用时间轮算法高效管理大量延迟任务
/// 适用于爬虫中的超时检测、重试调度等场景
/// </summary>
public class HashedWheelTimer
{
    private readonly int _ticksPerWheel;
    private readonly TimeSpan _tickDuration;
    private readonly List<TimerTask>[] _wheel;
    private int _currentTick;
    private readonly Timer _timer;
    private bool _disposed;

    /// <summary>
    /// 初始化哈希时间轮定时器
    /// </summary>
    /// <param name="ticksPerWheel">时间轮刻度数，会自动调整为 2 的幂次方</param>
    /// <param name="tickDurationMs">每个刻度的持续时间（毫秒）</param>
    public HashedWheelTimer(int ticksPerWheel = 512, int tickDurationMs = 100)
    {
        _ticksPerWheel = FindNextPositivePowerOfTwo(ticksPerWheel);
        _tickDuration = TimeSpan.FromMilliseconds(tickDurationMs);
        _wheel = new List<TimerTask>[_ticksPerWheel];
        
        for (int i = 0; i < _ticksPerWheel; i++)
        {
            _wheel[i] = new List<TimerTask>();
        }

        _timer = new Timer(Tick, null, _tickDuration, _tickDuration);
    }

    /// <summary>
    /// 添加延迟任务到时间轮中
    /// </summary>
    /// <param name="task">定时任务</param>
    /// <param name="delay">延迟时间</param>
    public void AddTask(TimerTask task, TimeSpan delay)
    {
        var ticks = (int)(delay.TotalMilliseconds / _tickDuration.TotalMilliseconds);
        var targetTick = (_currentTick + ticks) % _ticksPerWheel;
        
        task.Deadline = DateTime.UtcNow.Add(delay);
        task.RemainingTicks = ticks;
        
        lock (_wheel[targetTick])
        {
            _wheel[targetTick].Add(task);
        }
    }

    /// <summary>
    /// 时间轮刻度推进，执行到期任务
    /// </summary>
    /// <param name="state">定时器状态对象</param>
    private void Tick(object? state)
    {
        var tasksToExecute = new List<TimerTask>();
        
        lock (_wheel[_currentTick])
        {
            tasksToExecute.AddRange(_wheel[_currentTick]);
            _wheel[_currentTick].Clear();
        }

        foreach (var task in tasksToExecute)
        {
            if (task.RemainingTicks <= 0 && !task.IsCancelled)
            {
                try
                {
                    task.Run();
                }
                catch (Exception)
                {
                }
            }
            else if (!task.IsCancelled)
            {
                task.RemainingTicks--;
                var targetTick = _currentTick;
                lock (_wheel[targetTick])
                {
                    _wheel[targetTick].Add(task);
                }
            }
        }

        _currentTick = (_currentTick + 1) % _ticksPerWheel;
    }

    /// <summary>
    /// 找到大于等于指定值的最小 2 的幂次方
    /// </summary>
    /// <param name="value">目标值</param>
    /// <returns>2 的幂次方值</returns>
    private static int FindNextPositivePowerOfTwo(int value)
    {
        int result = 1;
        while (result < value)
        {
            result <<= 1;
        }
        return result;
    }

    /// <summary>
    /// 释放定时器资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _timer.Dispose();
        _disposed = true;
    }
}

/// <summary>
/// 定时任务，封装延迟执行的操作
/// </summary>
public class TimerTask
{
    /// <summary>
    /// 任务执行的操作
    /// </summary>
    public Action? Action { get; set; }

    /// <summary>
    /// 任务截止时间
    /// </summary>
    public DateTime Deadline { get; set; }

    /// <summary>
    /// 剩余刻度数
    /// </summary>
    public int RemainingTicks { get; set; }

    /// <summary>
    /// 是否已取消
    /// </summary>
    public bool IsCancelled { get; set; }

    /// <summary>
    /// 执行任务
    /// </summary>
    public void Run()
    {
        Action?.Invoke();
    }
}
