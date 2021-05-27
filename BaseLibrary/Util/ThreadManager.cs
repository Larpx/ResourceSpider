using Serilog;
using System;
using System.Threading;

namespace Larpx.ResourceSpider.BaseLibrary.Util
{
    /// <summary>
    /// 处理多线程实现细节
    /// </summary>
    public interface IThreadManager : IDisposable
    {
        /// <summary>
        /// 使用的最大线程数。
        /// </summary>
        int MaxThreads { get; set; }

        /// <summary>
        /// 将在单独的线程上并行执行操作
        /// </summary>
        void DoWork(Action action);

        /// <summary>
        /// 是否有运行线程
        /// </summary>
        bool HasRunningThreads();

        /// <summary>
        /// 中止所有正在运行的线程
        /// </summary>
        void AbortAll();
    }

    public abstract class ThreadManager : IThreadManager
    {
        protected bool _abortAllCalled = false;
        protected int _numberOfRunningThreads = 0;
        protected ManualResetEvent _resetEvent = new ManualResetEvent(true);
        protected object _locker = new object();
        protected bool _isDisplosed = false;

        protected ThreadManager(int maxThreads)
        {
            if ((maxThreads > 100) || (maxThreads < 1))
                throw new ArgumentException("MaxThreads must be from 1 to 100");

            MaxThreads = maxThreads;
        }

        /// <summary>
        /// 使用的最大线程数。
        /// </summary>
        public int MaxThreads
        {
            get;
            set;
        }

        /// <summary>
        /// 将在单独的线程上并行执行操作
        /// </summary>
        public virtual void DoWork(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (_abortAllCalled)
                throw new InvalidOperationException("在调用AbortAll()或Dispose()后不能调用DoWork()。");

            if (!_isDisplosed && MaxThreads > 1)
            {
                _resetEvent.WaitOne();
                lock (_locker)
                {
                    _numberOfRunningThreads++;
                    if (!_isDisplosed && _numberOfRunningThreads >= MaxThreads)
                        _resetEvent.Reset();

                    Log.Debug("启动另一个线程，将正在运行的线程增加到 [{0}].", _numberOfRunningThreads);
                }
                RunActionOnDedicatedThread(action);
            }
            else
            {
                RunAction(action, false);
            }
        }

        public virtual void AbortAll()
        {
            _abortAllCalled = true;
            lock (_locker)
            {
                _numberOfRunningThreads = 0;
            }
        }

        public virtual void Dispose()
        {
            AbortAll();
            _resetEvent.Dispose();
            _isDisplosed = true;
        }

        public virtual bool HasRunningThreads()
        {
            lock (_locker)
            {
                return _numberOfRunningThreads > 0;
            }
        }

        protected virtual void RunAction(Action action, bool decrementRunningThreadCountOnCompletion = true)
        {
            try
            {
                action.Invoke();
                Log.Debug("操作已成功完成。");
            }
            catch (OperationCanceledException)
            {
                Log.Debug("线程已取消");
                throw;
            }
            catch (Exception e)
            {
                Log.Error("运行操作时发生错误：{0}", e);
            }
            finally
            {
                if (decrementRunningThreadCountOnCompletion)
                {
                    lock (_locker)
                    {
                        _numberOfRunningThreads--;
                        Log.Debug("[{0}] 线程正在运行.", _numberOfRunningThreads);
                        if (!_isDisplosed && _numberOfRunningThreads < MaxThreads)
                            _resetEvent.Set();
                    }
                }
            }
        }

        /// <summary>
        /// Runs the action on a separate thread
        /// </summary>
        protected abstract void RunActionOnDedicatedThread(Action action);
    }
}
