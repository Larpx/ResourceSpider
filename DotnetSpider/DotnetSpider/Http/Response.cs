using System;
using System.Net;
using System.Text;

namespace DotnetSpider.Http
{
    [Serializable]
    public class Response : IDisposable
    {
        private ResponseHeaders _headers;
        private Version _version;
        private ResponseHeaders _trailingHeaders;
        private bool _disposed;

        /// <summary>
        /// 响应头
        /// </summary>
        public ResponseHeaders Headers => _headers ??= new ResponseHeaders();

        /// <summary>
        /// 尾部标题
        /// </summary>
        public ResponseHeaders TrailingHeaders => _trailingHeaders ??= new ResponseHeaders();

        /// <summary>
        /// 代理人ID
        /// </summary>
        public string Agent { get; set; }

        /// <summary>
        /// Request
        /// </summary>
        public string RequestHash { get; set; }

        /// <summary>
        /// Http version
        /// </summary>
        public Version Version
        {
            get => _version;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _version = value;
            }
        }

        /// <summary>
        /// 返回状态码
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// 获取或设置原因短语，该短语通常由服务器与状态代码一起发送。
        /// </summary>
        public string ReasonPhrase { get; set; }

        /// <summary>
        /// 下载内容
        /// </summary>
        public ByteArrayContent Content { get; set; }

        /// <summary>
        /// 下载消耗的时间
        /// </summary>
        public int ElapsedMilliseconds { get; set; }

        /// <summary>
        /// 最终地址
        /// </summary>
        public string TargetUrl { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccessStatusCode =>
            StatusCode >= HttpStatusCode.OK && StatusCode <= (HttpStatusCode)299;

        /// <summary>
        /// 读取流内容
        /// </summary>
        /// <returns></returns>
        public string ReadAsString()
        {
            var charset = string.IsNullOrWhiteSpace( Content.Headers.ContentCharset)? "UTF-8" : Content.Headers.ContentCharset;
            return Encoding.GetEncoding(charset).GetString(Content.Bytes);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }

            _disposed = true;

            _headers?.Clear();
            _headers = null;

            _trailingHeaders?.Clear();
            _trailingHeaders = null;

            Content?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("StatusCode: ");
            sb.Append((int)StatusCode);
            sb.Append(", ReasonPhrase: '");
            sb.Append(ReasonPhrase ?? "<null>");
            sb.Append("', Version: ");
            sb.Append(_version);
            sb.Append(", Content: ");
            sb.Append(Content == null ? "<null>" : Content.GetType().ToString());
            sb.AppendLine(", Headers:");
            HeaderUtilities.DumpHeaders(sb, _headers, Content?.Headers);

            if (_trailingHeaders == null)
            {
                return sb.ToString();
            }

            sb.AppendLine(", Trailing Headers:");
            HeaderUtilities.DumpHeaders(sb, _trailingHeaders);

            return sb.ToString();
        }
    }
}
