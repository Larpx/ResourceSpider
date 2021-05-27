using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Larpx.ResourceSpider.BaseLibrary.Util
{
    public interface IRateLimiter
    {
        void WaitToProceed();
    }

    //http://www.pennedobjects.com/2010/10/better-rate-limiting-with-dot-net/
    /// <summary>
    /// 用于控制单位时间内某些事件的发生率。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///    要使用<see cref="RateLimiter"/>控制操作的速率，
    ///    代码只需在执行操作之前调用<see cref="WaitToProceed"/>将阻止当前线程，直到基于速率限制允许该操作。
    ///     </para>
    ///     <para>
    ///     这个类是线程安全的。一个<see cref="RateLimiter"/>实例可用于控制多个线程之间的发生率。
    ///     </para>
    /// </remarks>
    public class RateLimiter : IRateLimiter, IDisposable
    {
        /// <summary>
        /// 用于计数和限制每单位时间内出现次数的信号量。
        /// </summary>
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// 应该退出信号量的次数（毫秒）。
        /// </summary>
        private readonly ConcurrentQueue<int> _exitTimes;

        /// <summary>
        /// 用于触发退出信号量的计时器。
        /// </summary>
        private readonly Timer _exitTimer;

        /// <summary>
        /// 是否释放此实例。
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        ///每单位时间允许出现的次数。
        /// </summary>
        public int Occurrences { get; private set; }

        /// <summary>
        /// 时间单位的长度，以毫秒为单位。
        /// </summary>
        public int TimeUnitMilliseconds { get; private set; }

        /// <summary>
        /// 按<paramref name="timeUnit"/>的<paramref name="occurrents"/>速率初始化<see cref="RateLimiter"/>。
        /// </summary>
        /// <param name="occurrences">每单位时间允许出现的次数。</param>
        /// <param name="timeUnit">时间单位的长度。</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="occurrences"/> or <paramref name="timeUnit"/> is negative.
        /// </exception>
        public RateLimiter(int occurrences, TimeSpan timeUnit)
        {
            //检查参数
            if (occurrences <= 0)
                throw new ArgumentOutOfRangeException("occurrences", "出现次数必须为正整数");
            if (timeUnit != timeUnit.Duration())
                throw new ArgumentOutOfRangeException("timeUnit", "时间单位必须是正的时间跨度");
            if (timeUnit >= TimeSpan.FromMilliseconds(UInt32.MaxValue))
                throw new ArgumentOutOfRangeException("timeUnit", "时间单位必须小于2^32毫秒");

            Occurrences = occurrences;
            TimeUnitMilliseconds = (int)timeUnit.TotalMilliseconds;

            // 创建信号量，将出现次数作为最大计数。
            _semaphore = new SemaphoreSlim(Occurrences, Occurrences);

            // 创建一个队列以保持信号量退出时间。
            _exitTimes = new ConcurrentQueue<int>();

            //创建一个计时器来退出信号量。使用时间单位作为初始间隔长度，因为这是我们需要退出信号量的最早时间。
            _exitTimer = new Timer(ExitTimerCallback, null, TimeUnitMilliseconds, -1);
        }

        /// <summary>
        /// 阻止当前线程直到允许继续或直到指定超时过去。
        /// </summary>
        /// <param name="millisecondsTimeout">等待的毫秒数，或者无限期等待。</param>
        /// <returns>如果允许线程继续，则为true；如果超时，则为false</returns>
        public bool WaitToProceed(int millisecondsTimeout)
        {
            // Check the arguments.
            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException("millisecondsTimeout");

            CheckDisposed();

            var entered = _semaphore.Wait(millisecondsTimeout);

            if (entered)
            {
                var timeToExit = unchecked(Environment.TickCount + TimeUnitMilliseconds);
                _exitTimes.Enqueue(timeToExit);
            }

            return entered;
        }

        /// <summary>
        /// 阻止当前线程直到允许继续或直到指定超时过去。
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>如果允许线程继续，则为true；如果超时，则为false</returns>
        public bool WaitToProceed(TimeSpan timeout)
        {
            return WaitToProceed((int)timeout.TotalMilliseconds);
        }

        /// <summary>
        /// 阻止当前线程直到允许继续或直到指定超时过去。
        /// </summary>
        public void WaitToProceed()
        {
            WaitToProceed(Timeout.Infinite);
        }

        /// <summary>
        /// 释放由此类实例持有的非托管资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放由此类实例持有的非托管资源。
        /// </summary>
        /// <param name="isDisposing">是否正在释放此对象。</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    _semaphore.Dispose();
                    _exitTimer.Dispose();

                    _isDisposed = true;
                }
            }
        }

        /// <summary>
        /// 退出定时器的回调，该定时器基于队列中的退出时间退出信号量，然后为nextexit时间设置定时器。
        /// </summary>
        /// <param name="state"></param>
        private void ExitTimerCallback(object state)
        {
            int exitTime;
            var exitTimeValid = _exitTimes.TryPeek(out exitTime);
            while (exitTimeValid)
            {
                if (unchecked(exitTime - Environment.TickCount) > 0)
                {
                    break;
                }
                _semaphore.Release();
                _exitTimes.TryDequeue(out exitTime);
                exitTimeValid = _exitTimes.TryPeek(out exitTime);
            }

            var timeUntilNextCheck = exitTimeValid ? Math.Min(TimeUnitMilliseconds, Math.Max(0, exitTime - Environment.TickCount)) : TimeUnitMilliseconds;

            _exitTimer.Change(timeUntilNextCheck, -1);
        }

        /// <summary>
        /// 如果该对象被释放，则抛出ObjectDisposedException。
        /// </summary>
        private void CheckDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("该对象被释放");
        }
    }
}
