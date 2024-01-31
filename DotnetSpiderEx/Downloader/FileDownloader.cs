using Larpx.ResourceSpider.DotnetSpiderEx.Http;
using System.Diagnostics;
using System.Net;
using ByteArrayContent = Larpx.ResourceSpider.DotnetSpiderEx.Http.ByteArrayContent;

namespace Larpx.ResourceSpider.DotnetSpiderEx.Downloader
{
    public class FileDownloader : IDownloader
    {
        public Task<Response> DownloadAsync(Request request)
        {
            var file = request.RequestUri.AbsoluteUri.Replace("file://", "");

            var response = new Response { RequestHash = request.Hash };
            if (!File.Exists(file))
            {
                response.StatusCode = HttpStatusCode.NotFound;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            response.TargetUrl = request.RequestUri.ToString();
            response.Content = new ByteArrayContent(File.ReadAllBytes(file));
            stopwatch.Stop();
            response.StatusCode = HttpStatusCode.OK;
            response.ElapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds;
            return Task.FromResult(response);
        }

        public string Name => Downloaders.File;
    }
}
