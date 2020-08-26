using Newtonsoft.Json;

namespace Larpx.ResourceSpider.Http
{
    public static class HeaderNames
    {
        public const string Accept = "Accept";
        public const string AcceptCharset = "Accept-Charset";
        public const string AcceptEncoding = "Accept-Encoding";
        public const string AcceptLanguage = "Accept-Language";
        public const string AcceptRanges = "Accept-Ranges";
        public const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";
        public const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";
        public const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        public const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        public const string AccessControlExposeHeaders = "Access-Control-Expose-Headers";
        public const string AccessControlMaxAge = "Access-Control-Max-Age";
        public const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
        public const string AccessControlRequestMethod = "Access-Control-Request-Method";
        public const string Age = "Age";
        public const string Allow = "Allow";
        public const string Authority = ":authority";
        public const string Authorization = "Authorization";
        public const string CacheControl = "Cache-Control";
        public const string Connection = "Connection";
        public const string ContentDisposition = "Content-Disposition";
        public const string ContentEncoding = "Content-Encoding";
        public const string ContentLanguage = "Content-Language";
        public const string ContentLength = "Content-Length";
        public const string ContentLocation = "Content-Location";
        public const string ContentMD5 = "Content-MD5";
        public const string ContentRange = "Content-Range";
        public const string ContentSecurityPolicy = "Content-Security-Policy";
        public const string ContentSecurityPolicyReportOnly = "Content-Security-Policy-Report-Only";
        public const string ContentType = "Content-Type";
        public const string Cookie = "Cookie";
        public const string Date = "Date";
        public const string ETag = "ETag";
        public const string Expires = "Expires";
        public const string Expect = "Expect";
        public const string From = "From";
        public const string Host = "Host";
        public const string IfMatch = "If-Match";
        public const string IfModifiedSince = "If-Modified-Since";
        public const string IfNoneMatch = "If-None-Match";
        public const string IfRange = "If-Range";
        public const string IfUnmodifiedSince = "If-Unmodified-Since";
        public const string LastModified = "Last-Modified";
        public const string Location = "Location";
        public const string MaxForwards = "Max-Forwards";
        public const string Method = ":method";
        public const string Origin = "Origin";
        public const string Path = ":path";
        public const string Pragma = "Pragma";
        public const string ProxyAuthenticate = "Proxy-Authenticate";
        public const string ProxyAuthorization = "Proxy-Authorization";
        public const string Range = "Range";
        public const string Referer = "Referer";
        public const string RetryAfter = "Retry-After";
        public const string Scheme = ":scheme";
        public const string Server = "Server";
        public const string SetCookie = "Set-Cookie";
        public const string Status = ":status";
        public const string StrictTransportSecurity = "Strict-Transport-Security";
        public const string TE = "TE";
        public const string Trailer = "Trailer";
        public const string TransferEncoding = "Transfer-Encoding";
        public const string Upgrade = "Upgrade";
        public const string UserAgent = "User-Agent";
        public const string Vary = "Vary";
        public const string Via = "Via";
        public const string Warning = "Warning";
        public const string WebSocketSubProtocols = "Sec-WebSocket-Protocol";
        public const string WWWAuthenticate = "WWW-Authenticate";
    }

    public static class Consts
    {
        public const string ProxyPrefix = "PROXY_";
        public const string RedialRegexExpression = "REDIAL_REGEXP";
        public const string ResponseBytes = "RESPONSE_BYTES";
    }

    public class PPPoEOptions
    {
        /// <summary>
        /// 节点类型
        /// ADSL 和普通型不能混合部署
        /// </summary>
        public string ADSLAccount { get; set; }
        public string ADSLPassword { get; set; }
        /// <summary>
        /// ADSL 网络接口
        /// </summary>
        public string ADSLInterface { get; set; }
    }

    /// <summary>
    /// 下载策略
    /// </summary>
    public enum RequestPolicy
    {
        /// <summary>
        /// 随机
        /// </summary>
        Random,

        /// <summary>
        /// 链式
        /// </summary>
        Chained
    }

    /// <summary>
    /// 下载类型
    /// </summary>
    public enum DownloaderTypeNames
    {
        /// <summary>
        /// 使用HttpClient
        /// </summary>
        HttpClient,

        /// <summary>
        /// 使用HttpClient并启用ADSL拨号
        /// </summary>
        HttpClientWithADSL,

        /// <summary>
        /// 使用Puppeteer模拟浏览器
        /// </summary>
        Puppeteer,

        /// <summary>
        /// 使用Puppeteer模拟浏览器并启用ADSL拨号
        /// </summary>
        PuppeteerWithADSL,

        /// <summary>
        /// 下载文件
        /// </summary>
        File

    }

    /// <summary>
    /// Puppeteer中Cookie中的SameSite值
    /// </summary>
    public enum SameSite
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 严格的
        /// </summary>
        Strict = 1,
        /// <summary>
        /// 松懈
        /// </summary>
        Lax = 2,
        /// <summary>
        /// 扩展
        /// </summary>
        Extended = 3
    }

    /// <summary>
    /// Cookies信息
    /// </summary>
    public class CookieItem : PuppeteerSharp.CookieParam
    {
        public CookieItem()
        {

        }

        ///// <summary>
        ///// Cookies名
        ///// </summary>
        //public string Name { get; set; }

        ///// <summary>
        ///// Cookie值
        ///// </summary>
        //public string Value { get; set; }

        ///// <summary>
        ///// Cookie作用域
        ///// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public string Domain { get; set; }

        ///// <summary>
        ///// 限定URL
        ///// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public string Url { get; set; }

        ///// <summary>
        ///// 路径
        ///// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public string Path { get; set; }

        ///// <summary>
        ///// 获取或设置过期时间。Unix时间（秒）
        ///// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public double? Expires { get; set; }

        ///// <summary>
        ///// 尺寸
        ///// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public int? Size { get; set; }

        ///// <summary>
        ///// 是否是HttpOnly
        ///// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public bool? HttpOnly { get; set; }

        ///// <summary>
        ///// 是否是安全的
        ///// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public bool? Secure { get; set; }

        ///// <summary>
        ///// 是否仅用作Session
        ///// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public bool? Session { get; set; }

        ///// <summary>
        ///// samesite值
        ///// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public SameSite? SameSite { get; set; }
    }
}
