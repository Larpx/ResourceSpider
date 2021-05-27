using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Larpx.ResourceSpider.BaseLibrary.Util
{
    /// <summary>
    ///ThreadManager实现，它将使用tpl任务来处理并发性。
    /// </summary>
    public class TaskThreadManager : ThreadManager
    {
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public TaskThreadManager(int maxConcurrentTasks)
            : this(maxConcurrentTasks, null)
        {
        }

        public TaskThreadManager(int maxConcurrentTasks, CancellationTokenSource cancellationTokenSource)
            : base(maxConcurrentTasks)
        {
            _cancellationTokenSource = cancellationTokenSource ?? _cancellationTokenSource;
        }

        public override void AbortAll()
        {
            _cancellationTokenSource.Cancel();
            base.AbortAll();
        }

        public override void Dispose()
        {
            base.Dispose();
            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();
        }

        protected override void RunActionOnDedicatedThread(Action action)
        {
            Task.Factory
                .StartNew(() => RunAction(action), _cancellationTokenSource.Token)
                .ContinueWith(HandleAggregateExceptions, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// 添加了此项以解决此处描述的问题
        /// http://stackoverflow.com/questions/7883052/a-tasks-exceptions-were-not-observed-either-by-waiting-on-the-task-or-accessi
        /// </summary>
        private void HandleAggregateExceptions(Task task)
        {
            if (task?.Exception == null)
                return;

            var aggException = task.Exception.Flatten();
            foreach (var exception in aggException.InnerExceptions)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                    //如果任务被取消，那么这个异常就会发生，我们不在乎
                    Log.Warning("CancellationRequested总异常: {0}", exception);
                else
                    //如果任务未被取消，那么这是一个错误
                    Log.Error("总异常: {0}", exception);
            }
        }
    }
}
