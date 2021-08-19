using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotnetSpider.Http
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// 将HttpResponseMessgae转换为Response
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <returns></returns>
        public static async Task<Response> ToResponseAsync(this HttpResponseMessage httpResponseMessage)
        {
            var response = new Response
            {
                StatusCode = httpResponseMessage.StatusCode
            };

            foreach (var header in httpResponseMessage.Headers)
            {
                response.Headers.Add(header.Key, header.Value?.ToString());
            }

            response.Version = httpResponseMessage.Version == null
                ? HttpVersion.Version11
                : httpResponseMessage.Version;

            ///Http内容体编码
            response.Content.Headers.Add(HeaderNames.ContentCharset, httpResponseMessage.Content.Headers.ContentType.CharSet);

            ///http内容是否分段
            response.Headers.TransferEncodingChunked = httpResponseMessage.Headers.TransferEncodingChunked;

            ///http内容
            response.Content = new ByteArrayContent(await httpResponseMessage.Content.ReadAsByteArrayAsync());

            ///解析头信息
            foreach (var header in httpResponseMessage.Content.Headers)
            {
                response.Content.Headers.Add(header.Key, header.Value?.ToString());
            }

            return response;
        }
    }
}
