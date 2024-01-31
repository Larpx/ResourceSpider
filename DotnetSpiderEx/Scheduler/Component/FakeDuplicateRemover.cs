using Larpx.ResourceSpider.DotnetSpiderEx.Http;
using Larpx.ResourceSpider.DotnetSpiderEx.Infrastructure;

namespace Larpx.ResourceSpider.DotnetSpiderEx.Scheduler.Component
{
    internal class FakeDuplicateRemover : IDuplicateRemover
    {
        private long _counter;

        public FakeDuplicateRemover()
        {
            _counter = 0;
        }

        public void Dispose()
        {
        }

        public Task<bool> IsDuplicateAsync(Request request)
        {
            request.NotNull(nameof(request));
            request.Owner.NotNullOrWhiteSpace(nameof(request.Owner));
            Interlocked.Increment(ref _counter);
            return Task.FromResult(false);
        }

        public Task<long> GetTotalAsync()
        {
            return Task.FromResult(_counter);
        }

        public Task ResetDuplicateCheckAsync()
        {
            return Task.CompletedTask;
        }

        public Task InitializeAsync(string spiderId)
        {
            return Task.CompletedTask;
        }
    }
}
