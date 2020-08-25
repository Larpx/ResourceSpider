using Larpx.ResourceSpider.Http.Content;
using Larpx.ResourceSpider.Http.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ByteArrayContent = Larpx.ResourceSpider.Http.Content.ByteArrayContent;
using FormUrlEncodedContent = Larpx.ResourceSpider.Http.Content.FormUrlEncodedContent;
using StringContent = Larpx.ResourceSpider.Http.Content.StringContent;

namespace Larpx.ResourceSpider.Http.Downloader
{
    public class HttpClientDownloader : IDownloader
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly PPPoEService _pppoeService;

        public HttpClientDownloader(IHttpClientFactory httpClientFactory,
            ILogger<HttpClientDownloader> logger,
            PPPoEService pppoeService = null)
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.MaxServicePointIdleTime = 2000;

            _httpClientFactory = httpClientFactory;
            _logger = logger;
            if (pppoeService != null)
                _pppoeService = pppoeService;
        }

        public async Task<Response> DownloadResponseAsync(Request request)
        {
            try
            {
                string sPppoeErroeMessage = "";
                var stopwatch = new Stopwatch();
                var clientName = string.IsNullOrWhiteSpace(request.Proxy)
                    ? request.RequestUri.Host
                    : $"{Consts.ProxyPrefix}{request.Proxy}";

                //创建HttpClient，创建请求对象
                var httpClient = _httpClientFactory.CreateClient(clientName);
                var httpRequest = GenerateHttpRequestMessage(request);

                //执行网络请求
                stopwatch.Start();
                var httpResponseMessage = await httpClient.SendAsync(httpRequest);
                stopwatch.Stop();

                //是否使用PPPOE
                if (request.DownloaderType == DownloaderTypeNames.HttpClientWithADSL && request.DownloaderType == DownloaderTypeNames.PuppeteerWithADSL)
                {
                    var oResponse = _pppoeService.DetectAsync(request, out sPppoeErroeMessage);
                    if (oResponse != null)
                    {
                        _logger.LogError($"{request.RequestUri} 下载失败，ADSL拨号时出现问题，错误信息：{sPppoeErroeMessage}");
                        return oResponse;
                    }
                }

                //处理返回信息
                var response = new Response
                {
                    ElapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds,
                    StatusCode = httpResponseMessage.StatusCode
                };

                //处理头信息
                foreach (var header in httpResponseMessage.Headers)
                {
                    response.Headers.Add(header.Key, new HashSet<string>(header.Value));
                }

                response.RequestHash = request.Hash;
                response.Content = new ResponseContent
                {
                    Data = await httpResponseMessage.Content.ReadAsByteArrayAsync()
                };
                foreach (var header in httpResponseMessage.Content.Headers)
                {
                    response.Content.Headers.Add(header.Key, new HashSet<string>(header.Value));
                }

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError($"{request.RequestUri} 下载失败，错误信息: {e}");
                return new Response
                {
                    RequestHash = request.Hash,
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new ResponseContent { Data = Encoding.UTF8.GetBytes(e.ToString()) }
                };
            }
        }

        public async Task<string> DownloadStringAsync(Request request)
        {
            try
            {
                string sPppoeErroeMessage = "";
                var stopwatch = new Stopwatch();
                var clientName = string.IsNullOrWhiteSpace(request.Proxy)
                    ? request.RequestUri.Host
                    : $"{Consts.ProxyPrefix}{request.Proxy}";

                //创建HttpClient，创建请求对象
                var httpClient = _httpClientFactory.CreateClient(clientName);
                var httpRequest = GenerateHttpRequestMessage(request);

                //执行网络请求
                stopwatch.Start();
                var httpResponseMessage = await httpClient.SendAsync(httpRequest);
                stopwatch.Stop();

                //是否使用PPPOE
                if (request.DownloaderType == DownloaderTypeNames.HttpClientWithADSL && request.DownloaderType == DownloaderTypeNames.PuppeteerWithADSL)
                {
                    var oResponse = _pppoeService.DetectAsync(request, out sPppoeErroeMessage);
                    if (oResponse != null)
                    {
                        _logger.LogError($"{request.RequestUri} 下载失败，ADSL拨号时出现问题，错误信息：{sPppoeErroeMessage}");
                        return null;
                    }
                }

                //处理返回信息
                return httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                _logger.LogError($"{request.RequestUri} 下载失败，错误信息: {e}");
                return null;
            }
        }

        private HttpRequestMessage GenerateHttpRequestMessage(Request request)
        {
            try
            {
                var httpRequestMessage = new HttpRequestMessage(
                            string.IsNullOrWhiteSpace(request.Method) ? HttpMethod.Get : new HttpMethod(request.Method.ToUpper()),
                            request.RequestUri);

                //添加头信息
                foreach (var header in request.Headers)
                {
                    httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                //添加UA
                if (string.IsNullOrWhiteSpace(request.UserAgent))
                {
                    httpRequestMessage.Headers.TryAddWithoutValidation("User-Agent", GetRandomUserAgent());
                }

                if (request.Method.ToUpper() == "POST")
                {
                    var content = request.GetContentObject();

                    if (content != null)
                    {
                        if (content is StringContent stringContent)
                        {
                            ///StringContent => Content-Type:application/json等
                            httpRequestMessage.Content = new System.Net.Http.StringContent(
                                stringContent.Content,
                                Encoding.GetEncoding(stringContent.EncodingName), stringContent.MediaType);
                        }
                        else if (content is ByteArrayContent byteArrayContent && byteArrayContent.Bytes != null)
                        {
                            httpRequestMessage.Content = new System.Net.Http.ByteArrayContent(byteArrayContent.Bytes);
                        }
                        else if (content is FormUrlEncodedContent formUrlEncodedContent)
                        {
                            //FormUrlEncodedContent => Content-Type:application/x-www-form-urlencoded
                            httpRequestMessage.Content = new System.Net.Http.FormUrlEncodedContent(formUrlEncodedContent.NameValueCollection);
                        }

                        foreach (var header in content.Headers)
                        {
                            httpRequestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                }

                return httpRequestMessage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取随机的User Agent
        /// </summary>
        /// <returns></returns>
        private string GetRandomUserAgent()
        {
            Random oRandom = new Random();
            List<string> oList = new List<string> {
                 @"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_3) AppleWebKit/535.20 (KHTML, like Gecko) Chrome/19.0.1036.7 Safari/535.20",
                 @"Mozilla/5.0 (X11; U; Linux i686; en-US; rv:1.9.0.8) Gecko Fedora/1.9.0.8-1.fc10 Kazehakase/0.5.6",
                 @"Mozilla/5.0 (X11; U; Linux x86_64; zh-CN; rv:1.9.2.10) Gecko/20100922 Ubuntu/10.10 (maverick) Firefox/3.6.10",
                 @"Mozilla/5.0 (X11; U; Linux i686; en-US; rv:1.8.0.12) Gecko/20070731 Ubuntu/dapper-security Firefox/1.5.0.12",
                 @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36",
                 @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.135 Safari/537.36",
                 @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586",
                 @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299",
                 @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.108 Safari/537.36",
                 @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Safari/537.36",
                 @"Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko",
                 @"Mozilla/5.0 (Windows NT 10.0; WOW64; rv:49.0) Gecko/20100101 Firefox/49.0",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64; rv:46.0) Gecko/20100101 Firefox/46.0",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/21.0.1180.71 Safari/537.1 LBBROWSER",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/21.0.1180.89 Safari/537.1",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.64 Safari/537.11",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.87 Safari/537.36 OPR/37.0.2178.32",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.87 Safari/537.36",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 BIDUBrowser/8.3 Safari/537.36",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Maxthon/4.9.2.1000 Chrome/39.0.2146.0 Safari/537.36",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.80 Safari/537.36 Core/1.47.277.400 QQBrowser/9.4.7658.400",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 UBrowser/5.6.12150.8 Safari/537.36",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.122 Safari/537.36 SE 2.X MetaSr 1.0",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.154 Safari/537.36 LBBROWSER",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36 TheWorld 7",
                 @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/534.57.2 (KHTML, like Gecko) Version/5.1.7 Safari/534.57.2"
            };

            return oList[oRandom.Next(0, oList.Count - 1)];
        }
    }
}
