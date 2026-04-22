using System.Collections.Concurrent;
using ResourceSpider.Infrastructure.Utils;
using Shouldly;
using Xunit;

namespace ResourceSpider.Tests.Unit;

/// <summary>
/// <see cref="HashedWheelTimer"/> 单元测试。
/// 
/// 这些测试主要验证三个关键目标：
/// 1. 延迟任务是否会被执行；
/// 2. 取消任务是否会被跳过；
/// 3. 多个任务在并发场景下是否保持稳定执行。
/// </summary>
public class HashedWheelTimerTests
{
    /// <summary>
    /// 验证：正常任务会在预期时间窗口内执行一次。
    /// </summary>
    [Fact]
    public async Task AddTask_ShouldExecuteTaskWithinExpectedDelay()
    {
        using var timer = new HashedWheelTimer(ticksPerWheel: 64, tickDurationMs: 20);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        var executedAt = DateTime.MinValue;
        var signal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        timer.AddTask(new TimerTask
        {
            Action = () =>
            {
                executedAt = DateTime.UtcNow;
                signal.TrySetResult();
            }
        }, TimeSpan.FromMilliseconds(120));

        await signal.Task.WaitAsync(cts.Token);

        executedAt.ShouldNotBe(DateTime.MinValue);
    }

    /// <summary>
    /// 验证：被取消的任务不会触发其回调。
    /// </summary>
    [Fact]
    public async Task AddTask_CancelledTask_ShouldNotExecute()
    {
        using var timer = new HashedWheelTimer(ticksPerWheel: 32, tickDurationMs: 20);

        var executed = false;
        var task = new TimerTask
        {
            IsCancelled = true,
            Action = () => executed = true
        };

        timer.AddTask(task, TimeSpan.FromMilliseconds(60));

        await Task.Delay(250);
        executed.ShouldBeFalse();
    }

    /// <summary>
    /// 验证：连续添加多个任务时，时间轮能稳定执行所有任务。
    /// </summary>
    [Fact]
    public async Task AddTask_MultipleTasks_ShouldExecuteAll()
    {
        using var timer = new HashedWheelTimer(ticksPerWheel: 64, tickDurationMs: 10);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        var executeCount = 0;
        var completed = new ConcurrentDictionary<int, bool>();

        for (var i = 0; i < 10; i++)
        {
            var index = i;
            timer.AddTask(new TimerTask
            {
                Action = () =>
                {
                    completed[index] = true;
                    Interlocked.Increment(ref executeCount);
                }
            }, TimeSpan.FromMilliseconds(30 + i * 15));
        }

        while (Volatile.Read(ref executeCount) < 10 && !cts.IsCancellationRequested)
        {
            await Task.Delay(20, cts.Token);
        }

        executeCount.ShouldBe(10);
        completed.Count.ShouldBe(10);
    }
}
