using Larpx.ResourceSpider.Http.Content;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Larpx.ResourceSpider.Http.Downloader
{
    public class FileDownloader : IDownloader
    {
        private readonly ILogger _logger;

        public FileDownloader(ILogger<FileDownloader> logger)
        {
            _logger = logger;
        }

        public Task<Response> DownloadResponseAsync(Request request)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError($"{request.RequestUri} 下载失败，错误信息: {ex.Message}");
                throw ex;
            }
        }

        public Task<string> DownloadStringAsync(Request request)
        {
            try
            {
                var file = request.RequestUri.AbsoluteUri.Replace("file://", "");
                string sResult = "";
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                sResult = File.ReadAllText(file);
                stopwatch.Stop();
                return Task.FromResult(sResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{request.RequestUri} 下载失败，错误信息: {ex.Message}");
                throw ex;
            }
        }
    }
}
