namespace ResourceSpider.Infrastructure.Utils;

public class HashedWheelTimer
{
    private readonly int _ticksPerWheel;
    private readonly TimeSpan _tickDuration;
    private readonly List<TimerTask>[] _wheel;
    private int _currentTick;
    private readonly Timer _timer;
    private bool _disposed;

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
                    // Log error
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

    private static int FindNextPositivePowerOfTwo(int value)
    {
        int result = 1;
        while (result < value)
        {
            result <<= 1;
        }
        return result;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _timer.Dispose();
        _disposed = true;
    }
}

public class TimerTask
{
    public Action? Action { get; set; }
    public DateTime Deadline { get; set; }
    public int RemainingTicks { get; set; }
    public bool IsCancelled { get; set; }

    public void Run()
    {
        Action?.Invoke();
    }
}
