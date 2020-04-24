using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Larpx.ResourceSpider.BaseLibrary.Data
{
    public class ClassData
    {
        /// <summary>
        /// Http请求参考类
        /// </summary>
        public class HttpItem
        {
            /// <summary>
            /// 请求URL必须填写
            /// </summary>
            public string URL { get; set; }
            string _Method = "GET";
            /// <summary>
            /// 请求方式默认为GET方式,当为POST方式时必须设置Postdata的值
            /// </summary>
            public string Method
            {
                get { return _Method; }
                set { _Method = value; }
            }
            int _Timeout = 100000;
            /// <summary>
            /// 默认请求超时时间
            /// </summary>
            public int Timeout
            {
                get { return _Timeout; }
                set { _Timeout = value; }
            }
            int _ReadWriteTimeout = 30000;
            /// <summary>
            /// 默认写入Post数据超时间
            /// </summary>
            public int ReadWriteTimeout
            {
                get { return _ReadWriteTimeout; }
                set { _ReadWriteTimeout = value; }
            }
            /// <summary>
            /// 设置Host的标头信息
            /// </summary>
            public string Host { get; set; }
            Boolean _KeepAlive = true;
            /// <summary>
            ///  获取或设置一个值，该值指示是否与 Internet 资源建立持久性连接默认为true。
            /// </summary>
            public Boolean KeepAlive
            {
                get { return _KeepAlive; }
                set { _KeepAlive = value; }
            }
            string _Accept = "text/html, application/xhtml+xml, */*";
            /// <summary>
            /// 请求标头值 默认为text/html, application/xhtml+xml, */*
            /// </summary>
            public string Accept
            {
                get { return _Accept; }
                set { _Accept = value; }
            }
            string _ContentType = "text/html";
            /// <summary>
            /// 请求返回类型默认 text/html
            /// </summary>
            public string ContentType
            {
                get { return _ContentType; }
                set { _ContentType = value; }
            }
            string _UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            /// <summary>
            /// 客户端访问信息默认Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)
            /// </summary>
            public string UserAgent
            {
                get { return _UserAgent; }
                set { _UserAgent = value; }
            }
            /// <summary>
            /// 返回数据编码默认为NUll,可以自动识别,一般为utf-8,gbk,gb2312
            /// </summary>
            public Encoding Encoding { get; set; }
            private PostDataType _PostDataType = PostDataType.String;
            /// <summary>
            /// Post的数据类型
            /// </summary>
            public PostDataType PostDataType
            {
                get { return _PostDataType; }
                set { _PostDataType = value; }
            }
            /// <summary>
            /// Post请求时要发送的字符串Post数据
            /// </summary>
            public string Postdata { get; set; }
            /// <summary>
            /// Post请求时要发送的Byte类型的Post数据
            /// </summary>
            public byte[] PostdataByte { get; set; }
            /// <summary>
            /// Cookie对象集合
            /// </summary>
            public CookieCollection CookieCollection { get; set; }
            /// <summary>
            /// 请求时的Cookie
            /// </summary>
            public string Cookie { get; set; }
            /// <summary>
            /// 来源地址，上次访问地址
            /// </summary>
            public string Referer { get; set; }
            /// <summary>
            /// 证书绝对路径
            /// </summary>
            public string CerPath { get; set; }
            /// <summary>
            /// 设置代理对象，不想使用IE默认配置就设置为Null，而且不要设置ProxyIp
            /// </summary>
            public WebProxy WebProxy { get; set; }
            private Boolean isToLower = false;
            /// <summary>
            /// 是否设置为全文小写，默认为不转化
            /// </summary>
            public Boolean IsToLower
            {
                get { return isToLower; }
                set { isToLower = value; }
            }
            private Boolean allowautoredirect = false;
            /// <summary>
            /// 支持跳转页面，查询结果将是跳转后的页面，默认是不跳转
            /// </summary>
            public Boolean Allowautoredirect
            {
                get { return allowautoredirect; }
                set { allowautoredirect = value; }
            }
            private int connectionlimit = 1024;
            /// <summary>
            /// 最大连接数
            /// </summary>
            public int Connectionlimit
            {
                get { return connectionlimit; }
                set { connectionlimit = value; }
            }
            /// <summary>
            /// 代理Proxy 服务器用户名
            /// </summary>
            public string ProxyUserName { get; set; }
            /// <summary>
            /// 代理 服务器密码
            /// </summary>
            public string ProxyPwd { get; set; }
            /// <summary>
            /// 代理 服务IP,如果要使用IE代理就设置为ieproxy
            /// </summary>
            public string ProxyIp { get; set; }
            private ResultType resulttype = ResultType.String;
            /// <summary>
            /// 设置返回类型String和Byte
            /// </summary>
            public ResultType ResultType
            {
                get { return resulttype; }
                set { resulttype = value; }
            }
            private WebHeaderCollection header = new WebHeaderCollection();
            /// <summary>
            /// header对象
            /// </summary>
            public WebHeaderCollection Header
            {
                get { return header; }
                set { header = value; }
            }
            /// <summary>
            //     获取或设置用于请求的 HTTP 版本。返回结果:用于请求的 HTTP 版本。默认为 System.Net.HttpVersion.Version11。
            /// </summary>
            public Version ProtocolVersion { get; set; }
            private Boolean _expect100continue = true;
            /// <summary>
            ///  获取或设置一个 System.Boolean 值，该值确定是否使用 100-Continue 行为。如果 POST 请求需要 100-Continue 响应，则为 true；否则为 false。默认值为 true。
            /// </summary>
            public Boolean Expect100Continue
            {
                get { return _expect100continue; }
                set { _expect100continue = value; }
            }
            /// <summary>
            /// 设置509证书集合
            /// </summary>
            public X509CertificateCollection ClentCertificates { get; set; }
            /// <summary>
            /// 设置或获取Post参数编码,默认的为Default编码
            /// </summary>
            public Encoding PostEncoding { get; set; }
            private ResultCookieType _ResultCookieType = ResultCookieType.String;
            /// <summary>
            /// Cookie返回类型,默认的是只返回字符串类型
            /// </summary>
            public ResultCookieType ResultCookieType
            {
                get { return _ResultCookieType; }
                set { _ResultCookieType = value; }
            }
            private ICredentials _ICredentials = CredentialCache.DefaultCredentials;
            /// <summary>
            /// 获取或设置请求的身份验证信息。
            /// </summary>
            public ICredentials ICredentials
            {
                get { return _ICredentials; }
                set { _ICredentials = value; }
            }
            /// <summary>
            /// 设置请求将跟随的重定向的最大数目
            /// </summary>
            public int MaximumAutomaticRedirections { get; set; }
            private DateTime? _IfModifiedSince = null;
            /// <summary>
            /// 获取和设置IfModifiedSince，默认为当前日期和时间
            /// </summary>
            public DateTime? IfModifiedSince
            {
                get { return _IfModifiedSince; }
                set { _IfModifiedSince = value; }
            }
            #region ip-port
            private IPEndPoint _IPEndPoint = null;
            /// <summary>
            /// 设置本地的出口ip和端口
            /// </summary>]
            /// <example>
            ///item.IPEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"),80);
            /// </example>
            public IPEndPoint IPEndPoint
            {
                get { return _IPEndPoint; }
                set { _IPEndPoint = value; }
            }
            #endregion
        }
        /// <summary>
        /// Http返回参数类
        /// </summary>
        public class HttpResult
        {
            /// <summary>
            /// Http请求返回的Cookie
            /// </summary>
            public string Cookie { get; set; }
            /// <summary>
            /// Cookie对象集合
            /// </summary>
            public CookieCollection CookieCollection { get; set; }
            private string _html = string.Empty;
            /// <summary>
            /// 返回的String类型数据 只有ResultType.String时才返回数据，其它情况为空
            /// </summary>
            public string Html
            {
                get { return _html; }
                set { _html = value; }
            }
            /// <summary>
            /// 返回的Byte数组 只有ResultType.Byte时才返回数据，其它情况为空
            /// </summary>
            public byte[] ResultByte { get; set; }
            /// <summary>
            /// header对象
            /// </summary>
            public WebHeaderCollection Header { get; set; }
            /// <summary>
            /// 返回状态说明
            /// </summary>
            public string StatusDescription { get; set; }
            /// <summary>
            /// 返回状态码,默认为OK
            /// </summary>
            public HttpStatusCode StatusCode { get; set; }
            /// <summary>
            /// 最后访问的URl
            /// </summary>
            public string ResponseUri { get; set; }
            /// <summary>
            /// 获取重定向的URl
            /// </summary>
            public string RedirectUrl
            {
                get
                {
                    try
                    {
                        if (Header != null && Header.Count > 0)
                        {
                            if (Header.AllKeys.Any(k => k.ToLower().Contains("location")))
                            {
                                string locationurl = Header["location"].ToString().ToLower();

                                if (!string.IsNullOrWhiteSpace(locationurl))
                                {
                                    bool b = locationurl.StartsWith("http://") || locationurl.StartsWith("https://");
                                    if (!b)
                                    {
                                        locationurl = new Uri(new Uri(ResponseUri), locationurl).AbsoluteUri;
                                    }
                                }
                                return locationurl;
                            }
                        }
                    }
                    catch { }
                    return string.Empty;
                }
            }
        }
        /// <summary>
        /// 返回类型
        /// </summary>
        public enum ResultType
        {
            /// <summary>
            /// 表示只返回字符串 只有Html有数据
            /// </summary>
            String,
            /// <summary>
            /// 表示返回字符串和字节流 ResultByte和Html都有数据返回
            /// </summary>
            Byte
        }
        /// <summary>
        /// Post的数据格式默认为string
        /// </summary>
        public enum PostDataType
        {
            /// <summary>
            /// 字符串类型，这时编码Encoding可不设置
            /// </summary>
            String,
            /// <summary>
            /// Byte类型，需要设置PostdataByte参数的值编码Encoding可设置为空
            /// </summary>
            Byte,
            /// <summary>
            /// 传文件，Postdata必须设置为文件的绝对路径，必须设置Encoding的值
            /// </summary>
            FilePath
        }
        /// <summary>
        /// Cookie返回类型
        /// </summary>
        public enum ResultCookieType
        {
            /// <summary>
            /// 只返回字符串类型的Cookie
            /// </summary>
            String,
            /// <summary>
            /// CookieCollection格式的Cookie集合同时也返回String类型的cookie
            /// </summary>
            CookieCollection
        }
    }

    /// <summary>
    /// Json通信模型，返回任务结果集合
    /// </summary>
    [Serializable]
    public class ResultList
    {
        private Int32 _Code;
        private String _Message;
        private int _Count;
        private List<ProcessTask> _Data;

        public ResultList()
        {
            Count = 0;
            _Data = new List<ProcessTask>();
        }

        public ResultList(Int32 _C, String _M, List<ProcessTask> _D)
        {
            Code = _C;
            Message = _M;
            Count = _D.Count;
            Data = _D;
        }

        /// <summary>
        /// 程序结果
        /// 0.执行失败
        /// 1.执行成功
        /// </summary>
        public int Code { get => _Code; set => _Code = value; }

        /// <summary>
        /// 请求结果信息
        /// </summary>
        public string Message { get => _Message; set => _Message = value; }

        /// <summary>
        /// 结果集合数量
        /// </summary>
        public int Count { get => _Count; set => _Count = value; }

        /// <summary>
        /// 结果结合
        /// </summary>
        public List<ProcessTask> Data { get => _Data; set => _Data = value; }
    }

    /// <summary>
    /// Json通信模型
    /// </summary>
    [Serializable]
    public class Result
    {
        private Int32 _Code;
        private String _Message;
        private Object _Data;

        public int Code { get => _Code; set => _Code = value; }
        public string Message { get => _Message; set => _Message = value; }
        public Object Data { get => _Data; set => _Data = value; }

        public Result()
        {
        }

        public Result(Int32 _C, String _M, Object _D)
        {
            Code = _C;
            Message = _M;
            Data = _D;
        }
    }

    /// <summary>
    /// 任务模型
    /// </summary>
    [Serializable]
    public class ProcessTask
    {
        private Guid _GUID;
        private string _ID;
        private string _ANSI;
        private string _Link;
        private byte _Type;

        public ProcessTask(Guid guid, string sID, string sANSI, string sLink)
        {
            _GUID = guid;
            _ID = sID;
            ANSI = sANSI;
            _Link = sLink;
            _Type = 0;
        }

        public ProcessTask(Guid guid, string sID, string sANSI, string sLink, byte bStatus)
        {
            _GUID = guid;
            _ID = sID;
            ANSI = sANSI;
            _Link = sLink;
            _Type = bStatus;
        }

        /// <summary>
        /// GUID
        /// </summary>
        public Guid GUID { get => _GUID; set => _GUID = value; }

        /// <summary>
        /// 校验值
        /// </summary>
        public string ID { get => _ID; set => _ID = value; }

        /// <summary>
        /// 商品链接
        /// </summary>
        public string Link { get => _Link; set => _Link = value; }

        /// <summary>
        /// 商品ANSI
        /// </summary>
        public string ANSI { get => _ANSI; set => _ANSI = value; }

        /// <summary>
        /// 操作类型
        /// 0.补充详情
        /// 1.更新信息
        /// 2.更新价格
        /// 3.翻译
        /// </summary>
        public byte Type { get => _Type; set => _Type = value; }
    }

    /// <summary>
    /// 附加内容
    /// </summary>
    [Serializable]
    public class AttachmentsItem
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 详情
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// 颜色
        /// </summary>
        public string color { get; set; }
    }

    /// <summary>
    /// 倍洽机器人提示信息
    /// </summary>
    [Serializable]
    public class BearyChat
    {
        public BearyChat()
        {
            attachments = new List<AttachmentsItem>();
        }

        /// <summary>
        /// 愿原力与你同在
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// 附加内容
        /// </summary>
        public List<AttachmentsItem> attachments { get; set; }
    }
}
