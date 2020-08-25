using Larpx.ResourceSpider.Http.Content;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Larpx.ResourceSpider.Http.Downloader
{
    public class FileDownloader : IDownloader
    {
        public Task<Response> DownloadResponseAsync(Request request)
        {
            var file = request.RequestUri.AbsoluteUri.Replace("file://", "");
            var response = new Response { RequestHash = request.Hash };
            if (!File.Exists(file))
            {
                response.StatusCode = HttpStatusCode.NotFound;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            response.TargetUri = request.RequestUri.ToString();
            response.Content = new ResponseContent { Data = File.ReadAllBytes(file) };
            stopwatch.Stop();
            response.StatusCode = HttpStatusCode.OK;
            response.ElapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds;
            return Task.FromResult(response);
        }

        public Task<string> DownloadStringAsync(Request request)
        {
            var file = request.RequestUri.AbsoluteUri.Replace("file://", "");
            string sResult = "";
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            sResult = File.ReadAllText(file);
            stopwatch.Stop();
            return Task.FromResult(sResult);
        }
    }
}
