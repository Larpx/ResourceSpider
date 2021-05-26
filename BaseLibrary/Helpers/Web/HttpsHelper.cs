using Larpx.ResourceSpider.BaseLibrary.Cache;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Larpx.ResourceSpider.BaseLibrary.Data.BaseClassData;

namespace Larpx.ResourceSpider.Helpers.Web
{
    public class EasyHttpHelper
    {
        /// <summary>
        /// 将获取到的Cookie存入redis中的哪个数据库中
        /// </summary>
        private static readonly int nRedisCookieDB = 0;

        /// <summary>
        /// Post 
        /// 通过提交表单的方式获取数据获取数据,需要判断返回结果是否为空
        /// </summary>
        /// <param name="url">Uri资源</param>
        /// <param name="sData">POST数据</param>
        /// <param name="n">循环次数</param>
        /// <returns>响应信息</returns>
        public static HttpWebResponse ReadDataByPostForm(string url, string sData, int n = 0,
            string sAccept = null, string sUserAgent = null, CookieContainer oCookieContainer = null)
        {
            int nSign = 4;
            try
            {
                if (n >= nSign)
                    return null;
                Random oRand = new Random();

                //设置https验证方式
                if (url.ToString().StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                            new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //url.Scheme + "://" + url.Host + HttpUtility.UrlEncode( url.AbsolutePath ) );
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                if (!String.IsNullOrEmpty(sAccept))
                    request.Accept = sAccept;
                else
                    request.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                if (!String.IsNullOrEmpty(sUserAgent))
                    request.UserAgent = sUserAgent;
                else
                    request.UserAgent = GetRandomUserAgent();
                request.Timeout = 20 * 1000;
                //request.Headers.Add( HttpRequestHeader.AcceptLanguage, @"zh-CN,zh;q=0.9,en;q=0.8" );
                request.Headers.Add(HttpRequestHeader.CacheControl, @"max-age=0");
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, @"gzip, deflate, br");
                request.Referer = new Uri(url).Host.ToString();

                //设置POST的数据类型和长度
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] data = Encoding.UTF8.GetBytes(sData);
                request.ContentLength = data.Length;

                //往服务器写入数据
                Stream oRequestStream = request.GetRequestStream();
                oRequestStream.Write(data, 0, data.Length);
                oRequestStream.Close();

                HttpWebResponse respone = (HttpWebResponse)request.GetResponse();
                int nRand = oRand.Next(20, 80);
                Console.WriteLine("Net Request Sleeping: " + nRand + "ms");
                if (respone.StatusCode == HttpStatusCode.OK)
                    return respone;
                else if (respone.StatusCode > HttpStatusCode.InternalServerError)
                {
                    n++;
                    return ReadDataByPostForm(url, sData, n);
                }
                else
                {
                    n++;
                    return ReadDataByPostForm(url, sData, n);
                }
            }
            catch (IOException e)
            {
                if (n > nSign)
                    throw e;
                else
                {
                    n++;
                    return ReadDataByPostForm(url, sData, n);
                }
            }
            catch (WebException e)
            {
                if (e.Message.Contains("40") || e.Message.Contains("50"))
                {
                    return null;
                }
                else
                {
                    if (n > nSign)
                        throw e;
                    else
                    {
                        n++;
                        return ReadDataByPostForm(url, sData, n);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("无效的控制字符"))
                {
                    if (n > nSign)
                        throw ex;
                    else
                    {
                        n++;
                        return ReadDataByPostForm(url, sData, n);
                    }
                }
                else
                    throw ex;
            }
        }

        /// <summary>
        /// Get
        /// 获取数据,需要判断返回结果是否为空
        /// </summary>
        /// <param name="url">Uri资源</param>
        /// <param name="sAccept">Accept头</param>
        /// <param name="sUserAgent">UserAgent头</param>
        /// <param name="oWebHeaderCollection">请求头信息</param>
        /// <param name="oCookieContainer">Cookie容器</param>
        /// <returns>响应信息</returns>
        public static HttpWebResponse ReadData(string url, int n = 0, string sRedisKey = null,
            string sAccept = null, string sUserAgent = null)
        {
            int nSign = 4;
            try
            {
                if (n >= nSign)
                    return null;

                Uri oURL = new Uri(url);
                Random oRand = new Random();
                string sRdsKey = string.IsNullOrEmpty(sRedisKey) ? oURL.Host : sRedisKey;
                StackExchangeRedisHelper oRedisCookie = new StackExchangeRedisHelper(nRedisCookieDB);

                //设置https验证方式
                if (url.ToString().StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                            new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                ServicePointManager.Expect100Continue = false;
                ServicePointManager.MaxServicePointIdleTime = 2000;

                //获取Cookie，从Redis
                if (oRedisCookie.IsSet(sRdsKey))
                {
                    var oCookies = CookieHelper.GetCookiesByHeader(oRedisCookie.Get<string>(sRdsKey));
                    request.CookieContainer.Add(oCookies);
                }
                else
                    request.CookieContainer.Add(GetCookie(oURL, true, oURL.Host, 5));

                //拼接头
                if (!String.IsNullOrEmpty(sAccept))
                    request.Accept = sAccept;
                else
                    request.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                if (!String.IsNullOrEmpty(sUserAgent))
                    request.UserAgent = sUserAgent;
                else
                    request.UserAgent = GetRandomUserAgent();

                request.Timeout = 20 * 1000;
                request.AllowAutoRedirect = true;
                request.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.ContentType = @"application/json";
                request.UserAgent = GetRandomUserAgent();
                request.Host = new Uri(url).Host;
                request.Method = "GET";
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, @"gzip, deflate br");
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, @"en-US,en;q=0.5");
                request.Headers.Add(HttpRequestHeader.CacheControl, @"max-age=0");
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");


                //sleep
                int nRand = oRand.Next(2, 80);
                Console.WriteLine("Net Request Sleeping: " + nRand + "ms");
                HttpWebResponse respone = (HttpWebResponse)request.GetResponse();
                if (respone.StatusCode == HttpStatusCode.OK)
                    return respone;
                else if (respone.StatusCode > HttpStatusCode.InternalServerError)
                    return ReadData(url, n++, sRedisKey, sAccept, sUserAgent);
                else
                    return ReadData(url, n++, sRedisKey, sAccept, sUserAgent);
            }
            catch (IOException e)
            {
                if (n > nSign)
                    throw e;
                else
                {
                    n++;
                    return ReadData(url, n, sRedisKey, sAccept, sUserAgent);
                }
            }
            catch (WebException e)
            {
                if (e.Message.Contains("40") || e.Message.Contains("50"))
                {
                    return null;
                }
                else
                {
                    if (n > nSign)
                        throw e;
                    else
                    {
                        n++;
                        return ReadData(url, n, sRedisKey, sAccept, sUserAgent);
                    }
                }

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("无效的控制字符"))
                {
                    if (n > nSign)
                        throw ex;
                    else
                    {
                        n++;
                        return ReadData(url, n, sRedisKey, sAccept, sUserAgent);
                    }
                }
                else
                    throw ex;
            }
        }

        /// <summary>
        /// Get
        /// 获取数据,需要判断返回结果是否为空
        /// </summary>
        /// <param name="url">Uri资源</param>
        /// <param name="oCookieCollection">Cookie容器</param>
        /// <param name="n">循环次数</param>
        /// <returns>响应信息</returns>
        public static HttpWebResponse ReadData(string url, CookieCollection oCookieCollection, int n = 0)
        {
            int nSign = 4;
            try
            {
                if (n >= nSign)
                    return null;

                Uri oURL = new Uri(url);
                Random oRand = new Random();

                //设置https验证方式
                if (url.ToString().StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                            new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                //获取Cookie
                if (request.CookieContainer == null)
                    request.CookieContainer = new CookieContainer();
                if (oCookieCollection != null)
                    request.CookieContainer.Add(oCookieCollection);

                System.Net.ServicePointManager.Expect100Continue = false;
                ServicePointManager.MaxServicePointIdleTime = 2000;

                request.Timeout = 20 * 1000;
                request.AllowAutoRedirect = true;
                request.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.ContentType = @"application/json";
                request.UserAgent = GetRandomUserAgent();
                request.Host = new Uri(url).Host;
                request.Method = "GET";
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, @"gzip, deflate br");
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, @"en-US,en;q=0.5");
                request.Headers.Add(HttpRequestHeader.CacheControl, @"max-age=0");
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                //sleep
                int nRand = oRand.Next(2, 80);
                Console.WriteLine("Net Request Sleeping: " + nRand + "ms");
                HttpWebResponse respone = (HttpWebResponse)request.GetResponse();
                if (respone.StatusCode == HttpStatusCode.OK)
                    return respone;
                else if (respone.StatusCode > HttpStatusCode.InternalServerError)
                {
                    n += 1;
                    return ReadData(url, oCookieCollection, n);
                }
                else
                {
                    n += 1;
                    return ReadData(url, oCookieCollection, n);
                }
            }
            catch (IOException e)
            {
                if (n > nSign)
                    throw e;
                else
                {
                    n += 1;
                    return ReadData(url, oCookieCollection, n);
                }
            }
            catch (WebException e)
            {
                if (e.Message.Contains("40") || e.Message.Contains("50"))
                {
                    return null;
                }
                else
                {
                    if (n > nSign)
                        throw e;
                    else
                    {
                        n += 1;
                        return ReadData(url, oCookieCollection, n);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("无效的控制字符"))
                {
                    if (n > nSign)
                        throw ex;
                    else
                    {
                        n += 1;
                        return ReadData(url, oCookieCollection, n);
                    }
                }
                else
                    throw ex;
            }
        }

        /// <summary>
        /// Get
        /// 获取数据,需要判断返回结果是否为空
        /// </summary>
        /// <param name="url"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static HttpWebResponse ReadData(string url, int n = 0)
        {
            return ReadData(url, n, null, null, null);
        }

        /// <summary>
        /// 获取指定Uri的Cookie信息
        /// </summary>
        /// <returns>返回的Cookie集合</returns>
        public static CookieCollection GetCookie(Uri url, bool bUseRedis = true, string sRedisKey = null, int? cacheTime = null)
        {
            try
            {
                //是否同步保存字符串形式的Cookies
                bool bStrCookies = true;
                CookieCollection oCookieCollection = new CookieCollection();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                RequestCachePolicy oRequestCachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

                //设置https验证方式
                if (url.ToString().StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                            new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                request.AllowAutoRedirect = true;
                request.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, @"gzip, deflate, br");
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, @"zh-CN,zh;q=0.9,en;q=0.8");
                request.Headers.Add(HttpRequestHeader.CacheControl, @"max-age=0");
                request.Headers.Add("DNT", "1");
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Safari/537.36";
                request.Host = url.Host;

                using (HttpWebResponse oHttpWebResponse = (HttpWebResponse)request.GetResponse())
                {
                    if (oHttpWebResponse.StatusCode == HttpStatusCode.OK)
                    {
                        //将Cookies添加到Cookie集合中
                        if (oHttpWebResponse.Cookies.Count == 0 && !string.IsNullOrEmpty(oHttpWebResponse.Headers.Get("Set-Cookie")))
                        {
                            var sCookies = CookieHelper.GetCookiesByHeader(oHttpWebResponse.Headers.Get("Set-Cookie"), url.Host);
                            oCookieCollection.Add(sCookies);
                        }
                        else
                            oCookieCollection.Add(oHttpWebResponse.Cookies);

                        //是否使用Redis
                        if (bUseRedis)
                        {
                            StackExchangeRedisHelper oRedisCookie = new StackExchangeRedisHelper(nRedisCookieDB);
                            string sRdsKey = string.IsNullOrEmpty(sRedisKey) ? url.Host : sRedisKey;

                            //仅在Redis中存字符串形式的Cookie
                            if (bStrCookies && !string.IsNullOrEmpty(oHttpWebResponse.Headers.Get("Set-Cookie")))
                            {
                                if (oRedisCookie.IsSet(sRdsKey))
                                    oRedisCookie.Del(sRdsKey);

                                if (cacheTime != null)
                                    oRedisCookie.Set(sRdsKey, oHttpWebResponse.Headers.Get("Set-Cookie"), (int)cacheTime);
                                else
                                    oRedisCookie.Set(sRdsKey, oHttpWebResponse.Headers.Get("Set-Cookie"));
                            }
                        }
                    }
                    else
                        return null;
                }
                return oCookieCollection;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 获取指定Uri的Cookie信息
        /// </summary>
        /// <returns>返回的Cookie集合</returns>
        public static void GetCookie(string url, string sRedisKey = null, int? cacheTime = null)
        {
            try
            {
                //是否同步保存字符串形式的Cookies
                bool bStrCookies = true;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                RequestCachePolicy oRequestCachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

                //设置https验证方式
                if (url.ToString().StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                            new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                System.Net.ServicePointManager.Expect100Continue = false;
                ServicePointManager.MaxServicePointIdleTime = 2000;

                request.Timeout = 20 * 1000;
                request.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.ContentType = @"application/json";
                request.UserAgent = GetRandomUserAgent();
                request.Host = new Uri(url).Host;
                request.Method = "GET";
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, @"gzip, deflate br");
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, @"en-US,en;q=0.5");
                request.Headers.Add(HttpRequestHeader.CacheControl, @"max-age=0");
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                using (HttpWebResponse oHttpWebResponse = (HttpWebResponse)request.GetResponse())
                {
                    if (oHttpWebResponse.StatusCode == HttpStatusCode.OK)
                    {
                        StackExchangeRedisHelper oRedisCookie = new StackExchangeRedisHelper(nRedisCookieDB);
                        string sRdsKey = string.IsNullOrEmpty(sRedisKey) ? new Uri(url).Host : sRedisKey;

                        //仅在Redis中存字符串形式的Cookie
                        if (bStrCookies && !string.IsNullOrEmpty(oHttpWebResponse.Headers.Get("Set-Cookie")))
                        {
                            if (oRedisCookie.IsSet(sRdsKey))
                                oRedisCookie.Del(sRdsKey);

                            if (cacheTime != null)
                                oRedisCookie.Set(sRdsKey, oHttpWebResponse.Headers.Get("Set-Cookie"), (int)cacheTime);
                            else
                                oRedisCookie.Set(sRdsKey, oHttpWebResponse.Headers.Get("Set-Cookie"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
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

        /// <summary>
        /// 处理http GET请求，返回数据
        /// </summary>
        /// <param name="url">请求的url地址</param>
        /// <returns>http GET成功后返回的数据，失败抛WebException异常</returns>
        public static string Get(Uri url, WebHeaderCollection oWebHead = null, CookieContainer oCookieContainer = null)
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

                if (oWebHead != null)
                    oRequest.Headers = oWebHead;
                if (oCookieContainer != null)
                    oRequest.CookieContainer = oCookieContainer;
                oRequest.Method = "GET";
                oRequest.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.108 Safari/537.36";
                oRequest.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
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
        /// 获取随机的User Agent
        /// </summary>
        /// <returns></returns>
        public static string GetRandomUserAgent()
        {
            Random oRandom = new Random();
            List<string> oList = new List<string> {
                 //@"Opera/9.80 (X11; Linux i686; U; ru) Presto/2.8.131 Version/11.11",
                 //@"Mozilla/5.0 (X11; Linux i686; U;) Gecko/20070322 Kazehakase/0.4.5",
                 @"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_3) AppleWebKit/535.20 (KHTML, like Gecko) Chrome/19.0.1036.7 Safari/535.20",
                 @"Mozilla/5.0 (X11; U; Linux i686; en-US; rv:1.9.0.8) Gecko Fedora/1.9.0.8-1.fc10 Kazehakase/0.5.6",
                 @"Mozilla/5.0 (X11; U; Linux x86_64; zh-CN; rv:1.9.2.10) Gecko/20100922 Ubuntu/10.10 (maverick) Firefox/3.6.10",
                 @"Mozilla/5.0 (X11; U; Linux i686; en-US; rv:1.8.0.12) Gecko/20070731 Ubuntu/dapper-security Firefox/1.5.0.12",
                 @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36",
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

    public class httph
    {
        //private Uri oUri;
        //private int nTimeout = 6 * 1000;
        //private int nLoopTimes = 0;
        //private CookieContainer oCookieContainer;

        ///// <summary>
        ///// 资源地址
        ///// </summary>
        //public Uri Uri { get => oUri; set => oUri = value; }

        ///// <summary>
        ///// 请求时所携带的Cookie
        ///// </summary>
        //public CookieContainer CookieContainer { get => oCookieContainer; set => oCookieContainer = value; }

        ///// <summary>
        ///// 重复请求次数
        ///// </summary>
        //public int LoopTimes { get => nLoopTimes; set => nLoopTimes = value; }

        ///// <summary>
        ///// 请求超时时间，ms
        ///// </summary>
        //public int NTimeout { get => nTimeout; set => nTimeout = value; }

        //public httph(CookieContainer oCookieContainer)
        //{
        //    this.CookieContainer = oCookieContainer;
        //}

        //public httph()
        //{
        //    this.CookieContainer = new CookieContainer();
        //    WebClient webClient = new WebClient();

        //}

        ///// <summary>
        ///// Get
        ///// 获取数据,需要判断返回结果是否为空
        ///// </summary>
        ///// <param name="url">Uri资源</param>
        ///// <param name="sAccept">Accept头</param>
        ///// <param name="sUserAgent">UserAgent头</param>
        ///// <param name="oWebHeaderCollection">请求头信息</param>
        ///// <param name="oCookieContainer">Cookie容器</param>
        ///// <returns>响应信息</returns>
        //public static HttpWebResponse ReadData(string url, int n = 0, string sRedisKey = null,
        //    string sAccept = null, string sUserAgent = null)
        //{
        //    int nSign = 4;
        //    try
        //    {
        //        if (n >= nSign)
        //            return null;

        //        Uri oURL = new Uri(url);
        //        Random oRand = new Random();
        //        string sRdsKey = string.IsNullOrEmpty(sRedisKey) ? oURL.Host : sRedisKey;
        //        StackExchangeRedisHelper oRedisCookie = new StackExchangeRedisHelper(nRedisCookieDB);

        //        //设置https验证方式
        //        if (url.ToString().StartsWith("https", StringComparison.OrdinalIgnoreCase))
        //        {
        //            ServicePointManager.ServerCertificateValidationCallback =
        //                    new RemoteCertificateValidationCallback(CheckValidationResult);
        //        }

        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

        //        request.AllowAutoRedirect = true;

        //        //获取Cookie，从Redis
        //        if (oRedisCookie.IsSet(sRdsKey))
        //        {
        //            var oCookies = CookieHelper.GetCookiesByHeader(oRedisCookie.Get<string>(sRdsKey));
        //            request.CookieContainer.Add(oCookies);
        //        }
        //        else
        //            request.CookieContainer.Add(GetCookie(oURL, true, oURL.Host, 5));

        //        if (!String.IsNullOrEmpty(sAccept))
        //            request.Accept = sAccept;
        //        else
        //            request.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
        //        if (!String.IsNullOrEmpty(sUserAgent))
        //            request.UserAgent = sUserAgent;
        //        else
        //            request.UserAgent = GetRandomUserAgent();
        //        request.Timeout = 20 * 1000;
        //        request.Headers.Add(HttpRequestHeader.CacheControl, @"no-cache");
        //        request.Headers.Add("upgrade-insecure-requests", "1");
        //        request.Headers.Add(HttpRequestHeader.Pragma, "no-cache");
        //        request.Headers.Add(HttpRequestHeader.AcceptEncoding, @"gzip, deflate, br");
        //        request.Referer = new Uri(url).Host.ToString();

        //        //sleep
        //        int nRand = oRand.Next(20, 80);
        //        Console.WriteLine("Net Request Sleeping: " + nRand + "ms");
        //        HttpWebResponse respone = (HttpWebResponse)request.GetResponse();
        //        if (respone.StatusCode == HttpStatusCode.OK)
        //            return respone;
        //        else if (respone.StatusCode > HttpStatusCode.InternalServerError)
        //            return ReadData(url, n++, sRedisKey, sAccept, sUserAgent);
        //        else
        //            return ReadData(url, n++, sRedisKey, sAccept, sUserAgent);
        //    }
        //    catch (IOException e)
        //    {
        //        if (n > nSign)
        //            throw e;
        //        else
        //        {
        //            n++;
        //            return ReadData(url, n, sRedisKey, sAccept, sUserAgent);
        //        }
        //    }
        //    catch (WebException e)
        //    {
        //        if (e.Message.Contains("40") || e.Message.Contains("50"))
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            if (n > nSign)
        //                throw e;
        //            else
        //            {
        //                n++;
        //                return ReadData(url, n, sRedisKey, sAccept, sUserAgent);
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message.Contains("无效的控制字符"))
        //        {
        //            if (n > nSign)
        //                throw ex;
        //            else
        //            {
        //                n++;
        //                return ReadData(url, n, sRedisKey, sAccept, sUserAgent);
        //            }
        //        }
        //        else
        //            throw ex;
        //    }
        //}

        ///// <summary>
        ///// 检查证书，适用于HTTPS
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="certificate"></param>
        ///// <param name="chain"></param>
        ///// <param name="errors"></param>
        ///// <returns></returns>
        //private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        //{
        //    return true; //总是接受   
        //}

    }

    /// <summary>
    /// 基于HttpClient实现的Http帮助类
    /// 线程安全，支持异步操作
    /// </summary>
    public class HttpClientHelper
    {
        private static Uri Uri;
        private static HttpClient client = null;
        private static readonly object LockObj = new object();

        /// <summary>
        /// 初始化HttpClientHelper对象
        /// </summary>
        /// <param name="sURI"></param>
        /// <param name="bPreload"></param>
        public HttpClientHelper(Uri sURI = null, bool bPreload = false)
        {
            try
            {
                GetInstance(sURI);

                //预热HttpClient
                if (Uri != null && bPreload)
                    client.SendAsync(new HttpRequestMessage
                    {
                        Method = HttpMethod.Head,
                        RequestUri = new Uri(Uri.AbsoluteUri + "/")
                    }).Result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 初始化HttpClientHelper对象
        /// </summary>
        /// <param name="sURI"></param>
        /// <param name="bPreload"></param>
        public HttpClientHelper(string sURI = null, bool bPreload = false)
        {
            try
            {
                GetInstance(sURI);

                //预热HttpClient
                if (Uri != null && bPreload)
                    client.SendAsync(new HttpRequestMessage
                    {
                        Method = HttpMethod.Head,
                        RequestUri = new Uri(Uri.AbsoluteUri + "/")
                    }).Result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 单例
        /// </summary>
        /// <param name="sURI"></param>
        /// <returns></returns>
        public static HttpClient GetInstance(string sURI = null)
        {
            try
            {
                return GetInstance(new Uri(sURI));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 单例
        /// </summary>
        /// <param name="sURI"></param>
        /// <returns></returns>
        public static HttpClient GetInstance(Uri sURI = null)
        {
            try
            {
                if (sURI == null)
                    throw new ArgumentNullException();

                if (client == null)
                {
                    lock (LockObj)
                    {
                        if (client == null)
                        {
                            client = new HttpClient();
                        }

                        if (Uri != null)
                            Uri = sURI;
                    }
                }
                return client;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> PostAsy(string url, HttpContent httpContent)
        {
            try
            {
                //由HttpClient发出异步Post请求
                HttpResponseMessage res = await client.PostAsync(url, httpContent);

                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                    return res.Content.ReadAsStringAsync().Result;
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Post异步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="strJson"></param>
        /// <returns></returns>
        public async Task<string> PostAsync(string url, string strJson)
        {
            try
            {
                HttpContent content = new StringContent(strJson);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                //由HttpClient发出异步Post请求
                HttpResponseMessage res = await client.PostAsync(url, content);
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string str = res.Content.ReadAsStringAsync().Result;
                    return str;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Post同步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="strJson"></param>
        /// <returns></returns>
        public string Post(string url, string strJson)//post同步请求方法
        {
            try
            {
                HttpContent content = new StringContent(strJson);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                //client.DefaultRequestHeaders.Connection.Add("keep-alive");
                //由HttpClient发出Post请求
                Task<HttpResponseMessage> res = client.PostAsync(url, content);
                if (res.Result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string str = res.Result.Content.ReadAsStringAsync().Result;
                    return str;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get同步请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string Get(string url)
        {
            try
            {
                var responseString = client.GetStringAsync(url);
                return responseString.Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

namespace Larpx.ResourceSpider.CommonHelper
{
    public class SmsWebClient : WebClient
    {
        public SmsWebClient(CookieContainer container, Dictionary<string, string> Headers)
        : this(container)
        {
            foreach (var keyVal in Headers)
            {
                this.Headers[keyVal.Key] = keyVal.Value;
            }
        }

        public SmsWebClient(bool flgAddContentType = true)
        : this(new CookieContainer(), flgAddContentType)
        {

        }

        public SmsWebClient(CookieContainer container, bool flgAddContentType = true)
        {
            this.Encoding = Encoding.UTF8;
            System.Net.ServicePointManager.Expect100Continue = false;
            ServicePointManager.MaxServicePointIdleTime = 2000;
            this.container = container;
            if (flgAddContentType)
                this.Headers["Content-Type"] = "application/json";//"application/x-www-form-urlencoded";
            this.Headers["Accept"] = "application/json, text/javascript, */*; q=0.01";//"text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //this.Headers["Accept-Encoding"] ="gzip, deflate";
            this.Headers["Accept-Language"] = "en-US,en;q=0.5";
            this.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.1; rv:23.0) Gecko/20100101 Firefox/23.0";
            this.Headers["X-Requested-With"] = "XMLHttpRequest";
            //this.Headers["Connection"] ="keep-alive";
        }

        private readonly CookieContainer container = new CookieContainer();
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            if (request != null)
            {
                request.CookieContainer = container;
                request.Timeout = 3600000;//20 * 60 * 1000
            }
            return r;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                container.Add(cookies);
            }
        }
    }
}

