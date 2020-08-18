using Larpx.ResourceSpider.Helpers.Encode;
using Larpx.ResourceSpider.Helpers.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace Larpx.ResourceSpider.Helpers
{
    public class TransHelper
    {
        private string appid;
        private string sAppsec;
        private readonly string sURL = "https://fanyi-api.baidu.com/api/trans/vip/translate";
        private JavaScriptHelper javaScriptHelper = new JavaScriptHelper();

        /// <summary>
        /// Google语言类型：
        /// 1.自动，2.中文，3.日语，4.法语，5.德语，
        /// 6.韩语，7.俄语，8.西班牙语，9.泰语，10.意大利语，11.葡萄牙语，12.阿拉伯语
        /// </summary>
        public Dictionary<string, string> dGoogleLanguageType = new Dictionary<string, string> {
             {"Auto","auto"},
            {"English","en" },
            {"Chinese","zh-CN"},
            {"Japanese","ja"},
            {"French","fr"},
            {"German","de"},
            {"Korean","ko" },
            {"Russian","ru" },
            {"Spanish","es" },
            {"Thai","th" },
            {"Italian","it" },
            {"Portuguese","pt" },
            {"Arabic","ar" },
            {"Nederland","nl" },
            {"Turkey","tr" }};

        /// <summary>
        /// BaiDu语言类型：
        /// 1.自动，2.中文，3.日语，4.法语，5.德语，
        /// 6.韩语，7.俄语，8.西班牙语，9.泰语，10.意大利语，11.葡萄牙语，12.阿拉伯语
        /// </summary>
        public Dictionary<string, string> dBaiDuLanguageType = new Dictionary<string, string> {
            {"Auto","auto"},
            {"Chinses","zh"},
            {"Japanese","jp"},
            {"French","fra"},
            {"German","de"},
            {"Korean","kor" },
            {"Russian","ru" },
            {"Spanish","spa" },
            {"Thai","th" },
            {"Italian","it" },
            {"Portuguese","pt" },
            {"Turkey","tr" },
            {"PortugueseBR","pt_BR" },
            {"Arabic","ara" }};

        public string Appid { get => appid; set => appid = value; }
        public string Appsec { get => sAppsec; set => sAppsec = value; }

        public TransHelper()
        {

        }

        public TransHelper(string sApp, string sSec)
        {
            Appid = sApp;
            Appsec = sSec;
        }

        public string GetTrans(string sQuery, string sTo, string sFrom = "auto")
        {
            int iSalt;
            string sSign;
            string sResult = "";

            try
            {
                Uri oURL = null;
                Random oRandom = new Random();

                iSalt = oRandom.Next(10000, 1000000);
                sSign = MD5.GetBufferHash(Appid + sQuery + iSalt + sAppsec).ToLower();
                sQuery = HttpUtility.UrlEncode(sQuery);
                oURL = new Uri(sURL
                    + "?q=" + sQuery
                    + "&from=" + sFrom
                    + "&to=" + sTo
                    + "&appid=" + Appid
                    + "&salt=" + iSalt
                    + "&sign=" + sSign
                    );

                sResult = Get(oURL);
                dynamic oResult = Newtonsoft.Json.Linq.JObject.Parse(sResult) as dynamic;
                if (oResult != null)
                {
                    oResult = Newtonsoft.Json.Linq.JArray.Parse(oResult.trans_result.ToString()) as dynamic;
                    sResult = oResult[0].dst;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return sResult;
        }

        /// <summary>
        /// 处理http GET请求，返回数据
        /// </summary>
        /// <param name="url">请求的url地址</param>
        /// <returns>http GET成功后返回的数据，失败抛WebException异常</returns>
        private static string Get(Uri url)
        {
            GC.Collect();
            string result = "";

            HttpWebRequest oRequest = null;
            HttpWebResponse oResponse = null;

            //请求url以获取数据
            try
            {
                //设置最大连接数
                ServicePointManager.DefaultConnectionLimit = 200;
                //设置https验证方式
                if (url.ToString().StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                            new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                oRequest = (HttpWebRequest)WebRequest.Create(url);

                oRequest.Method = "GET";
                oRequest.Referer = url.Host;

                //获取服务器返回
                using (oResponse = (HttpWebResponse)oRequest.GetResponse())
                {
                    //获取HTTP返回数据
                    using (StreamReader sr = new StreamReader(oResponse.GetResponseStream(), Encoding.UTF8))
                    {
                        result = sr.ReadToEnd().Trim();
                    }
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                System.Threading.Thread.ResetAbort();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    throw e;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //关闭连接和流
                if (oRequest != null)
                {
                    oRequest.Abort();
                }
            }
            return result;
        }

        /// <summary>
        /// 检查证书，适用于HTTPS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受   
        }

        #region Google翻译

        public string GoogleTranslate(string text, string fromLanguage, string toLanguage, CookieContainer cc = null)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            //string GoogleTransBaseUrl = "https://translate.google.cn/";
            //string BaseResultHtml = "";

            //cc = GetCookieHtml( GoogleTransBaseUrl, out BaseResultHtml );

            //Regex re = new Regex( @"(tkk:')(.*?)(')" );

            //var TKK = re.Match( BaseResultHtml ).ToString().TrimEnd( '\'' );//在返回的HTML中正则匹配TKK的值
            //TKK = TKK.Substring( 4, TKK.Length - 5 );

            //if(File.Exists())
            var GetTkkJS = File.ReadAllText("./Scripts/gettk.js");
            var tk = javaScriptHelper.ExecuteScript("token(\"" + text + "\")", GetTkkJS);

            string googleTransUrl = "https://translate.google.cn/translate_a/single?client=webapp&sl=" + fromLanguage + "&tl=" + toLanguage + "&hl=" + toLanguage + "&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&otf=1&ssel=3&tsel=0&kc=1&tk=" + tk + "&q=" + HttpUtility.UrlEncode(text);
            var ResultHtml = GetResultHtml(googleTransUrl, cc, "translate.google.cn");

            dynamic TempResult = Newtonsoft.Json.JsonConvert.DeserializeObject(ResultHtml);

            string ResultText;
            if (TempResult[5].Count == 1 && TempResult[5][0].Count > 2 && TempResult[5][0][2].Count > 1 && TempResult[5][0][2][1].Count == 1)
                //全翻译，全部为中文
                ResultText = Convert.ToString(TempResult[5][0][2][1][0]);
            else
                //精简翻译，保留部分外文，中外结合
                ResultText = Convert.ToString(TempResult[0][0][0]);

            return ResultText;
        }

        private string GetResultHtml(string url, CookieContainer cookie, string referer)
        {
            var html = "";
            var webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "GET";
            //网上程序代码,自己用chrome浏览器F12查看追踪修改为下列两行,同样执行成功.20180427
            webRequest.CookieContainer = cookie;
            webRequest.Referer = referer;
            webRequest.Timeout = 20000;
            webRequest.Headers.Add("X-Requested-With:XMLHttpRequest");
            webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*; q = 0.8";

            webRequest.Accept = "*/*";
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";

            using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                {
                    html = reader.ReadToEnd();
                    reader.Close();
                    webResponse.Close();
                }
            }
            return html;
        }

        public CookieContainer GetCookieHtml(string url, out string sHtml)
        {
            sHtml = "";
            CookieContainer cookie = new CookieContainer();
            if (url.ToString().StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                        new RemoteCertificateValidationCallback(CheckValidationResult);
            }

            var webRequest = WebRequest.Create(url) as HttpWebRequest;

            webRequest.Method = "GET";

            webRequest.CookieContainer = cookie;
            webRequest.Credentials = CredentialCache.DefaultCredentials;
            //webRequest.Referer = referer;

            webRequest.Timeout = 20000;

            //webRequest.Headers.Add( "X-Requested-With:XMLHttpRequest" );
            webRequest.Headers.Add("x-client-data:CIi2yQEIorbJAQjEtskBCKmdygEIqKPKAQi/p8oBCOynygEIi6jKAQjiqMoBGPmlygE=");
            webRequest.Headers.Add("dnt:1");
            webRequest.Headers.Add("accept-language:zh-CN,zh;q=0.9");
            webRequest.Accept = "*/*";

            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36";

            using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                    {
                        sHtml = reader.ReadToEnd();
                    }
                    cookie.Add(webResponse.Cookies);
                }
            }
            return cookie;
        }

        #endregion
    }

    public class GoogleTranslateHelper
    {
        private string sBaseURL = "https://translate.google.cn";
        private CookieContainer oCookieContainer = null;
        private JavaScriptHelper javaScriptHelper = new JavaScriptHelper();

        /// <summary>
        /// Google语言类型：
        /// 1.自动，2.中文，3.日语，4.法语，5.德语，
        /// 6.韩语，7.俄语，8.西班牙语，9.泰语，10.意大利语，11.葡萄牙语，12.阿拉伯语
        /// </summary>
        public Dictionary<string, string> dGoogleLanguageType = new Dictionary<string, string> {
            {"Auto","auto"},
            {"English","en" },
            {"Chinese","zh-CN"},
            {"Japanese","ja"},
            {"French","fr"},
            {"German","de"},
            {"Korean","ko" },
            {"Russian","ru" },
            {"Spanish","es" },
            {"Thai","th" },
            {"Italian","it" },
            {"Portuguese","pt" },
            {"Arabic","ar" },
            {"Nederland","nl" },
            {"Turkey","tr" }};


        public GoogleTranslateHelper()
        {
            if (oCookieContainer == null)
                oCookieContainer = GetCookie(sBaseURL);
        }

        public string GoogleTranslate(string text, string fromLanguage, string toLanguage)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            var GetTkkJS = File.ReadAllText("./Scripts/gettk.js");
            var tk = javaScriptHelper.ExecuteScript("token(\"" + text + "\")", GetTkkJS);

            string googleTransUrl = sBaseURL + "/translate_a/single?client=webapp&sl=" + fromLanguage + "&tl=" + toLanguage + "&hl=" + toLanguage + "&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&otf=1&ssel=3&tsel=0&kc=1&tk=" + tk + "&q=" + HttpUtility.UrlEncode(text);
            var ResultHtml = GetResultHtml(googleTransUrl, oCookieContainer);

            if (oCookieContainer == null)
                oCookieContainer = GetCookie(sBaseURL);

            dynamic TempResult = Newtonsoft.Json.JsonConvert.DeserializeObject(ResultHtml);

            string ResultText = Convert.ToString(TempResult[0][0][0]);

            return ResultText;
        }

        #region 私有方法

        private string GetResultHtml(string url, CookieContainer cookie)
        {
            var html = "";
            var webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "GET";
            //网上程序代码,自己用chrome浏览器F12查看追踪修改为下列两行,同样执行成功.20180427
            webRequest.CookieContainer = cookie;
            webRequest.Referer = sBaseURL;
            webRequest.Timeout = 1000 * 20;
            webRequest.Headers.Add("X-Requested-With:XMLHttpRequest");
            webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*; q = 0.8";

            webRequest.Accept = "*/*";
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";

            using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                {
                    html = reader.ReadToEnd();
                    reader.Close();
                    webResponse.Close();
                }
            }
            return html;
        }

        /// <summary>
        /// 检查证书，适用于HTTPS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受   
        }

        /// <summary>
        /// 获取请求用的Cookie
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private CookieContainer GetCookie(string url)
        {
            CookieContainer cookie = new CookieContainer();
            if (url.ToString().StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                        new RemoteCertificateValidationCallback(CheckValidationResult);
            }

            var webRequest = WebRequest.Create(url) as HttpWebRequest;

            webRequest.Method = "GET";
            webRequest.Credentials = CredentialCache.DefaultCredentials;
            webRequest.Timeout = 20000;
            //webRequest.Headers.Add( "X-Requested-With:XMLHttpRequest" );
            //webRequest.Headers.Add( "x-client-data:CIi2yQEIorbJAQjEtskBCKmdygEIqKPKAQi/p8oBCOynygEIi6jKAQjiqMoBGPmlygE=" );
            webRequest.Headers.Add("dnt:1");
            webRequest.Headers.Add("accept-language:zh-CN,zh;q=0.9");
            webRequest.Accept = "*/*";
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36";

            using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                if (webResponse.StatusCode == HttpStatusCode.OK)
                    cookie.Add(webResponse.Cookies);
                else
                    return null;
            }
            return cookie;
        }

        #endregion

    }
}
