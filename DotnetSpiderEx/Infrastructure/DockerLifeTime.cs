using Microsoft.Extensions.Hosting;

namespace Larpx.ResourceSpider.DotnetSpiderEx.Infrastructure
{
    public class DockerLifeTime : IHostLifetime
    {
        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Environment.Exit(0);
            return Task.CompletedTask;
        }
    }
}
